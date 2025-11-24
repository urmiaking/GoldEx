using GoldEx.Server.Domain.BlogCategoryAggregate;
using GoldEx.Server.Domain.BlogPostAggregate;
using GoldEx.Server.Infrastructure.Models.Blogs;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Blogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.BackgroundServices;

public class BlogDiskSyncManager(
    IBlogFileRepository fileRepo,
    IServiceScopeFactory scopeFactory,
    ILogger<BlogDiskSyncManager> logger,
    IHostEnvironment hostEnvironment,
    IConfiguration config)
    : BackgroundService
{ 
    // Config
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(config.GetValue<int?>("BlogSync:PollSeconds") ?? 30);
    private readonly string _rootPath = config["BlogSync:ContentRoot"] ?? "shared";
    private readonly TimeSpan _debounceDelay = TimeSpan.FromSeconds(2); // Increased debounce to be safe

    // State
    private FileSystemWatcher? _watcher;
    private readonly SemaphoreSlim _syncLock = new(1, 1); // Ensure only one sync runs at a time
    private volatile bool _changesDetected;

    // To prevent infinite loops where this service writes to disk -> watcher fires -> sync runs -> writes to disk
    private bool _isInternalWriteOp;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        InitWatcher();

        // Initial Sync on startup
        await RunSyncSafeAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                bool shouldSync = false;

                // Check flag set by FileSystemWatcher
                if (_changesDetected)
                {
                    _changesDetected = false;
                    logger.LogInformation("FileSystem change detected. Debouncing...");

                    // Debounce: wait to see if more files are written (e.g. if copying many files)
                    await Task.Delay(_debounceDelay, stoppingToken);
                    shouldSync = true;
                }

                // If watcher triggered OR it's time for the periodic poll
                // (Polling is still needed as a fallback for FSW failures on network shares)
                if (shouldSync)
                {
                    await RunSyncSafeAsync(stoppingToken);
                }
                else
                {
                    // Standard polling wait
                    await Task.Delay(_pollInterval, stoppingToken);
                    // Run sync after poll interval
                    await RunSyncSafeAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Critical error in BlogDiskSyncManager loop.");
                // Wait a bit before retrying to avoid log spamming in crash loops
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task RunSyncSafeAsync(CancellationToken token)
    {
        // Prevent overlapping syncs
        if (!await _syncLock.WaitAsync(0, token))
        {
            // Lock currently held, skip this iteration
            return;
        }

        try
        {
            logger.LogDebug("Starting Blog Sync...");

            // !IMPORTANT!: Create a NEW SCOPE for every sync execution.
            // Repositories are Scoped, BackgroundService is Singleton.
            using var scope = scopeFactory.CreateScope();
            var categoryRepo = scope.ServiceProvider.GetRequiredService<IBlogCategoryRepository>();
            var postRepo = scope.ServiceProvider.GetRequiredService<IBlogPostRepository>();

            await SyncLogicAsync(categoryRepo, postRepo, token);

            logger.LogDebug("Blog Sync Completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing sync logic.");
        }
        finally
        {
            _syncLock.Release();
        }
    }

    private async Task SyncLogicAsync(
    IBlogCategoryRepository categoryRepo,
    IBlogPostRepository postRepo,
    CancellationToken token)
    {
        // 1. Fetch External State (Files)
        var fileCategories = (await fileRepo.ReadAllCategoriesAsync(token)).ToList();
        var filePosts = (await fileRepo.ReadAllPostsAsync(token)).ToDictionary(p => p.Id);

        // 2. Fetch Internal State (DB)
        var dbCategories = await categoryRepo.Get(new BlogCategoriesEmptySpecifications()).ToListAsync(token);
        var dbPosts = await postRepo.Get(new BlogPostsDefaultSpecification()).ToListAsync(token);

        var dbCatDict = dbCategories.ToDictionary(c => c.Id.Value);
        var dbPostDict = dbPosts.ToDictionary(p => p.Id.Value);

        // ==============================================================================
        // PHASE 1: CATEGORIES (File -> DB)
        // ==============================================================================

        // Step 1.A: Upsert Entities (Ignore Hierarchy for now)
        foreach (var fileCat in fileCategories)
        {
            if (!dbCatDict.TryGetValue(fileCat.Id, out var dbCat))
            {
                logger.LogInformation("Importing new Category from File: {Id}", fileCat.Id);
                // Create with NULL parent initially to avoid foreign key errors if parent doesn't exist yet
                var entity = BlogCategory.Hydrate(
                    fileCat.Id, fileCat.Title, null, fileCat.CreatedAt, fileCat.LastUpdated);

                if (entity.IsActive != fileCat.IsActive) entity.SetStatus(fileCat.IsActive);

                await categoryRepo.CreateAsync(entity, token);
                dbCatDict[fileCat.Id] = entity;
            }
            else
            {
                // Only update Title/Status if file is actually newer
                if (IsFileNewer(fileCat.LastUpdated, dbCat.LastUpdated ?? dbCat.CreatedAt))
                {
                    logger.LogInformation("Updating Category Body from File: {Id}", fileCat.Id);
                    dbCat.SetTitle(fileCat.Title);
                    dbCat.SetStatus(fileCat.IsActive);
                    await categoryRepo.UpdateAsync(dbCat, token);
                }
            }
        }

        // Step 1.B: Resolve Hierarchy (File -> DB) - THE FIX IS HERE
        // We iterate ALL file categories. If the file has a parent, we force the DB to match.
        // We do NOT check "IsFileNewer" here. If the file structure dictates a parent, we enforce it.
        foreach (var fileCat in fileCategories)
        {
            if (dbCatDict.TryGetValue(fileCat.Id, out var dbCat))
            {
                BlogCategoryId? targetParentId = fileCat.ParentCategoryId.HasValue
                    ? new BlogCategoryId(fileCat.ParentCategoryId.Value)
                    : null;

                // Integrity Check: If file refers to a parent that does NOT exist in our (now synced) DB, force to root.
                if (targetParentId != null && !dbCatDict.ContainsKey(targetParentId.Value.Value))
                {
                    logger.LogWarning("Category {Id} refers to missing parent {ParentId}. Forcing to Root.", fileCat.Id, fileCat.ParentCategoryId);
                    targetParentId = null;
                }

                // If the DB parent is different from the File parent, update the DB.
                if (dbCat.ParentCategoryId != targetParentId)
                {
                    logger.LogInformation("Linking Parent for {Id}: {OldParent} -> {NewParent}",
                        fileCat.Id, dbCat.ParentCategoryId?.Value, targetParentId?.Value);

                    dbCat.SetParent(targetParentId);
                    await categoryRepo.UpdateAsync(dbCat, token);
                }
            }
        }

        // ==============================================================================
        // PHASE 2: CATEGORIES (DB -> File)
        // ==============================================================================
        // CRITICAL: Refresh DB List to get the updates from Phase 1 (especially the Parent linkages)
        dbCategories = await categoryRepo.Get(new BlogCategoriesEmptySpecifications()).ToListAsync(token);

        foreach (var dbCat in dbCategories)
        {
            var fileCat = fileCategories.FirstOrDefault(x => x.Id == dbCat.Id.Value);

            var dbDto = new BlogCategoryDto(
                dbCat.Id.Value,
                dbCat.Title,
                dbCat.ParentCategoryId?.Value, // This will now be correct
                dbCat.CreatedAt,
                dbCat.LastUpdated ?? dbCat.CreatedAt,
                dbCat.IsActive
            );

            // Logic:
            // 1. If missing on disk -> Write
            // 2. If exists, only write if DB is significantly newer
            bool shouldWrite = fileCat == null || IsDbNewer(dbDto.LastUpdated, fileCat.LastUpdated);

            if (shouldWrite)
            {
                // Safety: Check content equality to prevent loops if timestamps are just slightly off
                if (fileCat != null &&
                    fileCat.Title == dbDto.Title &&
                    fileCat.ParentCategoryId == dbDto.ParentCategoryId &&
                    fileCat.IsActive == dbDto.IsActive)
                {
                    continue;
                }

                logger.LogInformation("Exporting Category to File: {Id}", dbCat.Id.Value);
                await WriteToFileSafeAsync(() => fileRepo.SaveCategoryAsync(dbDto, token));
            }
        }

        // ==============================================================================
        // PHASE 3 & 4: POSTS (Same as before)
        // ==============================================================================
        foreach (var kv in filePosts)
        {
            var filePost = kv.Value;
            if (!dbCatDict.ContainsKey(filePost.CategoryId)) continue;

            if (!dbPostDict.TryGetValue(filePost.Id, out var dbPost))
            {
                var entity = BlogPost.Hydrate(
                    filePost.Id, filePost.Title, filePost.Slug, filePost.Content,
                    new BlogCategoryId(filePost.CategoryId),
                    filePost.CreatedAt, filePost.LastUpdated, filePost.IsActive
                );
                await postRepo.CreateAsync(entity, token);
                dbPostDict[filePost.Id] = entity;
            }
            else if (IsFileNewer(filePost.LastUpdated, dbPost.LastUpdated ?? dbPost.CreatedAt))
            {
                dbPost.UpdateContent(filePost.Title, filePost.Slug, filePost.Content);
                dbPost.SetStatus(filePost.IsActive);
                if (dbPost.BlogCategoryId.Value != filePost.CategoryId)
                {
                    // Assuming you have a method/logic to update category
                    // dbPost.SetCategory(new BlogCategoryId(filePost.CategoryId));
                }
                await postRepo.UpdateAsync(dbPost, token);
            }
        }

        foreach (var dbPost in dbPosts)
        {
            var dto = new BlogPostDto(
                dbPost.Id.Value,
                dbPost.Title,
                dbPost.Slug,
                dbPost.Content,
                dbPost.BlogCategoryId.Value,
                dbPost.CreatedAt,
                dbPost.LastUpdated ?? dbPost.CreatedAt,
                dbPost.IsActive 
            );

            bool fileExists = filePosts.TryGetValue(dbPost.Id.Value, out var filePost);
            bool shouldWrite = !fileExists || IsDbNewer(dto.LastUpdated, filePost!.LastUpdated);

            if (shouldWrite) 
            {
                if (fileExists && ArePostsContentEqual(dto, filePost!)) continue;

                logger.LogInformation("Exporting Post to File: {Id}", dbPost.Id.Value);
                await WriteToFileSafeAsync(() => fileRepo.SavePostAsync(dto, token));
            }
        }
    }

    // Helper to handle the "Ping Pong" timestamp issue
    private bool IsFileNewer(DateTimeOffset fileTime, DateTimeOffset dbTime)
    {
        // File time must be > DB time + tolerance
        return fileTime > dbTime.AddSeconds(1);
    }

    private bool IsDbNewer(DateTimeOffset dbTime, DateTimeOffset fileTime)
    {
        return dbTime > fileTime.AddSeconds(1);
    }

    private bool ArePostsContentEqual(BlogPostDto a, BlogPostDto b)
    {
        // Simple check to avoid IO if data is practically identical but timestamp differs slightly
        return a.Title == b.Title &&
               a.Content == b.Content &&
               a.Slug == b.Slug &&
               a.CategoryId == b.CategoryId &&
               a.IsActive == b.IsActive;
    }

    // Wrapper to suppress FileSystemWatcher events caused by ourselves
    private async Task WriteToFileSafeAsync(Func<Task> writeAction)
    {
        try
        {
            _isInternalWriteOp = true;
            await writeAction();
        }
        finally
        {
            // Keep the flag true for a moment to let OS events fire and be ignored
            // We can't await here easily without blocking, but usually events fire immediately.
            // The debounce in OnFileChanged handles the slight delay.
            _isInternalWriteOp = false;
        }
    }

    public Task NotifyLocalPostChangeAsync(BlogPost post)
    {
        // This method is called by the UI/API when a user saves.
        // We trigger an immediate sync (or direct write)
        _changesDetected = true;
        // The background loop will pick this up and perform the SyncOutbound
        return Task.CompletedTask;
    }

    public Task NotifyLocalCategoryChangeAsync(BlogCategory category)
    {
        _changesDetected = true;
        return Task.CompletedTask;
    }

    private void InitWatcher()
    {
        try
        {
            var root = Path.Combine(hostEnvironment.ContentRootPath, _rootPath);

            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);

            _watcher = new FileSystemWatcher(root)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
            };

            _watcher.Changed += OnFileChanged;
            _watcher.Created += OnFileChanged;
            _watcher.Renamed += OnFileChanged;
            // Deletions are tricky in sync, ignoring for now or handle carefully

            _watcher.EnableRaisingEvents = true;
            logger.LogInformation("BlogDiskSyncManager watcher initialized at: {Path}", root);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize FileSystemWatcher. Will rely on polling.");
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // If we are currently writing files, ignore this event
        if (_isInternalWriteOp) return;

        lock (_syncLock)
        {
            _changesDetected = true;
        }
    }
}
