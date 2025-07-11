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
              align-items: center;
              justify-content: space-between;
              /* height: 100%; */
              padding: 0.02in;
              flex-grow: 1; /* Allows this section to take up remaining space */
            }

            .barcode-img {
              max-width: 1.4in;
              height: 0.18in;
              object-fit: fill; /* Stretches barcode to fill the space */
            }

            .name-text {
              font-size: 5pt;
              text-align: center;
            }

            /* Right section containing weight and wage */
            .right-section {
              display: flex;
              flex-direction: column;
              justify-content: space-around;
              align-items: center;
              height: 100%;
              padding: 0.02in;
              width: 0.6in; /* Fixed width for the right column */
            }

            .right-text {
              font-size: 7pt;
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
        <body onload="window.focus(); window.print();">
          <!-- The actual label structure -->
          <div class="printable-label">
              <div class="left-section">
                  <img src="${barcodeUrl}" class="barcode-img" alt="barcode" />
                  <div class="name-text">${name}</div>
              </div>
              <div class="right-section">
                  <div class="right-text">${weight}</div>
                  <div class="right-text">${wage}</div>
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