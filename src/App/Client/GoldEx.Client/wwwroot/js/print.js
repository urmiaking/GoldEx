/**
 * Generates a barcode image from text and returns it as a Base64 data URL.
 * Requires the JsBarcode library to be included in your project.
 * @param {string} text - The text or data to encode in the barcode.
 * @returns {string} - A data URL representing the barcode image.
 */
function textToBase64Barcode(text) {
    const canvas = document.createElement("canvas");
    try {
        // JsBarcode is expected to be available globally
        JsBarcode(canvas, text, {
            format: "CODE128",
            displayValue: true, // We handle text display ourselves
            margin: 0,
            width: 2,          // Bar width
            height: 50         // Bar height
        });
        return canvas.toDataURL("image/png");
    } catch (e) {
        console.error("JsBarcode error: ", e);
        return ""; // Return empty string on error
    }
}

/**
 * Creates and prints a label with a barcode, name, weight, and wage.
 * @param {object} params - The parameters for the label.
 * @param {string} params.text - The data for the barcode.
 * @param {string} params.name - The product name to display.
 * @param {string} params.weight - The weight to display.
 * @param {string} params.wage - The wage/price to display.
 */
function printBarcode({ text, name, weight, wage }) {
    // Create a hidden iframe to load the content for printing
    const iframe = document.createElement('iframe');
    iframe.style.position = 'absolute';
    iframe.style.width = '0';
    iframe.style.height = '0';
    iframe.style.border = '0';
    document.body.appendChild(iframe);

    // Generate the barcode image URL
    const barcodeUrl = textToBase64Barcode(text);
    if (!barcodeUrl) {
        console.error("Could not generate barcode.");
        document.body.removeChild(iframe); // Clean up iframe
        return;
    }

    // This HTML content includes all necessary CSS for the specific label dimensions.
    // It uses physical units (inches, points) to ensure it prints correctly.
    const htmlContent = `
      <html>

        <head>
            <title>پرینت بارکد</title>
            <style>
                body {
                    margin: 0;
                    padding: 0;
                }

                * {
                    font-family: "IRANSans", "B Nazanin", Tahoma, sans-serif;
                }

                /* Styles for the printable label container. */
                .printable-label {
                    width: 2.09in;
                    height: 0.33in;
                    box-sizing: border-box;
                    display: flex;
                    flex-direction: row;
                    justify-content: space-between;
                    align-items: flex-start;
                    overflow: hidden;
                }

                /* Left section containing barcode and name */
                .left-section {
                    display: flex;
                    flex-direction: column;
                    align-items: flex-end;
                    /* Stick to left horizontally */
                    justify-content: flex-end;
                    /* Stick to top vertically */
                    padding: 0;
                    /* Remove padding */
                    margin: 0;
                    flex-grow: 1;
                }

                .barcode-img {
                    max-width: 1.4in;
                    height: 0.18in;
                    object-fit: fill;
                    margin: 0;
                }

                .name-text {
                    font-size: 5pt;
                    text-align: right;
                    /* Adjust for RTL */
                    margin: 0;
                }

                /* Right section containing weight and wage */
                .right-section {
                    display: flex;
                    flex-direction: column;
                    justify-content: space-around;
                    align-items: center;
                    padding: 0.02in;
                    width: 0.6in;
                    /* Fixed width for the right column */
                }

                .right-text {
                    font-size: 5pt;
                    font-weight: bold;
                    text-align: center;
                }

                /* Print-specific rules */
                @media print {

                    /* Define the exact page size for the printer */
                    @page {
                        size: 2.09in 0.33in;
                        margin: 0;
                    }
                }
            </style>
        </head>

        <body dir=rtl onload="window.focus(); window.print();">
            <!-- The actual label structure -->
            <div class="printable-label">
                <div class="right-section">
                    <div class="right-text">${weight}</div>
                    <div class="right-text">${wage}</div>
                </div>
                <div class="left-section">
                    <img src="${barcodeUrl}" class="barcode-img" alt="barcode" />
                    <div class="name-text">${name}</div>
                </div>

            </div>
        </body>

        </html>
    `;

    // Write the complete HTML into the iframe
    const doc = iframe.contentWindow.document;
    doc.open();
    doc.write(htmlContent);
    doc.close();

    // Clean up the iframe after printing is likely complete
    setTimeout(() => {
        document.body.removeChild(iframe);
    }, 1500);
}

