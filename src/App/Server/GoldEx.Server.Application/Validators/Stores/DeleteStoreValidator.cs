using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.Services.Abstractions;
using GoldEx.Server.Domain.CheckPaymentAggregate;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.InventoryEntryAggregate;
using GoldEx.Server.Domain.InventoryExitAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.MeltingBatchAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Server.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.Stores;

[ScopedService]
public class DeleteStoreValidator : AbstractValidator<Guid>
{
    private readonly GoldExDbContext _dbContext;
    private readonly IStoreContext _storeContext;

    public DeleteStoreValidator(GoldExDbContext dbContext, IStoreContext storeContext)
    {
        _dbContext = dbContext;
        _storeContext = storeContext;

        RuleFor(id => id)
            .NotEmpty().WithMessage("شناسه فروشگاه نمی‌تواند خالی باشد.")
            .NotEqual(Guid.Empty).WithMessage("فروشگاه پیش‌فرض سیستم قابل حذف نیست.")
            .Must(NotBeCurrentStore).WithMessage("فروشگاه فعال فعلی شما قابل حذف نیست.")
            .MustAsync(NotBeInUseAsync).WithMessage("این فروشگاه دارای اطلاعات ثبت شده است و نمی‌توان آن را حذف کرد. لطفاً به جای حذف، فروشگاه را غیرفعال کنید.")
            .MustAsync(NotBeDefaultStoreForUsersAsync).WithMessage("این فروشگاه به عنوان فروشگاه پیش‌فرض کاربران تنظیم شده است و نمی‌توان آن را حذف کرد.");
    }

    private bool NotBeCurrentStore(Guid id)
    {
        return _storeContext.StoreId != id;
    }

    private async Task<bool> NotBeInUseAsync(Guid id, CancellationToken cancellationToken)
    {
        var storeIdValue = new StoreId(id);
        var inUse = await _dbContext.Set<Customer>().IgnoreQueryFilters().AnyAsync(x => x.StoreId == storeIdValue, cancellationToken)
            || await _dbContext.Set<Product>().IgnoreQueryFilters().AnyAsync(x => x.StoreId == storeIdValue, cancellationToken)
            || await _dbContext.Set<Invoice>().IgnoreQueryFilters().AnyAsync(x => x.StoreId == storeIdValue, cancellationToken)
            || await _dbContext.Set<Transaction>().IgnoreQueryFilters().AnyAsync(x => x.StoreId == storeIdValue, cancellationToken)
            || await _dbContext.Set<PaymentVoucher>().IgnoreQueryFilters().AnyAsync(x => x.StoreId == storeIdValue, cancellationToken)
            || await _dbContext.Set<CheckPayment>().IgnoreQueryFilters().AnyAsync(x => x.StoreId == storeIdValue, cancellationToken)
            || await _dbContext.Set<MeltingBatch>().IgnoreQueryFilters().AnyAsync(x => x.StoreId == storeIdValue, cancellationToken)
            || await _dbContext.Set<InventoryEntry>().IgnoreQueryFilters().AnyAsync(x => x.StoreId == storeIdValue, cancellationToken)
            || await _dbContext.Set<InventoryExit>().IgnoreQueryFilters().AnyAsync(x => x.StoreId == storeIdValue, cancellationToken);

        return !inUse;
    }

    private async Task<bool> NotBeDefaultStoreForUsersAsync(Guid id, CancellationToken cancellationToken)
    {
        var storeIdValue = new StoreId(id);
        var isDefault = await _dbContext.Set<StoreUser>().AnyAsync(su => su.StoreId == storeIdValue && su.IsDefault, cancellationToken);
        return !isDefault;
    }
}
