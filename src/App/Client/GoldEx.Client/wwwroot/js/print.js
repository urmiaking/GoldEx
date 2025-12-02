
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
                /*@import '../fonts/IRANSANS/IRANSANS-font-face.css';*/
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
                    /*font-family: Tahoma, sans-serif;*/
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
    const isBottom = position.startsWith('Bottom');
    const marginProp = isBottom ? 'margin-top' : 'margin-bottom';

    const style = `font-size: ${item.fontSize}px; ${marginProp}: ${item.itemSpacing}px; font-weight: 500;`;

    switch (item.itemType) {
    case 'Barcode':
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
                        style="${marginProp}: ${item.itemSpacing}px;"></svg>`;

    case 'ProductName':
        return `<div class="item" style="${style} font-family: 'B Nazanin'"><strong>${escapeHtml(data.productName || 'نام محصول')}</strong></div>`;

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