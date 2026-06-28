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
    const marginTop = settings.marginTop ?? 1.0;
    const marginRight = settings.marginRight ?? 1.0;
    const marginBottom = settings.marginBottom ?? 1.0;
    const marginLeft = settings.marginLeft ?? 1.0;
    const tailWidth = settings.tailWidth ?? 30.0;
    const sideWidth = (settings.labelWidth - tailWidth) / 2.0;

    return `
        <!DOCTYPE html>
        <html dir="rtl" lang="fa">
        <head>
            <meta charset="UTF-8">
            <title>چاپ بارکد</title>
            <script src="${window.location.origin}/js/libs/jsbarcode.all.min.js"></script>
            <style>
                @page {
                    size: ${settings.labelWidth}mm ${settings.labelHeight}mm;
                    margin: ${marginTop}mm ${marginRight}mm ${marginBottom}mm ${marginLeft}mm;
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
                    background: white;
                }
                
                body {
                    direction: rtl;
                    display: flex;
                    justify-content: center;
                    align-items: center;
                }
                
                .label-container {
                    width: ${settings.labelWidth}mm;
                    height: ${settings.labelHeight}mm;
                    position: relative;
                    background: white;
                    overflow: hidden;
                }
                
                .label-side {
                    height: 100%;
                    position: absolute;
                    top: 0;
                    box-sizing: border-box;
                    display: flex;
                    flex-direction: column;
                    justify-content: space-between;
                }
                
                .left-side {
                    left: 0;
                    width: ${sideWidth}mm;
                    padding: ${settings.paddingTop}mm ${settings.paddingRight}mm ${settings.paddingBottom}mm ${settings.paddingLeft}mm;
                    text-align: right;
                }
                
                .right-side {
                    right: 0;
                    width: ${sideWidth}mm;
                    padding: ${settings.paddingTop}mm ${settings.paddingRight}mm ${settings.paddingBottom}mm ${settings.paddingLeft}mm;
                    text-align: left;
                }
                
                .position-area {
                    display: flex;
                    flex-direction: column;
                    width: 100%;
                    flex-shrink: 0;
                }
                
                .left-side .position-area {
                    align-items: flex-end;
                }
                
                .right-side .position-area {
                    align-items: flex-start;
                }
                
                .item {
                    line-height: 1.0;
                    white-space: nowrap;
                    flex-shrink: 0;
                }

                .barcode-svg {
                    display: block;
                    flex-shrink: 0;
                }
                
                @media print {
                    html, body {
                        width: ${settings.labelWidth}mm !important;
                        height: ${settings.labelHeight}mm !important;
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
                <!-- بخش چپ برچسب (حاوی بالا راست و پایین راست) -->
                <div class="label-side left-side">
                    ${generatePositionHtml('TopLeft', settings, data)}
                    ${generatePositionHtml('BottomLeft', settings, data)}
                </div>

                <!-- بخش راست برچسب (حاوی بالا چپ و پایین چپ) -->
                <div class="label-side right-side">
                    ${generatePositionHtml('TopRight', settings, data)}
                    ${generatePositionHtml('BottomRight', settings, data)}
                </div>
            </div>
        </body>
        </html>
    `;
}

function generatePositionHtml(position, settings, data) {
    const items = settings.positionItems
        .filter(x => x.position === position && x.isVisible)
        .sort((a, b) => a.order - b.order);

    if (items.length === 0) return '<div></div>';

    const positionClass = position.replace(/([A-Z])/g, '-$1').toLowerCase().substring(1);

    let html = `<div class="position-area ${positionClass}">`;

    items.forEach(item => {
        html += generateItemHtml(item, data, position);
    });

    html += '</div>';

    return html;
}

function generateItemHtml(item, data, position) {
    const isBottom = position.startsWith('Bottom');
    const marginProp = isBottom ? 'margin-top' : 'margin-bottom';

    const style = `font-size: ${item.fontSize}pt; ${marginProp}: ${item.itemSpacing}mm; font-weight: 500;`;

    switch (item.itemType) {
        case 'Barcode':
            const barcodeSettings = item.barcodeSettings || {
                width: 22.0,
                height: 8.0,
                displayValue: true,
                fontSize: 7.0,
                margin: 0.0,
                barWidthMultiplier: 2
            };

            let html = `<div class="item" style="display: flex; flex-direction: column; align-items: center; justify-content: center; ${marginProp}: ${item.itemSpacing}mm;">`;
            html += `<svg class="barcode-svg barcode-element" 
                        data-barcode="${escapeHtml(data.barcode)}" 
                        data-width-multiplier="${barcodeSettings.barWidthMultiplier}"
                        style="width: ${barcodeSettings.width}mm; height: ${barcodeSettings.height}mm; display: block;"></svg>`;
            if (barcodeSettings.displayValue) {
                html += `<div style="font-family: monospace; font-size: ${barcodeSettings.fontSize}pt; font-weight: bold; text-align: center; margin-top: 0.3mm; line-height: 1; color: #000;">${escapeHtml(data.barcode)}</div>`;
            }
            html += `</div>`;
            return html;

        case 'ProductName':
            if (!data.productName) return '';
            return `<div class="item" style="${style} font-family: 'B Nazanin', Arial; font-weight: bold;">
                        ${escapeHtml(data.productName)}
                    </div>`;

        case 'Weight':
            if (!data.weight) return '';
            return `<div class="item" style="${style}">
                        ${escapeHtml(data.weight)}
                    </div>`;

        case 'Wage':
            if (!data.wage) return '';
            return `<div class="item" style="${style}">
                        ${escapeHtml(data.wage)}
                    </div>`;

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
        const widthMultiplier = parseInt(svg.getAttribute('data-width-multiplier')) || 2;

        if (!value) return;

        try {
            // Keep the user-defined physical width and height styling intact
            const originalStyle = svg.getAttribute('style');

            printWindow.JsBarcode(svg, value, {
                format: "CODE128",
                width: widthMultiplier,
                height: 100, // Render tall in SVG (defines viewbox proportions)
                displayValue: false, // handled in HTML instead to prevent overlapping
                background: "#ffffff",
                lineColor: "#000000",
                margin: 0,
                valid: function (valid) {
                    if (!valid) {
                        console.error('Invalid barcode value:', value);
                    }
                }
            });

            // Post-process the SVG to allow custom stretching via CSS
            svg.setAttribute('preserveAspectRatio', 'none');
            svg.removeAttribute('width');
            svg.removeAttribute('height');

            // Forcefully restore styling so that physical dimensions are applied correctly
            if (originalStyle) {
                svg.setAttribute('style', originalStyle);
            }
        } catch (error) {
            console.error('Error generating barcode:', error, 'Value:', value);
            svg.style.display = 'none';
        }
    });
}

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
