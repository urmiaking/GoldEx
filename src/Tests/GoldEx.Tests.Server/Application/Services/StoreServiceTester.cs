using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GoldEx.Server.Application.Services;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.SettingAggregate;
using GoldEx.Server.Domain.SmsTemplateAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Services.Abstractions;
using GoldEx.Shared.DTOs.Stores;
using GoldEx.Sdk.Server.Application.Abstractions;
using GoldEx.Shared.Services.Abstractions;
using GoldEx.Shared.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using GoldEx.Server.Application.Validators.Stores;
using System.Data;
using GoldEx.Sdk.Server.Infrastructure.Specifications;

namespace GoldEx.Tests.Server.Application.Services;

[TestFixture]
public class StoreServiceTester
{
    private DbContextOptions<GoldExDbContext> _dbContextOptions;
    private Mock<IStoreRepository> _storeRepoMock;
    private Mock<IStoreUserRepository> _storeUserRepoMock;
    private Mock<IUserContext> _userContextMock;
    private TestStoreContext _storeContext;
    private Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private Mock<IFileService> _fileServiceMock;
    private Mock<IWebHostEnvironment> _webHostEnvironmentMock;

    private class TestStoreContext : IStoreContext
    {
        public Guid? StoreId { get; set; }
        public string? StoreSlug { get; set; }
    }

    [SetUp]
    public void SetUp()
    {
        _dbContextOptions = new DbContextOptionsBuilder<GoldExDbContext>()
            .UseInMemoryDatabase(databaseName: $"GoldEx_StoreTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _storeRepoMock = new Mock<IStoreRepository>();
        _storeUserRepoMock = new Mock<IStoreUserRepository>();
        _userContextMock = new Mock<IUserContext>();
        _storeContext = new TestStoreContext { StoreId = Guid.Empty, StoreSlug = "default" };
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _fileServiceMock = new Mock<IFileService>();
        _webHostEnvironmentMock = new Mock<IWebHostEnvironment>();

        _webHostEnvironmentMock.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());
    }

