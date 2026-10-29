/*
 * GoldEx / DevExpress JS-based Document Viewer 24.2.7.
 * Uses official BeforeRender and CustomizeMenuActions events.
 * Built-in clickAction / execute remain the vendor's own implementations.
 */
(function () {
    "use strict";

    const glyphs = {
        print: '<path d="M7 8V3h10v5M7 17H5a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-2"/><path d="M7 14h10v7H7z"/>',
        search: '<circle cx="10.8" cy="10.8" r="6.8"/><path d="m16 16 5 5"/>',
        // Export is a dropdown (not a direct file download). Use a document/export icon.
        export: '<path d="M13 3H5a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-8"/><path d="M13 3v6h6M19 3v7m0 0-3-3m3 3 3-3"/>',
        fullscreen: '<path d="M8 3H3v5m13-5h5v5M3 16v5h5m13-5v5h-5"/>',
        zoomIn: '<circle cx="10.6" cy="10.6" r="7"/><path d="M10.6 7v7m-3.5-3.5h7M16 16l5 5"/>',
        zoomOut: '<circle cx="10.6" cy="10.6" r="7"/><path d="M7.1 10.6h7M16 16l5 5"/>',
        previous: '<path d="m15 6-6 6 6 6"/>',
        next: '<path d="m9 6 6 6-6 6"/>'
    };

    function registerGlyphs() {
        for (const [name, shape] of Object.entries(glyphs)) {
            const id = "gex-report-icon-" + name;
            if (document.getElementById(id)) continue;
            const template = document.createElement("script");
            template.type = "text/html";
            template.id = id;
            template.innerHTML = '<svg class="gex-report-glyph" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">' + shape + "</svg>";
            document.head.appendChild(template);
        }
    }

    function customizeExportAction(action) {
        if (!action) return;
        const templateName = "gex-report-icon-export";
        // The ExportTo toolbar command is a complex dxrd-preview-export-to
        // dropdown. DevExpress documents items()[0].imageTemplateName, not
        // merely action.imageTemplateName (support ticket T1107801).
        const items = typeof action.items === "function" ? action.items() : action.items;
        if (Array.isArray(items) && items.length > 0) {
            items[0].imageTemplateName = templateName;
        } else {
            // Older/flat variants still support this action-level template.
            action.imageTemplateName = templateName;
        }
    }

    window.reportingCustomization = {
        onBeforeRender: function (_sender, args) {
            if (!args || !args.reportPreview) return;
            args.reportPreview.zoom = window.innerWidth < 960 ? -1 : 0;
        },
        onCustomizeMenuActions: function (_sender, args) {
            if (!args || typeof args.GetById !== "function") return;
            const ids = window.DevExpress && window.DevExpress.Reporting &&
                window.DevExpress.Reporting.Viewer && window.DevExpress.Reporting.Viewer.ActionId;
            if (!ids) return;
            registerGlyphs();

            // Three navigation controls are enough: previous, page picker,
            // next. First/Last and MultiPage make a one-page invoice look
            // cluttered and are still reachable through the page selector.
            for (const name of ["FirstPage", "LastPage", "MultipageToggle"]) {
                if (ids[name] === undefined) continue;
                const action = args.GetById(ids[name]);
                if (action) action.visible = false;
            }
            const commands = {
                Print: "print", PrintPage: "print",
                Search: "search", FullScreen: "fullscreen",
                Fullscreen: "fullscreen", ZoomIn: "zoomIn", ZoomOut: "zoomOut",
                PrevPage: "previous", NextPage: "next"
            };
            for (const [name, glyph] of Object.entries(commands)) {
                if (ids[name] === undefined) continue;
                const action = args.GetById(ids[name]);
                if (action) action.imageTemplateName = "gex-report-icon-" + glyph;
            }
            if (ids.ExportTo !== undefined) customizeExportAction(args.GetById(ids.ExportTo));
        }
    };
})();
