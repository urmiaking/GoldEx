using GoldEx.Client.Pages.UserAccounts.Validators;
using GoldEx.Client.Pages.UserAccounts.ViewModels;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.UserAccounts.Components.Editors;

public partial class UserEditor
{
    [Parameter] public UserEditorVm Model { get; set; } = new();
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private MudForm _form = default!;
    private readonly UserValidator _userValidator = new();

    private void Close() => MudDialog.Cancel();

    private async Task Submit()
    {
        await _form.ValidateAsync();

        if (!_form.IsValid)
            return;

        var isEdit = Model.Id.HasValue;

        var request = Model.ToRequest();

        await SendRequestAsync<IUserAccountService>(
            action: (s, ct) =>
                isEdit ? s.UpdateAccountAsync(Model.Id!.Value, request, ct) : s.CreateAccountAsync(request, ct),
            afterSend: () =>
            {
                AddSuccessToast(isEdit ? "ویرایش حساب کاربری با موفقیت انجام شد" : "حساب کاربری با موفقیت ایجاد شد");
                MudDialog.Close(true);
                return Task.CompletedTask;
            });
    }
}