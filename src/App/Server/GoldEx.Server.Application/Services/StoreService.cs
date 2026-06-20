using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.Constants;
using GoldEx.Sdk.Server.Application.Abstractions;
using GoldEx.Sdk.Server.Application.Exceptions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Application.Validators.Stores;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.SettingAggregate;
using GoldEx.Server.Domain.SmsTemplateAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Infrastructure;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Services.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Stores;
using GoldEx.Server.Infrastructure.Specifications.Settings;
using GoldEx.Server.Infrastructure.Specifications.SmsTemplates;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Specifications.Coins;
using GoldEx.Server.Infrastructure.Specifications.ProductCategories;
using GoldEx.Shared.DTOs.Stores;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class StoreService(
    IStoreRepository storeRepository,
    IStoreUserRepository storeUserRepository,
    IUserContext userContext,
    IStoreContext storeContext,
    IHttpContextAccessor httpContextAccessor,
    ISettingRepository settingRepository,
    ISmsTemplateRepository smsTemplateRepository,
    ILedgerAccountRepository ledgerAccountRepository,
    IFinancialAccountRepository financialAccountRepository,
    IPriceUnitRepository priceUnitRepository,
    ICoinRepository coinRepository,
    IProductCategoryRepository productCategoryRepository,
    IFileService fileService,
    IWebHostEnvironment webHostEnvironment,
    CreateStoreRequestValidator createValidator,
    UpdateStoreRequestValidator updateValidator,
    DeleteStoreValidator deleteValidator) : IStoreService
{
    public async Task<List<UserStoreDto>> GetUserStoresAsync(CancellationToken cancellationToken = default)
    {
        var userId = userContext.GetUserId() ?? throw new UnauthorizedAccessException();

        var storeUsers = await storeUserRepository.Get(new StoreUserByUserIdSpecification(userId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var storeIds = storeUsers.Select(x => x.StoreId).ToList();

        var stores = await storeRepository.Get(new StoresByIdsSpecification(storeIds))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return stores.Select(s => new UserStoreDto
        {
            Id = s.Id.Value,
            Name = s.Name,
            Slug = s.Slug,
            LogoUrl = s.LogoUrl,
            BackgroundImageUrl = s.BackgroundImageUrl,
            IsDefault = storeUsers.First(su => su.StoreId == s.Id).IsDefault,
            IsCurrent = s.Id.Value == storeContext.StoreId
        }).ToList();
    }

    public async Task SwitchStoreAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        var userId = userContext.GetUserId() ?? throw new UnauthorizedAccessException();

        var storeUser = await storeUserRepository.Get(new StoreUserByAccessSpecification(userId, new StoreId(storeId)))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (storeUser == null)
            throw new ForbiddenException("User does not have access to this store.");

        var store = await storeRepository.Get(new StoreByIdSpecification(new StoreId(storeId)))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (store == null)
            throw new NotFoundException("Store not found or inactive.");

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            httpContext.Response.Cookies.Append("ActiveStoreId", storeId.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });
        }
    }

    public async Task<PagedList<GetStoreRequest>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        var query = storeRepository.Get(new StoresByFilterSpecification(filter)).AsNoTracking();
        var list = await query.ToListAsync(cancellationToken);
        var total = await storeRepository.CountAsync(new StoresByFilterSpecification(filter), cancellationToken);

        return new PagedList<GetStoreRequest>
        {
            Data = list.Select(s => new GetStoreRequest
            {
                Id = s.Id.Value,
                Name = s.Name,
                Slug = s.Slug,
                LogoUrl = s.LogoUrl,
                BackgroundImageUrl = s.BackgroundImageUrl,
                IsActive = s.IsActive
            }).ToList(),
            Total = total,
            Skip = filter.Skip ?? 0,
            Take = filter.Take ?? 10
        };
    }

    public async Task CreateStoreAsync(StoreRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        await using var transaction = await storeRepository.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken);
        try
        {
            var store = Store.Create(request.Name, request.Slug);

            var logoUrl = store.LogoUrl;
            var backgroundImageUrl = store.BackgroundImageUrl;

            if (request.LogoContent is { Length: > 0 })
            {
                var ext = string.IsNullOrWhiteSpace(request.LogoExtension) ? "png" : request.LogoExtension;
                var relativePath = $"uploads/stores/{store.Id.Value}/logo.{ext}";
                await fileService.SaveLocalFileAsync(relativePath, request.LogoContent, cancellationToken);
                logoUrl = $"/{relativePath}";
            }

            if (request.BackgroundImageContent is { Length: > 0 })
            {
                var ext = string.IsNullOrWhiteSpace(request.BackgroundImageExtension) ? "jpg" : request.BackgroundImageExtension;
                var relativePath = $"uploads/stores/{store.Id.Value}/background.{ext}";
                await fileService.SaveLocalFileAsync(relativePath, request.BackgroundImageContent, cancellationToken);
                backgroundImageUrl = $"/{relativePath}";
            }

            store.UpdateDetails(request.Name, request.Slug, logoUrl, backgroundImageUrl);
            store.SetActive(request.IsActive);

            await storeRepository.CreateAsync(store, cancellationToken);

            var defaultStoreId = new StoreId(Guid.Empty);
            var newStoreId = new StoreId(store.Id.Value);

            // 1. Settings cloning
            var defaultSetting = await settingRepository.Get(new SettingsByStoreIdSpecification(defaultStoreId))
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(cancellationToken);
            if (defaultSetting != null)
            {
                var newSetting = defaultSetting.Clone(newStoreId);
                settingRepository.Create(newSetting);
            }

            // 2. SmsTemplates cloning
            var defaultSmsTemplates = await smsTemplateRepository.Get(new SmsTemplatesByStoreIdSpecification(defaultStoreId))
                .IgnoreQueryFilters()
                .ToListAsync(cancellationToken);
            foreach (var defaultTemp in defaultSmsTemplates)
            {
                var newTemp = SmsTemplate.Create(
                    defaultTemp.Subject,
                    defaultTemp.Body,
                    defaultTemp.Parameters,
                    newStoreId
                );
                newTemp.SetStatus(defaultTemp.IsActive);
                smsTemplateRepository.Create(newTemp);
            }

            // 3. LedgerAccounts cloning (recursively mapping parents)
            var defaultLedgerAccounts = await ledgerAccountRepository.Get(new LedgerAccountsByStoreIdSpecification(defaultStoreId))
                .IgnoreQueryFilters()
                .ToListAsync(cancellationToken);

            var ledgerIdMap = new Dictionary<LedgerAccountId, LedgerAccountId>();
            var remainingAccounts = new List<LedgerAccount>(defaultLedgerAccounts);

            while (remainingAccounts.Count > 0)
            {
                var toProcess = remainingAccounts
                    .Where(a => a.ParentAccountId == null || ledgerIdMap.ContainsKey(a.ParentAccountId.Value))
                    .ToList();

                if (toProcess.Count == 0)
                {
                    break;
                }

                foreach (var account in toProcess)
                {
                    LedgerAccountId? newParentId = null;
                    if (account.ParentAccountId.HasValue)
                    {
                        newParentId = ledgerIdMap[account.ParentAccountId.Value];
                    }

                    var newAccount = LedgerAccount.CreateSystemAccount(
                        account.Title,
                        account.AccountType,
                        newParentId,
                        account.PriceUnitId,
                        newStoreId
                    );

                    ledgerAccountRepository.Create(newAccount);
                    ledgerIdMap[account.Id] = newAccount.Id;
                    remainingAccounts.Remove(account);
                }
            }

            // 4. Create default Financial Accounts (Gold and Cash) for the new store
            var defaultGoldLedger = defaultLedgerAccounts.FirstOrDefault(la => la.Title == SystemLedgerAccounts.MoltenGoldInventory);
            if (defaultGoldLedger != null && ledgerIdMap.TryGetValue(defaultGoldLedger.Id, out var newGoldLedgerId))
            {
                var goldPriceUnit = await priceUnitRepository
                    .Get(new PriceUnitsByUnitTypeSpecification(UnitType.Gold18K))
                    .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new NotFoundException("Gold price unit is not initialized");

                var newGoldFA = FinancialAccount.CreateSystemAccount(
                    null,
                    null,
                    FinancialAccountType.Gold,
                    goldPriceUnit.Id,
                    newGoldLedgerId,
                    storeId: newStoreId
                );
                financialAccountRepository.Create(newGoldFA);
            }

            var defaultCashLedger = defaultLedgerAccounts.FirstOrDefault(la => la.Title == SystemLedgerAccounts.InternalCashAccounts);
            if (defaultCashLedger != null && ledgerIdMap.TryGetValue(defaultCashLedger.Id, out var newCashLedgerId))
            {
                var cashPriceUnit = await priceUnitRepository
                    .Get(new PriceUnitsByTitleSpecification(UnitType.TMN.GetDisplayName()))
                    .FirstOrDefaultAsync(cancellationToken)
                    ?? await priceUnitRepository.Get(new PriceUnitsByUnitTypeSpecification(UnitType.Gold18K)).FirstOrDefaultAsync(cancellationToken)
                    ?? throw new NotFoundException("Default cash price unit is not initialized");

                var newCashFA = FinancialAccount.CreateSystemAccount(
                    null,
                    null,
                    FinancialAccountType.Cash,
                    cashPriceUnit.Id,
                    newCashLedgerId,
                    cashAccount: CashAccount.Create(null, CashAccountType.Internal),
                    storeId: newStoreId
                );
                financialAccountRepository.Create(newCashFA);
            }

            // 5. Coins cloning (mapping LedgerAccountId)
            var defaultCoins = await coinRepository.Get(new CoinsByStoreIdSpecification(defaultStoreId))
                .IgnoreQueryFilters()
                .ToListAsync(cancellationToken);

            foreach (var coin in defaultCoins)
            {
                if (ledgerIdMap.TryGetValue(coin.LedgerAccountId, out var newLedgerId))
                {
                    var newCoin = Coin.Create(
                        coin.Title,
                        coin.Weight,
                        coin.Fineness,
                        coin.StartMintYear,
                        coin.EndMintYear,
                        newLedgerId,
                        coin.PriceId,
                        newStoreId
                    );
                    newCoin.SetStatus(coin.IsActive);
                    coinRepository.Create(newCoin);
                }
            }

            // 6. ProductCategories cloning
            var defaultCategories = await productCategoryRepository.Get(new ProductCategoriesByStoreIdSpecification(defaultStoreId))
                .IgnoreQueryFilters()
                .ToListAsync(cancellationToken);

            foreach (var category in defaultCategories)
            {
                var newCategory = ProductCategory.Create(
                    category.Title,
                    category.PrefixCode,
                    newStoreId
                );
                productCategoryRepository.Create(newCategory);
            }

            await storeRepository.SaveAsync(cancellationToken);

            // File operations copying (Logo & Report)
            var defaultStore = await storeRepository.Get(new StoreByIdSpecification(defaultStoreId))
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(cancellationToken);
            var defaultSlug = defaultStore?.Slug ?? "default";
            var newSlug = store.Slug;

            try
            {
                // App Logo copy
                var appIconDir = webHostEnvironment.GetAppIconDirectory();
                var defaultIconPath = webHostEnvironment.GetAppIconPath(defaultSlug);
                if (!File.Exists(defaultIconPath))
                {
                    defaultIconPath = webHostEnvironment.GetAppIconPath(null);
                }
                var newIconPath = webHostEnvironment.GetAppIconPath(newSlug);
                if (File.Exists(defaultIconPath) && !File.Exists(newIconPath))
                {
                    if (!Directory.Exists(appIconDir))
                    {
                        Directory.CreateDirectory(appIconDir);
                    }
                    File.Copy(defaultIconPath, newIconPath);
                }

                // Invoice Report copy
                var reportsDir = webHostEnvironment.GetReportsDirectory();
                var defaultReportPath = webHostEnvironment.GetStoreReportPath("InvoiceReport", defaultSlug);
                if (!File.Exists(defaultReportPath))
                {
                    defaultReportPath = webHostEnvironment.GetReportPath("InvoiceReport");
                }
                var newReportPath = webHostEnvironment.GetStoreReportPath("InvoiceReport", newSlug);
                if (File.Exists(defaultReportPath) && !File.Exists(newReportPath))
                {
                    if (!Directory.Exists(reportsDir))
                    {
                        Directory.CreateDirectory(reportsDir);
                    }
                    File.Copy(defaultReportPath, newReportPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying default store assets: {ex.Message}");
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateStoreAsync(Guid id, StoreRequest request, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync((id, request), cancellationToken);

        var store = await storeRepository.Get(new StoreByIdAnyStatusSpecification(new StoreId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        var oldSlug = store.Slug;
        var newSlug = request.Slug.ToLowerInvariant().Trim();

        var logoUrl = store.LogoUrl;
        var backgroundImageUrl = store.BackgroundImageUrl;

        if (request.LogoContent is { Length: > 0 })
        {
            var ext = string.IsNullOrWhiteSpace(request.LogoExtension) ? "png" : request.LogoExtension;
            var relativePath = $"uploads/stores/{store.Id.Value}/logo.{ext}";

            if (!string.IsNullOrWhiteSpace(store.LogoUrl))
            {
                var oldPath = Path.Combine(webHostEnvironment.ContentRootPath, store.LogoUrl.TrimStart('/'));
                fileService.DeleteLocalFile(oldPath);
            }

            await fileService.SaveLocalFileAsync(relativePath, request.LogoContent, cancellationToken);
            logoUrl = $"/{relativePath}";
        }

        if (request.BackgroundImageContent is { Length: > 0 })
        {
            var ext = string.IsNullOrWhiteSpace(request.BackgroundImageExtension) ? "jpg" : request.BackgroundImageExtension;
            var relativePath = $"uploads/stores/{store.Id.Value}/background.{ext}";

            if (!string.IsNullOrWhiteSpace(store.BackgroundImageUrl))
            {
                var oldPath = Path.Combine(webHostEnvironment.ContentRootPath, store.BackgroundImageUrl.TrimStart('/'));
                fileService.DeleteLocalFile(oldPath);
            }

            await fileService.SaveLocalFileAsync(relativePath, request.BackgroundImageContent, cancellationToken);
            backgroundImageUrl = $"/{relativePath}";
        }

        store.UpdateDetails(request.Name, request.Slug, logoUrl, backgroundImageUrl);
        store.SetActive(request.IsActive);

        await storeRepository.UpdateAsync(store, cancellationToken);

        // Rename assets if slug changed
        if (!string.Equals(oldSlug, newSlug, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                // 1. Rename App Logo
                var appIconDir = webHostEnvironment.GetAppIconDirectory();
                var oldLogoPath = webHostEnvironment.GetAppIconPath(oldSlug);
                var newLogoPath = webHostEnvironment.GetAppIconPath(newSlug);
                if (File.Exists(oldLogoPath))
                {
                    if (File.Exists(newLogoPath))
                    {
                        File.Delete(newLogoPath);
                    }
                    File.Move(oldLogoPath, newLogoPath);
                }

                // 2. Rename Report Files
                var reportsDir = webHostEnvironment.GetReportsDirectory();
                if (Directory.Exists(reportsDir))
                {
                    var oldSuffix = $"_{oldSlug}.repx";
                    var newSuffix = $"_{newSlug}.repx";
                    var reportFiles = Directory.GetFiles(reportsDir, $"*{oldSuffix}");
                    foreach (var file in reportFiles)
                    {
                        var fileName = Path.GetFileName(file);
                        var newFileName = fileName.Substring(0, fileName.Length - oldSuffix.Length) + newSuffix;
                        var newFilePath = Path.Combine(reportsDir, newFileName);

                        if (File.Exists(newFilePath))
                        {
                            File.Delete(newFilePath);
                        }
                        File.Move(file, newFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error renaming store assets from {oldSlug} to {newSlug}: {ex.Message}");
            }
        }
    }

    public async Task DeleteStoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await deleteValidator.ValidateAndThrowAsync(id, cancellationToken);

        var storeIdValue = new StoreId(id);

        var store = await storeRepository.Get(new StoreByIdAnyStatusSpecification(storeIdValue))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        var slug = store.Slug;

        // Delete associated configurations programmatically in dependency order
        // 1. Financial Accounts
        var financialAccounts = await financialAccountRepository.Get(new FinancialAccountsByStoreIdSpecification(storeIdValue))
            .ToListAsync(cancellationToken);
        if (financialAccounts.Any())
            await financialAccountRepository.DeleteRangeAsync(financialAccounts, cancellationToken);

        // 2. Coins
        var coins = await coinRepository.Get(new CoinsByStoreIdSpecification(storeIdValue))
            .ToListAsync(cancellationToken);
        if (coins.Any())
            await coinRepository.DeleteRangeAsync(coins, cancellationToken);

        // 3. Ledger Accounts
        var ledgerAccounts = await ledgerAccountRepository.Get(new LedgerAccountsByStoreIdSpecification(storeIdValue))
            .ToListAsync(cancellationToken);
        if (ledgerAccounts.Any())
            await ledgerAccountRepository.DeleteRangeAsync(ledgerAccounts, cancellationToken);

        // 4. Settings
        var settings = await settingRepository.Get(new SettingsByStoreIdSpecification(storeIdValue))
            .ToListAsync(cancellationToken);
        if (settings.Any())
            await settingRepository.DeleteRangeAsync(settings, cancellationToken);

        // 5. SMS Templates
        var smsTemplates = await smsTemplateRepository.Get(new SmsTemplatesByStoreIdSpecification(storeIdValue))
            .ToListAsync(cancellationToken);
        if (smsTemplates.Any())
            await smsTemplateRepository.DeleteRangeAsync(smsTemplates, cancellationToken);

        // 6. Product Categories
        var productCategories = await productCategoryRepository.Get(new ProductCategoriesByStoreIdSpecification(storeIdValue))
            .ToListAsync(cancellationToken);
        if (productCategories.Any())
            await productCategoryRepository.DeleteRangeAsync(productCategories, cancellationToken);

        // Delete associated StoreUsers
        var storeUsers = await storeUserRepository
            .Get(new StoreUserByStoreIdSpecification(storeIdValue))
            .ToListAsync(cancellationToken);

        if (storeUsers.Any()) 
            await storeUserRepository.DeleteRangeAsync(storeUsers, cancellationToken);

        // Delete store files folder
        var storeDir = Path.Combine(webHostEnvironment.ContentRootPath, "uploads", "stores", id.ToString());
        if (Directory.Exists(storeDir))
        {
            try
            {
                Directory.Delete(storeDir, true);
            }
            catch (Exception)
            {
                // Ignore folder deletion errors
            }
        }

        // Delete app logo if exists
        var logoPath = webHostEnvironment.GetAppIconPath(slug);
        if (File.Exists(logoPath))
        {
            try
            {
                File.Delete(logoPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting store logo: {ex.Message}");
            }
        }

        // Delete associated reports
        var reportsDir = webHostEnvironment.GetReportsDirectory();
        if (Directory.Exists(reportsDir))
        {
            try
            {
                var suffix = $"_{slug}.repx";
                var reportFiles = Directory.GetFiles(reportsDir, $"*{suffix}");
                foreach (var file in reportFiles)
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting store reports: {ex.Message}");
            }
        }

        await storeRepository.DeleteAsync(store, cancellationToken);
    }

    public async Task<List<Guid>> GetStoreUsersAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        var mappings = await storeUserRepository.Get(new StoreUserByStoreIdSpecification(new StoreId(storeId)))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return mappings.Select(x => x.UserId).ToList();
    }

    public async Task AssignStoreUsersAsync(Guid storeId, AssignStoreUsersRequest request, CancellationToken cancellationToken = default)
    {
        var storeIdObj = new StoreId(storeId);

        var storeExists = await storeRepository.ExistsAsync(new StoreByIdAnyStatusSpecification(storeIdObj), cancellationToken);
        if (!storeExists)
            throw new NotFoundException("فروشگاه مورد نظر یافت نشد.");

        var existingMappings = await storeUserRepository.Get(new StoreUserByStoreIdSpecification(storeIdObj))
            .ToListAsync(cancellationToken);

        var existingUserIds = existingMappings.Select(x => x.UserId).ToHashSet();
        var incomingUserIds = request.UserIds.ToHashSet();

        var mappingsToAdd = incomingUserIds.Where(uid => !existingUserIds.Contains(uid))
            .Select(uid => StoreUser.Create(storeIdObj, uid, isDefault: false))
            .ToList();

        var mappingsToRemove = existingMappings.Where(m => !incomingUserIds.Contains(m.UserId)).ToList();

        if (!mappingsToRemove.Any() && !mappingsToAdd.Any())
            return;

        await using var transaction = await storeUserRepository.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken);
        try
        {
            if (mappingsToRemove.Any())
            {
                await storeUserRepository.DeleteRangeAsync(mappingsToRemove, cancellationToken);
            }

            if (mappingsToAdd.Any())
            {
                await storeUserRepository.CreateRangeAsync(mappingsToAdd, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
