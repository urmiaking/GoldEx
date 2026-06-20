using GoldEx.Shared.DTOs.Stores;
using GoldEx.Shared.DTOs.UserAccounts;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoldEx.Client.Pages.Settings.Components.Stores;

public partial class StoreUsersDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public Guid StoreId { get; set; }
    [Parameter] public string StoreName { get; set; } = string.Empty;

    private List<GetUserAccountResponse> _allUsers = [];
    private HashSet<Guid> _selectedUserIds = [];
    private string _searchQuery = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        // 1. Fetch active users in the system
        var users = await SendRequestAsync<IUserAccountService, List<GetUserAccountResponse>>(
            action: (service, token) => service.GetAccountsListAsync(token));

        if (users != null)
        {
            _allUsers = users.Where(u => u.IsActive).ToList();
        }

        // 2. Fetch assigned user IDs for this store
        var assigned = await SendRequestAsync<IStoreService, List<Guid>>(
            action: (service, token) => service.GetStoreUsersAsync(StoreId, token));

        if (assigned != null)
        {
            _selectedUserIds = assigned.ToHashSet();
        }
    }

    private bool IsUserSelected(Guid userId) => _selectedUserIds.Contains(userId);

    private void OnUserSelectionToggled(Guid userId, bool isSelected)
    {
        if (isSelected)
            _selectedUserIds.Add(userId);
        else
            _selectedUserIds.Remove(userId);
    }

    private bool IsAllSelected => FilteredUsers.Any() && FilteredUsers.All(u => _selectedUserIds.Contains(u.Id));

    private void OnSelectAllChanged(bool isSelected)
    {
        foreach (var user in FilteredUsers)
        {
            if (isSelected)
                _selectedUserIds.Add(user.Id);
            else
                _selectedUserIds.Remove(user.Id);
        }
    }

    private void OnSearchChanged(string text)
    {
        _searchQuery = text;
    }

    private IEnumerable<GetUserAccountResponse> FilteredUsers =>
        string.IsNullOrWhiteSpace(_searchQuery)
            ? _allUsers
            : _allUsers.Where(u => u.FullName.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase)
                                   || u.Username.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase));

    private async Task SaveAsync()
    {
        var request = new AssignStoreUsersRequest(_selectedUserIds.ToList());

        await SendRequestAsync<IStoreService>(
            action: (service, token) => service.AssignStoreUsersAsync(StoreId, request, token),
            afterSend: () =>
            {
                AddSuccessToast("تخصیص کاربران با موفقیت به‌روزرسانی شد.");
                MudDialog.Close(DialogResult.Ok(true));
                return Task.CompletedTask;
            });
    }

    private void Cancel() => MudDialog.Cancel();
}
