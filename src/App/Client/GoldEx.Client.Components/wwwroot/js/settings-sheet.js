// Native modal keeps focus inside the mobile settings sheet and supports Escape.
export function bind(panel) {
    let previousFocus;

    const release = () => {
        if (panel.open) return;
        document.documentElement.classList.remove('gex-settings-open');
        if (previousFocus?.isConnected) previousFocus.focus({ preventScroll: true });
        previousFocus = null;
    };

    const close = () => {
        if (panel.open) panel.close();
        release();
    };

    const open = () => {
        if (panel.open || !window.matchMedia('(max-width: 959.98px)').matches) return;
        previousFocus = document.activeElement;
        panel.showModal();
        document.documentElement.classList.add('gex-settings-open');
    };

    const onClick = event => {
        if (event.target.closest('[data-settings-close]')) {
            close();
            return;
        }
        if (event.target !== panel) return;
        const bounds = panel.getBoundingClientRect();
        if (event.clientX < bounds.left || event.clientX > bounds.right ||
            event.clientY < bounds.top || event.clientY > bounds.bottom) close();
    };

    const onResize = () => {
        if (!window.matchMedia('(max-width: 959.98px)').matches) close();
    };

    panel.addEventListener('click', onClick);
    panel.addEventListener('close', release);
    window.addEventListener('resize', onResize);

    return {
        open,
        close,
        dispose() {
            close();
            panel.removeEventListener('click', onClick);
            panel.removeEventListener('close', release);
            window.removeEventListener('resize', onResize);
        }
    };
}
