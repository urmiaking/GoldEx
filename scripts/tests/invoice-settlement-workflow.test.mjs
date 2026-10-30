import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
const root=new URL('../../src/App/Client/GoldEx.Client/Pages/Invoices/Components/',import.meta.url);
const read=(file)=>readFileSync(new URL(file,root),'utf8');

test('invoice settlement is one clear primary entry, while discounts and expenses remain accessible',()=>{
 const view=read('EditorForm.razor');
 const logic=read('EditorForm.razor.cs');
 assert.match(view,/تسویه فاکتور/);
 assert.doesNotMatch(view,/<MudMenu EndIcon=.*Label="ثبت پرداخت"/);
 assert.match(view,/OpenPaymentsDialog\(false\)/);
 assert.match(view,/OpenPaymentsDialog\(true\)/);
 assert.match(view,/OpenDiscountsDialog/);
 assert.match(view,/OpenExtraCostsDialog/);
 assert.match(logic,/ResponsiveDialogs.Show<PaymentDialog>\("تسویه فاکتور"/);
 assert.match(logic,/StartWithReview\)\] = reviewFirst/);
 assert.doesNotMatch(logic,/private async Task OnAddPayment\(/);
});
test('settlement wizard asks receive/pay first, then method, then review and confirmation',()=>{
 const view=read('PaymentDialog.razor');
 const logic=read('PaymentDialog.razor.cs');
 const style=read('PaymentDialog.razor.css');
 assert.match(view,/SettlementStep.Direction/);
 assert.match(view,/SettlementStep.Method/);
 assert.match(view,/SettlementStep.Review/);
 assert.match(view,/SelectSide\(PaymentSide.Receive\)/);
 assert.match(view,/SelectSide\(PaymentSide.Pay\)/);
 assert.match(view,/ReceiveDescription/);
 assert.match(view,/PayDescription/);
 assert.match(view,/@onclick="GoBackToDirection"/);
 assert.match(view,/OnClick="Save"/);
 assert.match(view,/تأیید تسویه‌ها/);
 for(const type of ['InternalCash','CustomerTransfer','Check','UsedGoldInventory','MoltenGoldInventory','Coin']) {
  assert.match(view,new RegExp('OnAddPayment\\(PaymentType\\.'+type+'\\)'));
 }
 assert.doesNotMatch(view,/OnApplyPrepaymentVouchers|اعمال پیش.پرداخت|PaymentVouchersSelectorList/);
 assert.doesNotMatch(logic,/OnApplyPrepaymentVouchers|PaymentVouchersSelectorList|CustomerId/);
 assert.match(view,/@key="_step"/);
 assert.match(view,/invoice-settlement-stage/);
 assert.match(logic,/Task ChangeStepAsync\(SettlementStep next\)/);
 assert.match(logic,/await Task.Delay\(140\)/);
 assert.match(style,/@keyframes invoice-settlement-enter/);
 assert.match(style,/@keyframes invoice-settlement-leave/);
 assert.match(style,/prefers-reduced-motion/);
 assert.equal((view.match(/invoice-settlement-method-top/g) || []).length, 6);
 assert.match(style,/\.invoice-settlement-method-top\{display:flex;align-items:center/);
 assert.match(logic,/var defaultSide = _selectedSide.Value;/);
 assert.match(logic,/_step = StartWithReview && Items.Count > 0/);
 assert.match(logic,/ShowReview\(\)/);
 assert.match(style,/\.invoice-settlement-directions/);
 assert.match(style,/\.invoice-settlement-methods/);
 assert.match(style,/@media\(max-width:699px\)/);
});
test('each new settlement inherits the chosen side and recalculated balance; editing remains flexible',()=>{
 const flow=read('PaymentDialog.razor.cs');
 const editor=read('PaymentEditor.razor');
 const logic=read('PaymentEditor.razor.cs');
 assert.match(flow,/TotalRemaining \+ _initialEffect\s*- Items.Sum\(p => GetBalanceEffect\(InvoiceType, p\)\)/);
 assert.match(flow,/CurrentRemaining \+ GetBalanceEffect\(InvoiceType, payment\)/);
 assert.match(flow,/\[nameof\(PaymentEditor.TotalRemaining\)\] = CurrentRemaining/);
 assert.match(flow,/\[nameof\(PaymentEditor.LockPaymentSide\)\] = true/);
 assert.match(editor,/@if \(LockPaymentSide\)/);
 assert.match(editor,/@bind-SelectedValue="Model.PaymentSide"/);
 assert.match(logic,/\[Parameter\] public bool LockPaymentSide/);
 assert.match(editor,/CanApplyTotalRemaining/);
 assert.match(logic,/private bool CanApplyTotalRemaining/);
});
test('existing payments retain totals edit delete and premium restriction in review',()=>{
 const view=read('PaymentDialog.razor');
 const logic=read('PaymentDialog.razor.cs');
 for(const term of ['TotalReceiptsAmount','TotalPaymentsAmount','OnEdit(item)','OnDelete(item)','invoice-payment-description'])
  assert.ok(view.includes(term),term);
 assert.match(logic,/_license.IsExpired/);
 assert.match(logic,/Items.Remove\(model\)/);
 assert.match(logic,/Session.Close\(Items\)/);
});
test('invoice workspace includes an always-visible settlement sidebar and accessible actions',()=>{
 const view=read('EditorForm.razor');
 const css=read('EditorForm.razor.css');
 assert.match(view,/class="invoice-workspace-columns"/);
 assert.match(view,/id="invoice-finance"/);
 assert.match(view,/class="invoice-financial-actions"/);
 for(const label of ['تسویه فاکتور','تسویه‌ها','تخفیف','مخارج']) assert.ok(view.includes(label),label);
 assert.match(view,/OpenDiscountsDialog/);
 assert.match(view,/OpenExtraCostsDialog/);
 assert.doesNotMatch(view,/<MudExpansionPanel/);
 assert.match(css,/\.invoice-workspace-side\{[^}]*position:sticky/);
 assert.match(css,/font-weight:500/);
 assert.match(view,/class="invoice-form-actions"/);
 assert.match(view,/class="invoice-form-actions-primary"/);
 assert.match(view,/class="invoice-form-actions-danger"/);
 assert.match(view,/Class="invoice-form-action-delete"/);
 const dialogStyle=read('PaymentDialog.razor.css');
 assert.match(dialogStyle,/\.invoice-settlement-direction-info strong\{font-weight:500\}/);
});