    [Test]
    public async Task CreateStoreAsync_ClonesDefaultStoreConfigurations()
    {
        // Arrange
        var defaultStoreId = new StoreId(Guid.Empty);

        using var dbContext = new GoldExDbContext(_dbContextOptions, _storeContext);
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        // 1. Seed Default Settings
        var defaultSetting = Setting.Create(
            "Default Inst", "Address", "123", 9m, 7m, 10m, 1m, 2m, 750m, 4.6083m, TimeSpan.FromMinutes(15), defaultStoreId
        );
        defaultSetting.UpdateBarcodePrintSettings(BarcodePrintSettings.CreateDefault());
        await dbContext.Set<Setting>().AddAsync(defaultSetting);

        // 2. Seed Default PriceUnit
        var defaultPriceUnit = PriceUnit.Create("18K Gold", GoldEx.Shared.Enums.UnitType.Gold18K, true, null);
        await dbContext.Set<PriceUnit>().AddAsync(defaultPriceUnit);

        // 3. Seed Default SmsTemplate
        var defaultTemplate = SmsTemplate.Create(GoldEx.Shared.Enums.SmsTemplateSubject.DueInvoice, "Body", "Params", defaultStoreId);
        await dbContext.Set<SmsTemplate>().AddAsync(defaultTemplate);

        // 4. Seed Default LedgerAccounts (Assets -> CurrentAssets)
        var defaultParentLedger = LedgerAccount.CreateSystemAccount("Assets", GoldEx.Shared.Enums.LedgerAccountType.Asset, null, null, defaultStoreId);
        await dbContext.Set<LedgerAccount>().AddAsync(defaultParentLedger);
        await dbContext.SaveChangesAsync(); // Save so parent ID is available

        var defaultChildLedger = LedgerAccount.CreateSystemAccount(SystemLedgerAccounts.InternalCashAccounts, GoldEx.Shared.Enums.LedgerAccountType.Asset, defaultParentLedger.Id, defaultPriceUnit.Id, defaultStoreId);
        await dbContext.Set<LedgerAccount>().AddAsync(defaultChildLedger);

        // 5. Seed Default FinancialAccount (Cash) linked to child ledger
        var defaultCashAccount = FinancialAccount.CreateSystemAccount(
            "Holder", "Broker", GoldEx.Shared.Enums.FinancialAccountType.Cash, defaultPriceUnit.Id, defaultChildLedger.Id,
            null, null, CashAccount.Create(null, GoldEx.Shared.Enums.CashAccountType.Internal), defaultStoreId
        );
        await dbContext.Set<FinancialAccount>().AddAsync(defaultCashAccount);

        // 6. Seed a non-clonable system Bank Account
        var nonClonableBankAccount = FinancialAccount.CreateSystemAccount(
            "Bank Holder", "Saman Bank", GoldEx.Shared.Enums.FinancialAccountType.LocalBankAccount, defaultPriceUnit.Id, defaultChildLedger.Id,
            LocalBankAccount.Create("CardNo", "ShabaNo", "AccNo"), null, null, defaultStoreId
        );
        await dbContext.Set<FinancialAccount>().AddAsync(nonClonableBankAccount);

        // 7. Seed Default Coin linked to child ledger
        var defaultCoin = Coin.Create("Default Coin", 8.13m, 900m, 1386, null, defaultChildLedger.Id, null, defaultStoreId);
        await dbContext.Set<Coin>().AddAsync(defaultCoin);

        // 8. Seed Default ProductCategory
        var defaultCategory = ProductCategory.Create("Default Category", "CAT", defaultStoreId);
        await dbContext.Set<ProductCategory>().AddAsync(defaultCategory);

        await dbContext.SaveChangesAsync();

        // Setup Repository Mock behaviors
        _storeRepoMock.Setup(x => x.ExistsAsync(It.IsAny<GoldEx.Sdk.Server.Infrastructure.Specifications.ISpecification<Store>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _storeRepoMock.Setup(x => x.BeginTransactionAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .Returns(() => dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted));

        _storeRepoMock.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(() => dbContext.SaveChangesAsync());

        _storeRepoMock.Setup(x => x.Get(It.IsAny<GoldEx.Sdk.Server.Infrastructure.Specifications.ISpecification<Store>>()))
            .Returns((GoldEx.Sdk.Server.Infrastructure.Specifications.ISpecification<Store> spec) =>
            {
                IQueryable<Store> query = dbContext.Set<Store>();
                if (spec.Criteria != null) query = query.Where(spec.Criteria);
                return query;
            });

        var settingRepoMock = new Mock<ISettingRepository>();
        var smsTemplateRepoMock = new Mock<ISmsTemplateRepository>();
        var ledgerAccountRepoMock = new Mock<ILedgerAccountRepository>();
        var financialAccountRepoMock = new Mock<IFinancialAccountRepository>();
        var priceUnitRepoMock = new Mock<IPriceUnitRepository>();
        priceUnitRepoMock.Setup(x => x.Get(It.IsAny<ISpecification<PriceUnit>>()))
            .Returns((ISpecification<PriceUnit> spec) =>
            {
                IQueryable<PriceUnit> query = dbContext.Set<PriceUnit>();
                if (spec.Criteria != null) query = query.Where(spec.Criteria);
                return query;
            });

        var coinRepoMock = new Mock<ICoinRepository>();
        var productCategoryRepoMock = new Mock<IProductCategoryRepository>();

        settingRepoMock.Setup(x => x.Get(It.IsAny<ISpecification<Setting>>()))
            .Returns((ISpecification<Setting> spec) =>
            {
                IQueryable<Setting> query = dbContext.Set<Setting>();
                if (spec.Criteria != null) query = query.Where(spec.Criteria);
                query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));
                return query;
            });
        settingRepoMock.Setup(x => x.Create(It.IsAny<Setting>()))
            .Callback<Setting>(s => dbContext.Set<Setting>().Add(s));

        smsTemplateRepoMock.Setup(x => x.Get(It.IsAny<ISpecification<SmsTemplate>>()))
            .Returns((ISpecification<SmsTemplate> spec) =>
            {
                IQueryable<SmsTemplate> query = dbContext.Set<SmsTemplate>();
                if (spec.Criteria != null) query = query.Where(spec.Criteria);
                return query;
            });
        smsTemplateRepoMock.Setup(x => x.Create(It.IsAny<SmsTemplate>()))
            .Callback<SmsTemplate>(t => dbContext.Set<SmsTemplate>().Add(t));

        ledgerAccountRepoMock.Setup(x => x.Get(It.IsAny<ISpecification<LedgerAccount>>()))
            .Returns((ISpecification<LedgerAccount> spec) =>
            {
                IQueryable<LedgerAccount> query = dbContext.Set<LedgerAccount>();
                if (spec.Criteria != null) query = query.Where(spec.Criteria);
                return query;
            });
        ledgerAccountRepoMock.Setup(x => x.Create(It.IsAny<LedgerAccount>()))
            .Callback<LedgerAccount>(la => dbContext.Set<LedgerAccount>().Add(la));

        financialAccountRepoMock.Setup(x => x.Get(It.IsAny<ISpecification<FinancialAccount>>()))
            .Returns((ISpecification<FinancialAccount> spec) =>
            {
                IQueryable<FinancialAccount> query = dbContext.Set<FinancialAccount>();
                if (spec.Criteria != null) query = query.Where(spec.Criteria);
                return query;
            });
        financialAccountRepoMock.Setup(x => x.Create(It.IsAny<FinancialAccount>()))
            .Callback<FinancialAccount>(fa => dbContext.Set<FinancialAccount>().Add(fa));

        coinRepoMock.Setup(x => x.Get(It.IsAny<ISpecification<Coin>>()))
            .Returns((ISpecification<Coin> spec) =>
            {
                IQueryable<Coin> query = dbContext.Set<Coin>();
                if (spec.Criteria != null) query = query.Where(spec.Criteria);
                return query;
            });
        coinRepoMock.Setup(x => x.Create(It.IsAny<Coin>()))
            .Callback<Coin>(c => dbContext.Set<Coin>().Add(c));

        productCategoryRepoMock.Setup(x => x.Get(It.IsAny<ISpecification<ProductCategory>>()))
            .Returns((ISpecification<ProductCategory> spec) =>
            {
                IQueryable<ProductCategory> query = dbContext.Set<ProductCategory>();
                if (spec.Criteria != null) query = query.Where(spec.Criteria);
                return query;
            });
        productCategoryRepoMock.Setup(x => x.Create(It.IsAny<ProductCategory>()))
            .Callback<ProductCategory>(pc => dbContext.Set<ProductCategory>().Add(pc));

        // Instantiate StoreService
        var storeService = new StoreService(
            _storeRepoMock.Object,
            _storeUserRepoMock.Object,
            _userContextMock.Object,
            _storeContext,
            _httpContextAccessorMock.Object,
            settingRepoMock.Object,
            smsTemplateRepoMock.Object,
            ledgerAccountRepoMock.Object,
            financialAccountRepoMock.Object,
            priceUnitRepoMock.Object,
            coinRepoMock.Object,
            productCategoryRepoMock.Object,
            _fileServiceMock.Object,
            _webHostEnvironmentMock.Object,
            new CreateStoreRequestValidator(_storeRepoMock.Object, new Mock<IConfiguration>().Object, dbContext),
            new UpdateStoreRequestValidator(_storeRepoMock.Object),
            new DeleteStoreValidator(dbContext, _storeContext)
        );

        var request = new StoreRequest(
            "New Branch",
            "new-branch",
            null,
            null,
            null,
            null,
            true
        );

        // Act
        await storeService.CreateStoreAsync(request, CancellationToken.None);

        // Assert
        // Verify new store repository CreateAsync was called
        _storeRepoMock.Verify(x => x.CreateAsync(It.Is<Store>(s => s.Name == "New Branch" && s.Slug == "new-branch"), It.IsAny<CancellationToken>()), Times.Once);

        // Verify that Settings, SmsTemplates, LedgerAccounts, and FinancialAccounts were cloned in the dbContext
        var allSettings = await dbContext.Set<Setting>().IgnoreQueryFilters().ToListAsync();
        Assert.That(allSettings.Count, Is.EqualTo(2));
        var clonedSetting = allSettings.FirstOrDefault(s => s.StoreId != defaultStoreId);
        Assert.That(clonedSetting, Is.Not.Null);
        Assert.That(clonedSetting!.InstitutionName, Is.EqualTo("Default Inst"));
        Assert.That(clonedSetting.BarcodePrintSettings, Is.Not.Null);
        Assert.That(clonedSetting.BarcodePrintSettings!.PositionItems.Count, Is.EqualTo(4));

        var allPriceUnits = await dbContext.Set<PriceUnit>().IgnoreQueryFilters().ToListAsync();
        Assert.That(allPriceUnits.Count, Is.EqualTo(1));

        // Verify cloned financial accounts: only cash is cloned (1 default + 1 cloned), bank account is NOT cloned (1 default + 0 cloned)
        var allFinancialAccounts = await dbContext.Set<FinancialAccount>().IgnoreQueryFilters().ToListAsync();
        Assert.That(allFinancialAccounts.Count, Is.EqualTo(3)); // 2 default (Cash + Bank) + 1 cloned (Cash)
        var clonedCash = allFinancialAccounts.FirstOrDefault(fa => fa.StoreId != defaultStoreId && fa.AccountType == GoldEx.Shared.Enums.FinancialAccountType.Cash);
        Assert.That(clonedCash, Is.Not.Null);
        var clonedBank = allFinancialAccounts.FirstOrDefault(fa => fa.StoreId != defaultStoreId && fa.AccountType == GoldEx.Shared.Enums.FinancialAccountType.LocalBankAccount);
        Assert.That(clonedBank, Is.Null);

        // Verify cloned Coin (1 default + 1 cloned)
        var allCoins = await dbContext.Set<Coin>().IgnoreQueryFilters().ToListAsync();
        Assert.That(allCoins.Count, Is.EqualTo(2));
        var clonedCoin = allCoins.FirstOrDefault(c => c.StoreId != defaultStoreId);
        Assert.That(clonedCoin, Is.Not.Null);
        Assert.That(clonedCoin!.Title, Is.EqualTo("Default Coin"));

        // Verify cloned ProductCategory (1 default + 1 cloned)
        var allCategories = await dbContext.Set<ProductCategory>().IgnoreQueryFilters().ToListAsync();
        Assert.That(allCategories.Count, Is.EqualTo(2));
        var clonedCategory = allCategories.FirstOrDefault(pc => pc.StoreId != defaultStoreId);
        Assert.That(clonedCategory, Is.Not.Null);
        Assert.That(clonedCategory!.Title, Is.EqualTo("Default Category"));

        var allTemplates = await dbContext.Set<SmsTemplate>().IgnoreQueryFilters().ToListAsync();
        Assert.That(allTemplates.Count, Is.EqualTo(2));
        var clonedTemp = allTemplates.FirstOrDefault(t => t.StoreId != defaultStoreId);
        Assert.That(clonedTemp, Is.Not.Null);
        Assert.That(clonedTemp!.Body, Is.EqualTo("Body"));

        var allLedgers = await dbContext.Set<LedgerAccount>().IgnoreQueryFilters().ToListAsync();
        Assert.That(allLedgers.Count, Is.EqualTo(4)); // 2 default + 2 cloned
        var clonedParentLedger = allLedgers.FirstOrDefault(l => l.StoreId != defaultStoreId && l.Title == "Assets");
        var clonedChildLedger = allLedgers.FirstOrDefault(l => l.StoreId != defaultStoreId && l.Title == SystemLedgerAccounts.InternalCashAccounts);
        Assert.That(clonedParentLedger, Is.Not.Null);
        Assert.That(clonedChildLedger, Is.Not.Null);
        Assert.That(clonedChildLedger!.ParentAccountId, Is.EqualTo(clonedParentLedger!.Id));

        var clonedFa = allFinancialAccounts.FirstOrDefault(fa => fa.StoreId != defaultStoreId && fa.AccountType == GoldEx.Shared.Enums.FinancialAccountType.Cash);
        Assert.That(clonedFa, Is.Not.Null);
        Assert.That(clonedFa!.LedgerAccountId, Is.EqualTo(clonedChildLedger.Id));
    }

