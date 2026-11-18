const invoicePrintButton = document.querySelector('[data-print-invoice]');

if (invoicePrintButton) {
    invoicePrintButton.hidden = false;
    invoicePrintButton.addEventListener('click', async () => {
        invoicePrintButton.disabled = true;
        try {
            const images = Array.from(document.querySelectorAll('.invoice-sheet img'));
            await Promise.allSettled([document.fonts.ready, ...images.map(image => image.decode())]);
            window.print();
        } finally {
            invoicePrintButton.disabled = false;
        }
    });
}
