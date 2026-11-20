(() => {
    const picker = document.querySelector("[data-invoice-style-picker]");
    const sheet = document.querySelector(".invoice-sheet");
    const title = document.querySelector("[data-invoice-theme-name]");
    if (!picker || !sheet || !title) return;

    const arrows = document.querySelectorAll("[data-invoice-step]");
    const themes = [
        { id: "engraved", name: "زرنگار" },
        { id: "deco", name: "آرت‌دکو" },
        { id: "atelier", name: "ویترین" },
        { id: "minimal", name: "مینیمال" }
    ];
    const colors = [...picker.querySelectorAll("[data-invoice-color-option]")];
    const root = document.documentElement;
    const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)");
    const storageKey = "goldex.invoicePrintStyle";
    let animation;
    let pointerStart;
    let saved;
    try {
        saved = JSON.parse(localStorage.getItem(storageKey));
    } catch {
        // Printing still works when storage is unavailable or contains an invalid value.
    }

    let activeTheme = Math.max(0, themes.findIndex(theme => theme.id === saved?.theme));
    let activeColor = Math.max(0, colors.findIndex(button => button.dataset.invoiceColorOption === saved?.color));

    function applyStyle(persist) {
        const theme = themes[activeTheme];
        const color = colors[activeColor].dataset.invoiceColorOption;
        root.dataset.invoiceTheme = theme.id;
        root.dataset.invoiceColor = color;
        if (title.textContent !== theme.name) title.textContent = theme.name;
        colors.forEach((button, index) => button.setAttribute("aria-pressed", String(index === activeColor)));
        if (persist) {
            try {
                localStorage.setItem(storageKey, JSON.stringify({ theme: theme.id, color }));
            } catch {
                // Keep the current selection for this preview.
            }
        }
    }

    function stepTheme(direction) {
        activeTheme = (activeTheme + direction + themes.length) % themes.length;
        applyStyle(true);
        animation?.cancel();
        if (!reduceMotion.matches) {
            animation = sheet.animate([
                { transform: `translateX(${direction * -24}px)`, opacity: .28 },
                { transform: "translateX(0)", opacity: 1 }
            ], { duration: 320, easing: "cubic-bezier(.22, .75, .26, 1)" });
        }
    }

    colors.forEach((button, index) => button.addEventListener("click", () => {
        activeColor = index;
        applyStyle(true);
    }));
    arrows.forEach(button => {
        button.addEventListener("click", () => stepTheme(Number(button.dataset.invoiceStep)));
        button.addEventListener("keydown", event => {
            if (event.key !== "ArrowLeft" && event.key !== "ArrowRight") return;
            event.preventDefault();
            stepTheme(event.key === "ArrowLeft" ? 1 : -1);
        });
        button.hidden = false;
    });

    sheet.addEventListener("pointerdown", event => {
        if (!event.isPrimary || event.pointerType === "mouse" || event.target.closest("a, button")) return;
        pointerStart = { id: event.pointerId, x: event.clientX, y: event.clientY };
    });
    sheet.addEventListener("pointerup", event => {
        if (pointerStart?.id !== event.pointerId) return;
        const x = event.clientX - pointerStart.x;
        const y = event.clientY - pointerStart.y;
        pointerStart = undefined;
        if (Math.abs(x) < 36 || Math.abs(x) < Math.abs(y) * 1.3) return;
        stepTheme(Math.sign(x));
    });
    sheet.addEventListener("pointercancel", () => { pointerStart = undefined; });
    window.addEventListener("beforeprint", () => animation?.cancel());

    applyStyle(false);
    picker.hidden = false;
    title.hidden = false;
})();