    [Test]
    public async Task UpdateStoreAsync_RenamesLogoAndReportFilesOnSlugChange()
    {
        // Arrange
        using var dbContext = new GoldExDbContext(_dbContextOptions, _storeContext);
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        var store = Store.Create("Old Name", "old-slug");
        await dbContext.Set<Store>().AddAsync(store);
        await dbContext.SaveChangesAsync();

        var storeId = store.Id.Value;

        // Setup mock repository delegation to dbContext
        _storeRepoMock.Setup(x => x.Get(It.IsAny<GoldEx.Sdk.Server.Infrastructure.Specifications.ISpecification<Store>>()))
            .Returns((GoldEx.Sdk.Server.Infrastructure.Specifications.ISpecification<Store> spec) => 
            {
                IQueryable<Store> query = dbContext.Set<Store>();
                if (spec.Criteria != null) query = query.Where(spec.Criteria);
                return query;
            });

        _storeRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Store>(), It.IsAny<CancellationToken>()))
            .Callback<Store, CancellationToken>((s, ct) => dbContext.Set<Store>().Update(s))
            .Returns(Task.CompletedTask);

        // Create dummy files for old-slug
        var baseDir = Directory.GetCurrentDirectory();
        var appIconDir = Path.Combine(baseDir, "uploads", "icons", "app");
        var reportsDir = Path.Combine(baseDir, "Reports");

        Directory.CreateDirectory(appIconDir);
        Directory.CreateDirectory(reportsDir);

        var oldLogoPath = Path.Combine(appIconDir, "logo_old-slug.png");
        var oldReportPath1 = Path.Combine(reportsDir, "InvoiceReport_old-slug.repx");
        var oldReportPath2 = Path.Combine(reportsDir, "OtherReport_old-slug.repx");

        await File.WriteAllTextAsync(oldLogoPath, "logo-content");
        await File.WriteAllTextAsync(oldReportPath1, "report-1-content");
        await File.WriteAllTextAsync(oldReportPath2, "report-2-content");

        // Instantiate StoreService
        var storeService = new StoreService(
            _storeRepoMock.Object,
            _storeUserRepoMock.Object,
            _userContextMock.Object,
            _storeContext,
            _httpContextAccessorMock.Object,
            new Mock<ISettingRepository>().Object,
            new Mock<ISmsTemplateRepository>().Object,
            new Mock<ILedgerAccountRepository>().Object,
            new Mock<IFinancialAccountRepository>().Object,
            new Mock<IPriceUnitRepository>().Object,
            new Mock<ICoinRepository>().Object,
            new Mock<IProductCategoryRepository>().Object,
            _fileServiceMock.Object,
            _webHostEnvironmentMock.Object,
            new CreateStoreRequestValidator(_storeRepoMock.Object, new Mock<IConfiguration>().Object, dbContext),
            new UpdateStoreRequestValidator(_storeRepoMock.Object),
            new DeleteStoreValidator(dbContext, _storeContext)
        );

        var request = new StoreRequest(
            "New Name",
            "new-slug",
            null,
            null,
            null,
            null,
            true
        );

        // Act
        await storeService.UpdateStoreAsync(storeId, request, CancellationToken.None);

        // Assert
        var newLogoPath = Path.Combine(appIconDir, "logo_new-slug.png");
        var newReportPath1 = Path.Combine(reportsDir, "InvoiceReport_new-slug.repx");
        var newReportPath2 = Path.Combine(reportsDir, "OtherReport_new-slug.repx");

        Assert.That(File.Exists(newLogoPath), Is.True);
        Assert.That(File.Exists(newReportPath1), Is.True);
        Assert.That(File.Exists(newReportPath2), Is.True);

        Assert.That(File.Exists(oldLogoPath), Is.False);
        Assert.That(File.Exists(oldReportPath1), Is.False);
        Assert.That(File.Exists(oldReportPath2), Is.False);

        // Cleanup
        try
        {
            if (Directory.Exists(appIconDir)) Directory.Delete(appIconDir, true);
            if (Directory.Exists(reportsDir)) Directory.Delete(reportsDir, true);
            var uploadsDir = Path.Combine(baseDir, "uploads");
            if (Directory.Exists(uploadsDir)) Directory.Delete(uploadsDir, true);
        }
        catch { }
    }

    [Test]
    public async Task DeleteStoreAsync_DeletesLogoAndReportFiles()
    {
        // Arrange
        using var dbContext = new GoldExDbContext(_dbContextOptions, _storeContext);
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        var store = Store.Create("Store to Delete", "delete-me");
        await dbContext.Set<Store>().AddAsync(store);
        await dbContext.SaveChangesAsync();

        var storeId = store.Id.Value;

        // Setup mock repository delegation to dbContext
        _storeRepoMock.Setup(x => x.Get(It.IsAny<GoldEx.Sdk.Server.Infrastructure.Specifications.ISpecification<Store>>()))
            .Returns((GoldEx.Sdk.Server.Infrastructure.Specifications.ISpecification<Store> spec) => 
            {
                IQueryable<Store> query = dbContext.Set<Store>();
                if (spec.Criteria != null) query = query.Where(spec.Criteria);
                return query;
            });

        _storeRepoMock.Setup(x => x.DeleteAsync(It.IsAny<Store>(), It.IsAny<CancellationToken>()))
            .Callback<Store, CancellationToken>((s, ct) => dbContext.Set<Store>().Remove(s))
            .Returns(Task.CompletedTask);

        _storeUserRepoMock.Setup(x => x.Get(It.IsAny<GoldEx.Sdk.Server.Infrastructure.Specifications.ISpecification<StoreUser>>()))
            .Returns((GoldEx.Sdk.Server.Infrastructure.Specifications.ISpecification<StoreUser> spec) =>
            {
                IQueryable<StoreUser> query = dbContext.Set<StoreUser>();
                if (spec.Criteria != null) query = query.Where(spec.Criteria);
                return query;
            });

        // Create dummy files for delete-me slug
        var baseDir = Directory.GetCurrentDirectory();
        var appIconDir = Path.Combine(baseDir, "uploads", "icons", "app");
        var reportsDir = Path.Combine(baseDir, "Reports");

        Directory.CreateDirectory(appIconDir);
        Directory.CreateDirectory(reportsDir);

        var logoPath = Path.Combine(appIconDir, "logo_delete-me.png");
        var reportPath1 = Path.Combine(reportsDir, "InvoiceReport_delete-me.repx");
        var reportPath2 = Path.Combine(reportsDir, "OtherReport_delete-me.repx");

        await File.WriteAllTextAsync(logoPath, "logo-content");
        await File.WriteAllTextAsync(reportPath1, "report-1-content");
        await File.WriteAllTextAsync(reportPath2, "report-2-content");

        var settingRepoMock = new Mock<ISettingRepository>();
        var smsTemplateRepoMock = new Mock<ISmsTemplateRepository>();
        var ledgerAccountRepoMock = new Mock<ILedgerAccountRepository>();
        var financialAccountRepoMock = new Mock<IFinancialAccountRepository>();
        var coinRepoMock = new Mock<ICoinRepository>();
        var productCategoryRepoMock = new Mock<IProductCategoryRepository>();

        settingRepoMock.Setup(x => x.Get(It.IsAny<ISpecification<Setting>>()))
            .Returns((ISpecification<Setting> spec) => dbContext.Set<Setting>().AsQueryable());
        smsTemplateRepoMock.Setup(x => x.Get(It.IsAny<ISpecification<SmsTemplate>>()))
            .Returns((ISpecification<SmsTemplate> spec) => dbContext.Set<SmsTemplate>().AsQueryable());
        ledgerAccountRepoMock.Setup(x => x.Get(It.IsAny<ISpecification<LedgerAccount>>()))
            .Returns((ISpecification<LedgerAccount> spec) => dbContext.Set<LedgerAccount>().AsQueryable());
        financialAccountRepoMock.Setup(x => x.Get(It.IsAny<ISpecification<FinancialAccount>>()))
            .Returns((ISpecification<FinancialAccount> spec) => dbContext.Set<FinancialAccount>().AsQueryable());
        coinRepoMock.Setup(x => x.Get(It.IsAny<ISpecification<Coin>>()))
            .Returns((ISpecification<Coin> spec) => dbContext.Set<Coin>().AsQueryable());
        productCategoryRepoMock.Setup(x => x.Get(It.IsAny<ISpecification<ProductCategory>>()))
            .Returns((ISpecification<ProductCategory> spec) => dbContext.Set<ProductCategory>().AsQueryable());

        // Instantiate StoreService
        var storeService = new StoreService(
            _storeRepoMock.Object,
            _storeUserRepoMock.Object,
            _userContextMock.Object,
            _storeContext,
            _httpContextAccessorMock.Object,
            settingRepoMock.Object,
            smsTemplateRepoMock.Object,
            ledgerAccountRepoMock.Object,
            financialAccountRepoMock.Object,
            new Mock<IPriceUnitRepository>().Object,
            coinRepoMock.Object,
            productCategoryRepoMock.Object,
            _fileServiceMock.Object,
            _webHostEnvironmentMock.Object,
            new CreateStoreRequestValidator(_storeRepoMock.Object, new Mock<IConfiguration>().Object, dbContext),
            new UpdateStoreRequestValidator(_storeRepoMock.Object),
            new DeleteStoreValidator(dbContext, _storeContext)
        );

        // Act
        await storeService.DeleteStoreAsync(storeId, CancellationToken.None);

        // Assert
        Assert.That(File.Exists(logoPath), Is.False);
        Assert.That(File.Exists(reportPath1), Is.False);
        Assert.That(File.Exists(reportPath2), Is.False);

        // Cleanup
        try
        {
            if (Directory.Exists(appIconDir)) Directory.Delete(appIconDir, true);
            if (Directory.Exists(reportsDir)) Directory.Delete(reportsDir, true);
            var uploadsDir = Path.Combine(baseDir, "uploads");
            if (Directory.Exists(uploadsDir)) Directory.Delete(uploadsDir, true);
        }
        catch { }
    }
}