/**
 * چاپ بارکد با تنظیمات دینامیک و کیفیت بالا (SVG)
 * @param {Object} settings - تنظیمات قالب بارکد
 * @param {Object} data - داده‌های واقعی برای چاپ
 */
window.printDynamicBarcode = function (settings, data) {
    const printWindow = window.open('', '', 'height=800,width=600');

    if (!printWindow) {
        alert('لطفا popup blocker را غیرفعال کنید');
        return;
    }

    const html = generateDynamicBarcodeHtml(settings, data);

    printWindow.document.write(html);
    printWindow.document.close();

    printWindow.onload = function () {
        generateAllBarcodes(printWindow, data.barcode);

        setTimeout(() => {
            printWindow.focus();
            printWindow.print();

            // انصراف خودکار پنجره
            setTimeout(() => {
                printWindow.close();
            }, 100);
        }, 500);
    };
};

function generateDynamicBarcodeHtml(settings, data) {
    return `
        <!DOCTYPE html>
        <html dir="rtl" lang="fa">
        <head>
            <meta charset="UTF-8">
            <title>چاپ بارکد</title>
            <script src="${window.location.origin}/js/libs/jsbarcode.all.min.js"></script>
            <style>
                @import '../fonts/IRANSANS/IRANSANS-font-face.css';
                @page {
                    size: ${settings.labelWidth}px ${settings.labelHeight}px;
                    margin: 0;
                }
                
                * {
                    margin: 0;
                    padding: 0;
                    box-sizing: border-box;
                }
                
                html, body {
                    width: 100%;
                    height: 100%;
                    margin: 0;
                    padding: 0;
                    overflow: hidden;
                }
                
                body {
                    font-family: "IRANSans", "B Homa", "B Nazanin", Tahoma, sans-serif;
                    direction: rtl;
                    background: white;
                    display: flex;
                    justify-content: center;
                    align-items: center;
                }
                
                .label-container {
                    width: ${settings.labelWidth}px;
                    height: ${settings.labelHeight}px;
                    padding: ${settings.paddingTop}px ${settings.paddingRight}px ${settings.paddingBottom}px ${settings.paddingLeft}px;
                    position: relative;
                    background: white;
                }
                
                .position-area {
                    position: absolute;
                    display: flex;
                    flex-direction: column;
                }
                
                .top-left {
                    top: ${settings.paddingTop}px;
                    left: ${settings.paddingLeft}px;
                }
                
                .top-right {
                    top: ${settings.paddingTop}px;
                    right: ${settings.paddingRight}px;
                }
                
                .bottom-left {
                    bottom: ${settings.paddingBottom}px;
                    left: ${settings.paddingLeft}px;
                }
                
                .bottom-right {
                    bottom: ${settings.paddingBottom}px;
                    right: ${settings.paddingRight}px;
                }
                
                .item {
                    line-height: 1.3;
                }
                
                @media print {
                    html, body {
                        width: ${settings.labelWidth}px !important;
                        height: ${settings.labelHeight}px !important;
                        overflow: hidden !important;
                    }
                    
                    .label-container {
                        -webkit-print-color-adjust: exact;
                        print-color-adjust: exact;
                    }
                }
            </style>
        </head>
        <body>
            <div class="label-container">
                ${generatePositionHtml('TopLeft', settings, data)}
                ${generatePositionHtml('TopRight', settings, data)}
                ${generatePositionHtml('BottomLeft', settings, data)}
                ${generatePositionHtml('BottomRight', settings, data)}
            </div>
        </body>
        </html>
    `;
}

/**
 * دریافت کمترین مقدار spacing
 */
function getItemSpacings(items) {
    const spacings = items.map(x => x.itemSpacing || 5);
    return spacings.length > 0 ? spacings : [5];
}

/**
 * تولید HTML برای هر موقعیت
 */
function generatePositionHtml(position, settings, data) {
    const items = settings.positionItems
        .filter(x => x.position === position && x.isVisible)
        .sort((a, b) => a.order - b.order);

    if (items.length === 0) return '';

    const positionClass = position.replace(/([A-Z])/g, '-$1').toLowerCase().substring(1);

    let html = `<div class="position-area ${positionClass}">`;

    items.forEach(item => {
        html += generateItemHtml(item, data, position);
    });

    html += '</div>';

    return html;
}

/**
 * تولید HTML برای هر آیتم
 */
