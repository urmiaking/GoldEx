/*
 * DevExpress JS-based Document Viewer 24.2.7.
 * Public BeforeRender and CustomizeMenuActions; native commands are untouched.
 */
(function () {
    "use strict";
    const glyphs = {
        print: '<path d="M7 8V3h10v5M7 17H5a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-2"/><path d="M7 14h10v7H7z"/>',
        search: '<circle cx="10.8" cy="10.8" r="6.8"/><path d="m16 16 5 5"/>',
        export: '<path d="M12 3v12m0 0-4-4m4 4 4-4"/><path d="M4 16v4h16v-4"/>',
        fullscreen: '<path d="M8 3H3v5m13-5h5v5M3 16v5h5m13-5v5h-5"/>',
        zoomIn: '<circle cx="10.6" cy="10.6" r="7"/><path d="M10.6 7v7m-3.5-3.5h7M16 16l5 5"/>',
        zoomOut: '<circle cx="10.6" cy="10.6" r="7"/><path d="M7.1 10.6h7M16 16l5 5"/>'
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
    window.reportingCustomization = {
        onBeforeRender: function (_sender, args) {
            if (!args || !args.reportPreview) return;
            // -1 = Page Width, 0 = Whole Page. Desktop unchanged.
            args.reportPreview.zoom = window.innerWidth < 960 ? -1 : 0;
        },
        onCustomizeMenuActions: function (_sender, args) {
            if (!args || typeof args.GetById !== "function") return;
            const ids = window.DevExpress && window.DevExpress.Reporting &&
                        window.DevExpress.Reporting.Viewer && window.DevExpress.Reporting.Viewer.ActionId;
            if (!ids) return;
            registerGlyphs();
            const commands = {
                Print: "print", PrintPage: "print",
                Search: "search", ExportTo: "export",
                FullScreen: "fullscreen", Fullscreen: "fullscreen",
                ZoomIn: "zoomIn", ZoomOut: "zoomOut"
            };
            for (const [name, glyph] of Object.entries(commands)) {
                if (ids[name] === undefined) continue;
                const action = args.GetById(ids[name]);
                if (action) action.imageTemplateName = "gex-report-icon-" + glyph;
            }
        }
    };
})();
