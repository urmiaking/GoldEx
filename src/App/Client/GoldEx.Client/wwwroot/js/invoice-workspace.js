// Imported on demand so navigation works after SSR, hydration and enhanced navigation.
const allowedSections = new Set(["invoice-core", "invoice-customer", "invoice-lines", "invoice-finance"]);

export function scrollToSection(sectionId) {
    if (!allowedSections.has(sectionId)) return;
    const target = document.getElementById(sectionId);
    if (!target) return;
    const reducedMotion = window.matchMedia?.("(prefers-reduced-motion: reduce)").matches ?? false;
    target.scrollIntoView({
        behavior: reducedMotion ? "auto" : "smooth",
        block: "start",
        inline: "nearest"
    });
}
