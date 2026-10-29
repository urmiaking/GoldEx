// Adapted from Mkx's workspace navigation. Only the mobile modal owns inert and scroll locking.
export function bind(panel) {
    const mobile = matchMedia('(max-width: 959.98px)');
    const shell = panel.closest('.goldex-shell');
    let expanded = false, modal = false, previous = null;
    const savedInert = new Map();
    const controls = () => [...panel.querySelectorAll('a[href],button:not([disabled])')]
        .filter(element => element.getClientRects().length && !element.closest('[inert]'));
    const release = () => {
        for (const [element, inert] of savedInert) element.inert = inert;
        savedInert.clear();
        document.documentElement.classList.remove('gex-navigation-open');
        panel.setAttribute('role', 'complementary');
        panel.removeAttribute('aria-modal');
    };
    const sync = () => {
        const nextModal = expanded && mobile.matches;
        if (nextModal === modal) return;
        modal = nextModal;
        if (modal) {
            previous = document.activeElement;
            for (const popover of shell.querySelectorAll('.gex-palette-sheet:popover-open')) popover.hidePopover();
            for (const element of shell.querySelectorAll('.gex-workspace,.gex-bottom-nav,.gex-skip-link,.gex-palette-floating')) {
                savedInert.set(element, element.inert);
                element.inert = true;
            }
            document.documentElement.classList.add('gex-navigation-open');
            panel.setAttribute('role', 'dialog');
            panel.setAttribute('aria-modal', 'true');
            (controls()[0] ?? panel).focus({ preventScroll: true });
        } else {
            release();
            if (previous?.isConnected && mobile.matches) previous.focus({ preventScroll: true });
            previous = null;
        }
    };
    const keydown = event => {
        if (!modal || event.key !== 'Tab') return;
        const all = controls(), first = all[0], last = all.at(-1);
        if (!first) { event.preventDefault(); panel.focus({ preventScroll: true }); return; }
        if (event.shiftKey && (document.activeElement === first || !panel.contains(document.activeElement))) {
            event.preventDefault(); last.focus();
        } else if (!event.shiftKey && (document.activeElement === last || !panel.contains(document.activeElement))) {
            event.preventDefault(); first.focus();
        }
    };
    // Dismiss the live Blazor store picker on click-away and Escape, on
    // desktop AND mobile. Clicking the original trigger preserves all
    // existing Blazor state/animation and prevents a JS/Blazor state split.
    const dismissStore = event => {
        const trigger = panel.querySelector('.gex-store-picker-trigger[aria-expanded="true"]');
        if (!trigger || trigger.disabled) return;
        if (event.type === 'pointerdown' && event.target.closest?.('.gex-store-picker')) return;
        if (event.type === 'keydown' && event.key !== 'Escape') return;
        trigger.click();
    };
    document.addEventListener('pointerdown', dismissStore, true);
    document.addEventListener('keydown', dismissStore);
    panel.addEventListener('keydown', keydown);
    mobile.addEventListener('change', sync);
    return {
        update(open) { expanded = open; sync(); },
        dispose() {
            modal = expanded = false;
            release();
            document.removeEventListener('pointerdown', dismissStore, true);
            document.removeEventListener('keydown', dismissStore);
            panel.removeEventListener('keydown', keydown);
            mobile.removeEventListener('change', sync);
        }
    };
}
