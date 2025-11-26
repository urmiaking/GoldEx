window.reportingCustomization = {
    onBeforeRender: function (s, e) {
        // Set the desired zoom level here
        // -1: Page Width
        // 0: Whole Page
        // A positive number represents the percentage (e.g., 1 for 100%)
        e.reportPreview.zoom = 0; // Example: Set to Whole Page
    }
};