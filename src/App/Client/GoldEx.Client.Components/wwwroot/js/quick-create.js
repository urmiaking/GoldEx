// Native action sheet: the browser owns modal focus containment and background inertness.
export function bind(trigger, panel) {
    const mobile = matchMedia('(max-width: 959.98px)');
    const shell = trigger.closest('.goldex-shell');
    const release = () => {
        if (panel.open) return;
        document.documentElement.classList.remove('gex-quick-create-open');
        trigger.setAttribute('aria-expanded', 'false');
    };
    const close = () => { if (panel.open) panel.close(); release(); };
    const open = () => {
        if (!mobile.matches || panel.open) return;
        for (const popover of shell.querySelectorAll('[popover]:popover-open')) popover.hidePopover();
        panel.showModal();
        document.documentElement.classList.add('gex-quick-create-open');
        trigger.setAttribute('aria-expanded', 'true');
    };
    const click = event => {
        if (event.target.closest('[data-quick-close],a[href]')) { close(); return; }
        if (event.target !== panel) return;
        const bounds = panel.getBoundingClientRect();
        if (event.clientX < bounds.left || event.clientX > bounds.right || event.clientY < bounds.top || event.clientY > bounds.bottom) close();
    };
    const resized = () => { if (!mobile.matches) close(); };
    trigger.addEventListener('click', open);
    panel.addEventListener('click', click);
    panel.addEventListener('close', release);
    mobile.addEventListener('change', resized);
    return {
        close,
        dispose() {
            close();
            trigger.removeEventListener('click', open);
            panel.removeEventListener('click', click);
            panel.removeEventListener('close', release);
            mobile.removeEventListener('change', resized);
        }
    };
}