function generateItemHtml(item, data, position) {
    const style = `font-size: ${item.fontSize}px; margin-bottom: ${item.itemSpacing}px; font-weight: 500;`;

    switch (item.itemType) {
        case 'Barcode':
            // دریافت تنظیمات بارکد
            const barcodeSettings = item.barcodeSettings || {
                width: 2,
                height: 50,
                displayValue: true,
                fontSize: 14,
                margin: 0
            };

            return `<svg class="barcode-svg barcode-element" 
                        data-barcode="${escapeHtml(data.barcode)}" 
                        data-position="${position}"
                        data-width="${barcodeSettings.width}"
                        data-height="${barcodeSettings.height}"
                        data-display-value="${barcodeSettings.displayValue}"
                        data-font-size="${barcodeSettings.fontSize}"
                        data-margin="${barcodeSettings.margin}"
                        style="margin-bottom: ${item.itemSpacing}px;"></svg>`;

        case 'ProductName':
            return `<div class="item" style="${style}"><strong>${escapeHtml(data.productName || 'نام محصول')}</strong></div>`;

        case 'Weight':
            return `<div class="item" style="${style}">${escapeHtml(data.weight || 'وزن')}</div>`;

        case 'Wage':
            return `<div class="item" style="${style}">${escapeHtml(data.wage || 'اجرت')}</div>`;

        default:
            return '';
    }
}

function generateAllBarcodes(printWindow, barcodeValue) {
    if (!printWindow || !printWindow.JsBarcode) {
        console.error('JsBarcode library not loaded in print window');
        return;
    }

    const barcodeElements = printWindow.document.querySelectorAll('.barcode-element');

    barcodeElements.forEach(svg => {
        const value = svg.getAttribute('data-barcode');
        const width = parseInt(svg.getAttribute('data-width')) || 2;
        const height = parseInt(svg.getAttribute('data-height')) || 50;
        const displayValue = svg.getAttribute('data-display-value') === 'true';
        const fontSize = parseInt(svg.getAttribute('data-font-size')) || 14;
        const margin = parseInt(svg.getAttribute('data-margin')) || 0;

        if (!value) return;

        try {
            printWindow.JsBarcode(svg, value, {
                format: "CODE128",
                width: width,
                height: height,
                displayValue: displayValue,
                fontSize: fontSize,
                font: "monospace",
                fontOptions: "bold",
                textAlign: "center",
                textPosition: "bottom",
                textMargin: 2,
                background: "#ffffff",
                lineColor: "#000000",
                margin: margin,
                marginTop: margin,
                marginBottom: margin,
                marginLeft: margin,
                marginRight: margin,
                valid: function (valid) {
                    if (!valid) {
                        console.error('Invalid barcode value:', value);
                    }
                }
            });
        } catch (error) {
            console.error('Error generating barcode:', error, 'Value:', value);
            svg.innerHTML = `<text x="50%" y="50%" text-anchor="middle" font-size="12" fill="#000">${value}</text>`;
        }
    });
}

/**
 * Escape HTML برای امنیت
 */
function escapeHtml(text) {
    if (!text) return '';

    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };

    return String(text).replace(/[&<>"']/g, m => map[m]);
}

// نگهداری تابع قبلی برای سازگاری با کدهای موجود
window.printBarcode = function (labelData) {
    console.warn('Using legacy printBarcode. Consider migrating to printDynamicBarcode.');

    // تبدیل به فرمت جدید
    const settings = {
        labelWidth: 300,
        labelHeight: 150,
        marginTop: 5,
        marginRight: 5,
        marginBottom: 5,
        marginLeft: 5,
        paddingTop: 10,
        paddingRight: 10,
        paddingBottom: 10,
        paddingLeft: 10,
        positionItems: [
            {
                position: 'TopLeft',
                itemType: 'Barcode',
                order: 0,
                isVisible: true,
                fontSize: 14,
                itemSpacing: 5
            },
            {
                position: 'TopRight',
                itemType: 'ProductName',
                order: 0,
                isVisible: true,
                fontSize: 12,
                itemSpacing: 3
            },
            {
                position: 'BottomLeft',
                itemType: 'Weight',
                order: 0,
                isVisible: true,
                fontSize: 11,
                itemSpacing: 3
            },
            {
                position: 'BottomRight',
                itemType: 'Wage',
                order: 0,
                isVisible: true,
                fontSize: 11,
                itemSpacing: 3
            }
        ]
    };

    const data = {
        barcode: labelData.text || '',
        productName: labelData.name || '',
        weight: labelData.weight || '',
        wage: labelData.wage || ''
    };

    printDynamicBarcode(settings, data);
};