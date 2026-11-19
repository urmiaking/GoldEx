const invoicePrintButtons = [...document.querySelectorAll('[data-print-invoice]')];
const invoiceDocument = document.documentElement;

function finishInvoicePrint() {
    delete invoiceDocument.dataset.invoicePrintMode;
    for (const button of invoicePrintButtons) button.disabled = false;
}

window.addEventListener('afterprint', finishInvoicePrint);

for (const button of invoicePrintButtons) {
    button.hidden = false;
    button.addEventListener('click', async () => {
        for (const action of invoicePrintButtons) action.disabled = true;
        try {
            const images = Array.from(document.querySelectorAll('.invoice-sheet img'));
            await Promise.allSettled([document.fonts.ready, ...images.map(image => image.decode())]);
            invoiceDocument.dataset.invoicePrintMode = button.dataset.printInvoice;
            window.print();
        } catch (error) {
            finishInvoicePrint();
            throw error;
        }
    });
}
