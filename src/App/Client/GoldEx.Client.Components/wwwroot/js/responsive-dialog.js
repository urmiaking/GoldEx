// Adapted from MeetFlow's native modal lifecycle. Browser modality owns inertness.
const dialogs = new Map();
let scrollStyles;

export function setOpen(dialog, open) {
    if (open) activate(dialog);
    else {
        deactivate(dialog, true);
        if (!dialog.open) dialog.show();
    }
}

export function focusSession(dialog, key) {
    const state = dialogs.get(dialog);
    if (!state) return;
    const remembered = state.focusedSessions.get(key);
    if (remembered?.isConnected && remembered.getClientRects().length) remembered.focus({ preventScroll: true });
    else (dialog.querySelector('.gex-modal-close:not(:disabled)') ?? dialog).focus({ preventScroll: true });
}

export function canDismiss(dialog) {
    if ([...dialogs.keys()].at(-1) !== dialog) return false;
    return ![...dialog.querySelectorAll('.mud-popover-open:not(.mud-tooltip)')].some(node => node.getClientRects().length);
}

export function activate(dialog) {
    if (!dialog || dialogs.has(dialog)) return;
    const previous = document.activeElement;
    const preventCancel = event => event.preventDefault();
    const focusable = () => [...dialog.querySelectorAll(
        'button:not([disabled]):not([tabindex="-1"]),input:not([disabled]):not([type="hidden"]),select:not([disabled]),textarea:not([disabled]),a[href],[tabindex]:not([tabindex="-1"])'
    )].filter(node => node.getClientRects().length && !node.closest('[inert]'));
    const trap = event => {
        if (event.key !== 'Tab') return;
        const all = focusable(), first = all[0], last = all.at(-1);
        if (!first) { event.preventDefault(); dialog.focus({ preventScroll: true }); return; }
        if (event.shiftKey && (document.activeElement === first || document.activeElement === dialog)) {
            event.preventDefault(); last.focus({ preventScroll: true });
        } else if (!event.shiftKey && document.activeElement === last) {
            event.preventDefault(); first.focus({ preventScroll: true });
        }
    };
    const viewport = window.visualViewport;
    const focusedSessions = new Map();
    const rememberFocus = event => {
        const session = event.target.closest('[data-dialog-session]');
        if (session) focusedSessions.set(session.getAttribute('data-dialog-session'), event.target);
    };
    const resize = () => {
        dialog.style.setProperty('--gex-modal-viewport-height', `${viewport?.height ?? window.innerHeight}px`);
        dialog.style.setProperty('--gex-modal-viewport-offset', `${viewport?.offsetTop ?? 0}px`);
    };
    dialog.addEventListener('cancel', preventCancel);
    dialog.addEventListener('keydown', trap);
    dialog.addEventListener('focusin', rememberFocus);
    viewport?.addEventListener('resize', resize);
    viewport?.addEventListener('scroll', resize);
    window.addEventListener('resize', resize);
    dialogs.set(dialog, { previous, preventCancel, trap, resize, viewport, rememberFocus, focusedSessions });
    if (dialogs.size === 1) {
        scrollStyles = { overflow: document.documentElement.style.overflow, overscroll: document.documentElement.style.overscrollBehavior };
        document.documentElement.style.overflow = 'hidden';
        document.documentElement.style.overscrollBehavior = 'none';
        document.documentElement.classList.add('gex-responsive-dialog-open');
    }
    resize();
    if (dialog.open) dialog.close();
    dialog.showModal();
    (focusable()[0] ?? dialog).focus({ preventScroll: true });
}

export function deactivate(dialog, keepNonModal = false) {
    const state = dialogs.get(dialog);
    if (!state) return;
    dialog.removeEventListener('cancel', state.preventCancel);
    dialog.removeEventListener('keydown', state.trap);
    dialog.removeEventListener('focusin', state.rememberFocus);
    state.viewport?.removeEventListener('resize', state.resize);
    state.viewport?.removeEventListener('scroll', state.resize);
    window.removeEventListener('resize', state.resize);
    if (dialog.open) dialog.close();
    dialogs.delete(dialog);
    if (keepNonModal) dialog.show();
    if (!dialogs.size) {
        document.documentElement.style.overflow = scrollStyles.overflow;
        document.documentElement.style.overscrollBehavior = scrollStyles.overscroll;
        document.documentElement.classList.remove('gex-responsive-dialog-open');
        scrollStyles = undefined;
    }
    if (state.previous?.isConnected) state.previous.focus({ preventScroll: true });
}
