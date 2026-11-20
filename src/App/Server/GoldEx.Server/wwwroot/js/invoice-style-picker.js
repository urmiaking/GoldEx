(() => {
    const picker = document.querySelector("[data-invoice-style-picker]");
    if (!picker) return;

    const sheet = document.querySelector(".invoice-sheet");
    const track = picker.querySelector("[data-invoice-design-track]");
    const themes = [...track.querySelectorAll("[data-invoice-theme-option]")];
    const colors = [...picker.querySelectorAll("[data-invoice-color-option]")];
    const root = document.documentElement;
    const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)");
    const storageKey = "goldex.invoicePrintStyle";
    let animation;
    let pointerStart;
    let suppressClick = false;
    let saved;
    try {
        saved = JSON.parse(localStorage.getItem(storageKey));
    } catch {
        // Printing still works when storage is unavailable or contains an invalid value.
    }

    let activeTheme = Math.max(0, themes.findIndex(button => button.dataset.invoiceThemeOption === saved?.theme));
    let activeColor = Math.max(0, colors.findIndex(button => button.dataset.invoiceColorOption === saved?.color));

    function applyStyle(persist) {
        const theme = themes[activeTheme].dataset.invoiceThemeOption;
        const color = colors[activeColor].dataset.invoiceColorOption;
        root.dataset.invoiceTheme = theme;
        root.dataset.invoiceColor = color;
        themes.forEach((button, index) => button.setAttribute("aria-pressed", String(index === activeTheme)));
        colors.forEach((button, index) => button.setAttribute("aria-pressed", String(index === activeColor)));
        if (persist) {
            try {
                localStorage.setItem(storageKey, JSON.stringify({ theme, color }));
            } catch {
                // Keep the current selection for this preview.
            }
        }
    }

    function revealTheme(behavior = reduceMotion.matches ? "instant" : "smooth") {
        const left = themes[activeTheme].getBoundingClientRect().left;
        const trackLeft = track.getBoundingClientRect().left;
        const centeredLeft = trackLeft + (track.clientWidth - themes[activeTheme].offsetWidth) / 2;
        track.scrollBy({ left: left - centeredLeft, behavior });
    }

    function selectTheme(index, direction, focus = false) {
        activeTheme = (index + themes.length) % themes.length;
        if (focus) themes[activeTheme].focus({ preventScroll: true });
        applyStyle(true);
        revealTheme();
        animation?.cancel();
        if (!reduceMotion.matches) {
            animation = sheet.animate([
                { transform: `translateX(${direction * -24}px)`, opacity: .28 },
                { transform: "translateX(0)", opacity: 1 }
            ], { duration: 320, easing: "cubic-bezier(.22, .75, .26, 1)" });
        }
    }

    themes.forEach((button, index) => button.addEventListener("click", event => {
        if (suppressClick && event.detail > 0) return;
        if (index !== activeTheme) selectTheme(index, Math.sign(index - activeTheme));
    }));
    colors.forEach((button, index) => button.addEventListener("click", () => {
        activeColor = index;
        applyStyle(true);
    }));
    picker.querySelectorAll("[data-invoice-step]").forEach(button => button.addEventListener("click", () => {
        const step = Number(button.dataset.invoiceStep);
        selectTheme(activeTheme + step, step);
    }));
    track.addEventListener("keydown", event => {
        if (event.key !== "ArrowLeft" && event.key !== "ArrowRight") return;
        event.preventDefault();
        const step = event.key === "ArrowLeft" ? 1 : -1;
        selectTheme(activeTheme + step, step, true);
    });

    track.addEventListener("pointerdown", event => {
        if (!event.isPrimary || event.button !== 0) return;
        suppressClick = false;
        pointerStart = { id: event.pointerId, x: event.clientX, y: event.clientY };
    });
    track.addEventListener("pointerup", event => {
        if (pointerStart?.id !== event.pointerId) return;
        const x = event.clientX - pointerStart.x;
        const y = event.clientY - pointerStart.y;
        pointerStart = undefined;
        if (Math.abs(x) < 36 || Math.abs(x) < Math.abs(y) * 1.3) return;
        suppressClick = true;
        const step = Math.sign(x);
        selectTheme(activeTheme + step, step);
    });
    track.addEventListener("pointercancel", () => { pointerStart = undefined; });
    window.addEventListener("beforeprint", () => animation?.cancel());

    applyStyle(false);
    picker.hidden = false;
    revealTheme("instant");
})();
