using GoldEx.Client.Components.Components.Dialogs;
using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.FinancialAccounts.ViewModels;
using Microsoft.AspNetCore.Components;
using GoldEx.Shared.DTOs.PriceUnits;

namespace GoldEx.Tests.Server.Application.Services;

[TestFixture]
public class ResponsiveDialogTester
{
    private sealed class TestEditor : ComponentBase { }

    [Test]
    public async Task BusySession_BlocksCancel_ButSuccessfulSaveCompletesOnce()
    {
        var dialogs = new ResponsiveDialogService();
        var session = dialogs.Show<TestEditor>("editor");
        session.SetBusy(true);
        session.Cancel();
        Assert.That(session.Result.IsCompleted, Is.False);
        session.Close("saved");
        session.Cancel();
        var result = await session.Result;
        Assert.Multiple(() =>
        {
            Assert.That(result.Canceled, Is.False);
            Assert.That(result.Data, Is.EqualTo("saved"));
            Assert.That(dialogs.Sessions, Is.Empty);
        });
    }

    [Test]
    public async Task CancelingChild_LeavesParentOpen_AndNavigationCancelsBusyParent()
    {
        var dialogs = new ResponsiveDialogService();
        var parent = dialogs.Show<TestEditor>("parent");
        var child = dialogs.Show<TestEditor>("child");
        child.Cancel();
        Assert.That((await child.Result).Canceled, Is.True);
        Assert.That(dialogs.Sessions, Has.Count.EqualTo(1));
        parent.SetBusy(true);
        dialogs.CancelAll();
        Assert.That((await parent.Result).Canceled, Is.True);
        Assert.That(dialogs.Sessions, Is.Empty);
    }

    [Test]
    public async Task ClosingParent_CancelsAllDescendants()
    {
        var dialogs = new ResponsiveDialogService();
        var parent = dialogs.Show<TestEditor>("parent");
        var child = dialogs.Show<TestEditor>("child");
        child.SetBusy(true);
        parent.Cancel();
        Assert.That((await child.Result).Canceled, Is.True);
        Assert.That(dialogs.Sessions, Is.Empty);
    }

    [Test]
    public void CustomerDraft_KeepsIdsAndDecimalPrecision_AndIsolatesNestedAccountEdits()
    {
        var unit = new GetPriceUnitTitleResponse(Guid.NewGuid(), "Unit", false, true, false);
        var original = new CustomerVm
        {
            Id = Guid.NewGuid(), FullName = "Original", NationalId = "code", PhoneNumber = "phone",
            CreditLimit = 123456789012345678.1234567890m,
            CreditLimitPriceUnit = unit,
            FinancialAccounts = [new FinancialAccountVm { Id = Guid.NewGuid(), PriceUnit = unit, LocalBankAccount = new() { AccountNumber = "original" } }]
        };
        var draft = EditorDraft.Copy(original);
        draft.FullName = "Changed";
        draft.FinancialAccounts![0].LocalBankAccount!.AccountNumber = "changed";
        draft.FinancialAccounts.Add(new());
        Assert.Multiple(() =>
        {
            Assert.That(draft.Id, Is.EqualTo(original.Id));
            Assert.That(draft.CreditLimit, Is.EqualTo(original.CreditLimit));
            Assert.That(draft.CreditLimitPriceUnit, Is.EqualTo(unit));
            Assert.That(draft.FinancialAccounts[0].PriceUnit, Is.EqualTo(unit));
            Assert.That(original.FullName, Is.EqualTo("Original"));
            Assert.That(original.FinancialAccounts, Has.Count.EqualTo(1));
            Assert.That(original.FinancialAccounts![0].LocalBankAccount!.AccountNumber, Is.EqualTo("original"));
            Assert.That(draft.FinancialAccounts[0].Id, Is.EqualTo(original.FinancialAccounts[0].Id));
        });
    }
}
