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
    const stopDialogPopovers = watchDialogPopovers(dialog);
    const focusedSessions = new Map();
    const rememberFocus = event => {
        const session = event.target.closest('[data-dialog-session]');
        if (session) focusedSessions.set(session.getAttribute('data-dialog-session'), event.target);
    };
    // VisualViewport scroll can fire every frame while a user scrolls behind
    // a sheet. Only geometry changes (e.g. keyboard resize) need a layout write.
    let lastHeight, lastOffset;
    const resize = () => {
        const height = `${Math.round(viewport?.height ?? window.innerHeight)}px`;
        const offset = `${Math.round(viewport?.offsetTop ?? 0)}px`;
        if (height !== lastHeight) {
            dialog.style.setProperty('--gex-modal-viewport-height', height);
            lastHeight = height;
        }
        if (offset !== lastOffset) {
            dialog.style.setProperty('--gex-modal-viewport-offset', offset);
            lastOffset = offset;
        }
        // Reposition menus when the keyboard changes the visual viewport.
        placeDialogPopovers(dialog);
    };
    dialog.addEventListener('cancel', preventCancel);
    dialog.addEventListener('keydown', trap);
    dialog.addEventListener('focusin', rememberFocus);
    viewport?.addEventListener('resize', resize);
    window.addEventListener('resize', resize);
    dialogs.set(dialog, { previous, preventCancel, trap, resize, viewport, rememberFocus, focusedSessions, stopDialogPopovers });
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
    state.stopDialogPopovers?.();
    state.viewport?.removeEventListener('resize', state.resize);
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

// Clamp MudSelect/MudAutocomplete menus to the usable body of the native dialog.
function placeDialogPopovers(dialog) {
    const panel = dialog.querySelector('.gex-modal-panel');
    const overlay = dialog.querySelector('.gex-modal-overlays');
    if (!panel || !overlay || !dialog.classList?.contains('is-active')) return;
    const box = panel.getBoundingClientRect(), header = panel.querySelector('.gex-modal-header');
    const footer = panel.querySelector('.gex-editor-actions, .gex-customer-history-actions');
    const topMin = Math.max(box.top + 8, (header?.getBoundingClientRect().bottom ?? box.top) + 8);
    const bottomMax = Math.min(box.bottom - 8, (footer?.getBoundingClientRect().top ?? box.bottom) - 8);
    const leftMin = Math.max(10, box.left + 8), rightMax = Math.min(window.innerWidth - 10, box.right - 8);
    if (bottomMax - topMin < 36 || rightMax - leftMin < 40) return;
    for (const popup of overlay.querySelectorAll('.mud-popover-open[id^="popovercontent-"]')) {
        const marker = document.getElementById('popover-' + popup.id.slice(15));
        if (!marker || !panel.contains(marker)) continue;
        const container = marker.parentElement, list = popup.querySelector('.mud-list');
        // The MudPopover marker can be wrapped in a full-width container.
        // Measure the actual SELECT input, not the popup or its portal parent.
        const control = container?.closest('.mud-select, .mud-autocomplete');
        if (!control || !list || control.closest('[inert], [hidden]')) continue;
        const anchor = control.getBoundingClientRect();
        if (!anchor.width || !anchor.height) continue;
        // Select menus should match their field, not expand to the whole dialog.
        const width = Math.min(anchor.width, rightMax - leftMin);
        const below = Math.max(0, bottomMax - anchor.bottom - 4);
        const above = Math.max(0, anchor.top - topMin - 4);
        const natural = Math.max(48, list.scrollHeight || popup.scrollHeight || 180);
        // Desktop menus stay below their input with an internal scrollbar.
        // On a small screen, use the larger side when the full list will not fit.
        const up = window.innerWidth <= 640
            ? natural > below && above > below
            : below < 112 && above > below;
        const height = Math.max(0, Math.min(320, up ? above : below));
        const x = Math.max(leftMin, Math.min(anchor.right - width, rightMax - width));
        const y = up ? anchor.top - 4 - Math.min(natural, height) : anchor.bottom + 4;
        if (!popup.classList.contains('gex-dialog-bound-popover'))
            popup.classList.add('gex-dialog-bound-popover');
        popup.style.setProperty('--gex-popup-left', Math.round(x) + 'px');
        popup.style.setProperty('--gex-popup-top', Math.round(Math.max(topMin, y)) + 'px');
        popup.style.setProperty('--gex-popup-height', Math.round(height) + 'px');
        popup.style.setProperty('--gex-popup-width', Math.round(width) + 'px');
    }
}

function watchDialogPopovers(dialog) {
    const overlay = dialog.querySelector('.gex-modal-overlays');
    if (!overlay || typeof MutationObserver === 'undefined') return () => {};
    let frame = 0;
    const schedule = () => {
        if (!frame) frame = window.requestAnimationFrame(() => {
            frame = 0;
            placeDialogPopovers(dialog);
        });
    };
    const observer = new MutationObserver(schedule);
    observer.observe(overlay, { subtree: true, childList: true, attributes: true, attributeFilter: ['class'] });
    dialog.addEventListener('scroll', schedule, true);
    const ro = typeof ResizeObserver === 'function' ? new ResizeObserver(schedule) : null;
    const panel = dialog.querySelector('.gex-modal-panel');
    if (panel) ro?.observe(panel);
    schedule();
    return () => {
        observer.disconnect();
        dialog.removeEventListener('scroll', schedule, true);
        ro?.disconnect();
        if (frame) window.cancelAnimationFrame(frame);
    };
}
