using GoldEx.Client.Components.Components.Dialogs;
using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.Invoices.Components;
using GoldEx.Client.Pages.Transactions.Components;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Customers.Components;

public partial class CustomersList
{
    [Parameter] public string Class { get; set; } = default!;
    [Inject] private ResponsiveDialogService ResponsiveDialogs { get; set; } = default!;
    [Inject] private HelpContext HelpContext { get; set; } = default!;

    private string? _searchString;
    private DateRange _filterDateRange = new();
    private const int BatchSize = 30;
    private List<CustomerVm> _customers = [];
    private int _nextOffset;
    private int _appendStart;
    private bool _hasMore;
    private bool _loadingMore;
    private bool _loadMoreFailed;
    private int _totalItems;
    private int _loadVersion;
    private bool _loading = true;
    private bool _loadFailed;
    private string _sortLabel = nameof(CustomerVm.CreatedAt);
    private bool _sortDescending = true;
    private int ActiveFilterCount => (_customerType.HasValue ? 1 : 0)
        + (_transactionType.HasValue ? 1 : 0)
        + (_filterDateRange.Start.HasValue || _filterDateRange.End.HasValue ? 1 : 0);
    private bool HasQuery => !string.IsNullOrWhiteSpace(_searchString) || ActiveFilterCount > 0;
    private int ActiveOptionCount => ActiveFilterCount + (_sortLabel != nameof(CustomerVm.CreatedAt) || !_sortDescending ? 1 : 0);
    private string FilterButtonLabel => ActiveOptionCount == 0 ? "فیلتر و مرتب‌سازی مشتریان"
        : $"فیلتر و مرتب‌سازی مشتریان، {ActiveOptionCount} تنظیم فعال";
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Medium };
    private CustomerType? _customerType;
    private TransactionType? _transactionType;

    protected override void OnInitialized()
    {
        HelpContext.Slug = "customer-management-video";
        base.OnInitialized();
    }

    public override ValueTask DisposeAsync()
    {
        HelpContext.Slug = null;
        return base.DisposeAsync();
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadCustomersAsync();
    }

    private async Task LoadCustomersAsync(bool append = false, int? take = null)
    {
        if (append && (_loading || _loadingMore || !_hasMore)) return;

        var version = ++_loadVersion;
        var skip = append ? _nextOffset : 0;
        var requestedCount = take ?? BatchSize;
        _loading = !append;
        _loadingMore = append;
        _loadFailed = false;
        _loadMoreFailed = false;
        if (!append)
        {
            _nextOffset = 0;
            _appendStart = 0;
            _hasMore = false;
        }
        StateHasChanged();

        var filter = new RequestFilter(skip, requestedCount, _searchString, _sortLabel,
            _sortDescending ? Sdk.Common.Definitions.SortDirection.Descending : Sdk.Common.Definitions.SortDirection.Ascending);
        var customerFilter = new CustomerFilter(_customerType, _transactionType, _filterDateRange.Start, _filterDateRange.End);

        await SendRequestAsync<ICustomerService, PagedList<GetCustomerResponse>>(
            action: (service, token) => service.GetListAsync(filter, customerFilter, token),
            afterSend: response =>
            {
                // A slow earlier query must never replace/append to a newer set of results.
                if (version != _loadVersion || IsDisposed) return;
                var items = response.Data.Select(CustomerVm.CreateFrom).ToList();
                if (append)
                {
                    _appendStart = _customers.Count;
                    var existing = _customers.Select(customer => customer.Id).ToHashSet();
                    _customers.AddRange(items.Where(customer => existing.Add(customer.Id)));
                }
                else
                {
                    _customers = items;
                }
                _totalItems = response.Total;
                // Advance by consumed server records, even if a concurrent change produced duplicates.
                _nextOffset = skip + items.Count;
                _hasMore = items.Count > 0 && _nextOffset < _totalItems;
            },
            onFailure: () =>
            {
                if (version != _loadVersion || IsDisposed) return;
                if (append) _loadMoreFailed = true;
                else _loadFailed = true;
            },
            createScope: true,
            cancelPrevious: true);

        if (version != _loadVersion || IsDisposed) return;
        _loading = false;
        _loadingMore = false;
    }

    private async Task LoadMoreAsync()
    {
        await LoadCustomersAsync(append: true);
    }

    private async Task OnSearch(string? text)
    {
        _searchString = text;
        await ResetResultsAsync();
    }

    private async Task OnSortChanged(string label)
    {
        _sortLabel = label;
        _sortDescending = label == nameof(CustomerVm.CreatedAt);
        await ResetResultsAsync();
    }

    private async Task ToggleSortDirection()
    {
        _sortDescending = !_sortDescending;
        await ResetResultsAsync();
    }

    private async Task ResetResultsAsync()
    {
        await LoadCustomersAsync();
    }

    private async Task ClearFilters()
    {
        _filterDateRange = new DateRange();
        _customerType = null;
        _transactionType = null;
        await ResetResultsAsync();
    }

    private async Task ClearQuery()
    {
        _searchString = null;
        await ClearFilters();
    }

    public async Task OnCreate()
    {
        var dialog = ResponsiveDialogs.Show<Editor>("افزودن مشتری جدید");

        var result = await dialog.Result;

        if (result is {Canceled: false})
        {
            AddSuccessToast("مشتری جدید با موفقیت افزوده شد.");
            await RefreshAsync();
        }
    }

    private async Task OnRemove(CustomerVm model)
    {
        if (!model.Id.HasValue)
            throw new InvalidOperationException();

        var parameters = new DialogParameters<Remove>
        {
            { x => x.Id, model.Id.Value },
            { x => x.CustomerName, model.FullName }
        };

        var dialog = await DialogService.ShowAsync<Remove>("حذف مشتری", parameters, _dialogOptions with { MaxWidth = MaxWidth.Small });

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("مشتری با موفقیت حذف شد.");
            await RefreshAsync();
        }
    }

    private async Task OnEdit(CustomerVm model)
    {
        var parameters = new Dictionary<string, object>
        {
            { nameof(Editor.Model), EditorDraft.Copy(model) }
        };

        var dialog = ResponsiveDialogs.Show<Editor>("ویرایش اطلاعات مشتری", parameters);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("اطلاعات مشتری با موفقیت ویرایش شد.");
            await RefreshAsync();
        }
    }

    private async Task OnViewTransaction(CustomerVm customerVm)
    {
        var parameters = new DialogParameters<TransactionList>
        {
            { x => x.CustomerId, customerVm.Id },
            { x => x.ShowReversed, false },
            { x => x.Descending, true },
            { x => x.ContainerClass, "responsive-table-toolbar" }
        };
        var dialog = await DialogService.ShowAsync<TransactionList>($"تراکنش های {customerVm.FullName}",
            parameters,
            _dialogOptions with
            {
                MaxWidth = MaxWidth.ExtraLarge
            });
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            await RefreshAsync();
        }
    }

    private async Task OnViewInvoices(CustomerVm customerVm)
    {
        var parameters = new DialogParameters
        {
            { nameof(InvoicesList.CustomerId), customerVm.Id }
        };
        var dialog = await DialogService.ShowAsync<InvoicesList>($"فاکتورهای {customerVm.FullName}", parameters, _dialogOptions with { MaxWidth = MaxWidth.ExtraLarge });
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            await RefreshAsync();
        }
    }

    private async Task RefreshAsync()
    {
        // Preserve the loaded window after editing instead of discarding previously revealed cards.
        await LoadCustomersAsync(take: Math.Max(BatchSize, _nextOffset));
        if (!IsDisposed) StateHasChanged();
    }

    #region Mobile filters

    public record CustomerMobileFiltersResult(
        DateRange DateRange,
        CustomerType? CustomerType,
        TransactionType? TransactionType,
        string SortLabel,
        bool SortDescending
    );

    private async Task OpenMobileFilters()
    {
        var parameters = new DialogParameters<CustomerFiltersDialog>
        {
            { x => x.DateRange, _filterDateRange },
            { x => x.CustomerTypeParam, _customerType },
            { x => x.TransactionTypeParam, _transactionType },
            { x => x.SortLabel, _sortLabel },
            { x => x.SortDescending, _sortDescending }
        };

        var options = new DialogOptions
        {
            CloseButton = true,
            FullWidth = true,
            MaxWidth = MaxWidth.Small
        };

        var dialog = await DialogService.ShowAsync<CustomerFiltersDialog>("فیلترها", parameters, options);
        var result = await dialog.Result;

        if (result is { Canceled: false } && result.Data is CustomerMobileFiltersResult filterResult)
        {
            await ApplyMobileFilters(filterResult);
        }
    }

    private async Task ApplyMobileFilters(CustomerMobileFiltersResult result)
    {
        _filterDateRange = new DateRange(result.DateRange.Start, result.DateRange.End);
        _customerType = result.CustomerType;
        _transactionType = result.TransactionType;
        _sortLabel = result.SortLabel;
        _sortDescending = result.SortDescending;

        await ResetResultsAsync();
    }

    #endregion
}
