// Adapted from Mkx.Templates.Blazor; shared with GoldEx theme state via PalettePicker.razor.
// Pointer capture handles mouse/pen; explicit non-passive Touch Events keep phone gestures out of native scrolling.
export function bind(trigger, popover) {
    const host = trigger.closest('.gex-palette-floating');
    const storageKey = 'GoldEx.PickerPosition';
    let fraction = .5, pointer = null, touch = null, suppressClick = false, lastTouch = -Infinity, frame = 0;
    try {
        const saved = localStorage.getItem(storageKey);
        if (saved !== null && saved.trim() !== '' && Number.isFinite(Number(saved))) fraction = Math.max(0, Math.min(1, Number(saved)));
    } catch { /* Dragging also works with blocked storage. */ }
    const viewport = () => {
        const visual = window.visualViewport;
        return { top: visual?.offsetTop || 0, width: Math.min(visual?.width || innerWidth, document.documentElement.clientWidth),
            height: visual?.height || innerHeight };
    };
    const range = () => {
        const view = viewport();
        const header = document.querySelector('.gex-topbar')?.getBoundingClientRect();
        const nav = document.querySelector('.gex-bottom-nav')?.getBoundingClientRect();
        const min = Math.max(view.top + 12, (header?.bottom || 0) + 12);
        const bottom = nav?.height ? Math.min(view.top + view.height, nav.top) : view.top + view.height;
        return { min, max: Math.max(min, bottom - trigger.offsetHeight - 12) };
    };
    const save = () => { try { localStorage.setItem(storageKey, String(fraction)); } catch { } };
    const move = y => {
        const bounds = range();
        const top = Math.max(bounds.min, Math.min(y, bounds.max));
        fraction = bounds.max > bounds.min ? (top - bounds.min) / (bounds.max - bounds.min) : .5;
        host.style.top = `${top}px`;
    };
    const placeTab = () => {
        const bounds = range();
        host.style.top = `${bounds.min + fraction * (bounds.max - bounds.min)}px`;
    };
    const position = () => {
        if (!trigger.isConnected || !popover.matches(':popover-open')) return;
        const view = viewport();
        popover.style.maxWidth = `${view.width - 12}px`;
        popover.style.maxHeight = `${view.height - 24}px`;
        const width = popover.offsetWidth, height = popover.offsetHeight;
        const bounds = trigger.getBoundingClientRect();
        const top = Math.max(view.top + 12, Math.min(bounds.top + bounds.height / 2 - height / 2, view.top + view.height - height - 12));
        const center = Math.max(28, Math.min(bounds.top + bounds.height / 2 - top, height - 28));
        popover.style.left = '0px';
        popover.style.top = `${top}px`;
        popover.style.setProperty('--gex-palette-origin-y', `${center}px`);
        // A narrow neck grows directly out of the tab into the rounded sheet, with no detached card.
        popover.querySelector('.gex-palette-sheet-shape path').setAttribute('d',
            `M 0 ${center - 28} C 26 ${center - 28} 18 0 66 0 H ${width - 26} Q ${width} 0 ${width} 26 V ${height - 26} Q ${width} ${height} ${width - 26} ${height} H 66 C 18 ${height} 26 ${center + 28} 0 ${center + 28} Z`);
    };
    const opening = event => {
        if (event.newState === 'open') { cancelAnimationFrame(frame); frame = requestAnimationFrame(position); }
    };
    const toggled = () => {
        const open = popover.matches(':popover-open');
        trigger.setAttribute('aria-expanded', String(open));
        if (open) position();
    };
    const close = () => { if (popover.matches(':popover-open')) popover.hidePopover(); };
    const resized = () => { placeTab(); position(); };
    const pointerDown = event => {
        if (event.pointerType === 'touch' || !event.isPrimary || event.button !== 0 || touch) return;
        suppressClick = false;
        pointer = { id: event.pointerId, y: event.clientY, x: event.clientX, top: host.getBoundingClientRect().top, fraction, dragged: false };
        trigger.setPointerCapture(event.pointerId);
    };
    const pointerMove = event => {
        if (!pointer || event.pointerId !== pointer.id) return;
        const dy = event.clientY - pointer.y;
        if (!pointer.dragged && Math.hypot(dy, event.clientX - pointer.x) < 6) return;
        pointer.dragged = true;
        host.dataset.dragging = '';
        close();
        move(pointer.top + dy);
        event.preventDefault();
    };
    const finishPointer = event => {
        if (!pointer || event.pointerId !== pointer.id) return;
        const finished = pointer;
        pointer = null;
        delete host.dataset.dragging;
        if (event.type === 'pointercancel') { fraction = finished.fraction; placeTab(); }
        else if (finished.dragged) { suppressClick = true; save(); }
        if (trigger.hasPointerCapture(event.pointerId)) trigger.releasePointerCapture(event.pointerId);
    };
    // A canceled touchstart suppresses compatibility clicks. Also reject delayed ghost clicks on mobile browsers.
    const touchStart = event => {
        if (event.touches.length !== 1 || pointer) return;
        const point = event.changedTouches[0];
        touch = { id: point.identifier, x: point.clientX, y: point.clientY, top: host.getBoundingClientRect().top, fraction, dragged: false };
        suppressClick = false;
        if (event.cancelable) event.preventDefault();
        document.addEventListener('touchmove', touchMove, { passive: false });
        document.addEventListener('touchend', touchEnd, { passive: false });
        document.addEventListener('touchcancel', touchEnd, { passive: false });
    };
    const touchMove = event => {
        if (!touch) return;
        const point = [...event.changedTouches].find(point => point.identifier === touch.id);
        if (!point) return;
        if (event.cancelable) event.preventDefault();
        const dy = point.clientY - touch.y;
        if (!touch.dragged && Math.hypot(dy, point.clientX - touch.x) < 6) return;
        touch.dragged = true;
        host.dataset.dragging = '';
        close();
        move(touch.top + dy);
    };
    const touchEnd = event => {
        if (!touch || ![...event.changedTouches].some(point => point.identifier === touch.id)) return;
        if (event.cancelable) event.preventDefault();
        const finished = touch;
        touch = null;
        detachTouch();
        lastTouch = performance.now();
        delete host.dataset.dragging;
        if (event.type === 'touchcancel') { fraction = finished.fraction; placeTab(); }
        else if (finished.dragged) { suppressClick = true; save(); }
        else trigger.click();
    };
    const detachTouch = () => {
        document.removeEventListener('touchmove', touchMove);
        document.removeEventListener('touchend', touchEnd);
        document.removeEventListener('touchcancel', touchEnd);
    };
    const click = event => {
        if (event.detail > 0 && performance.now() - lastTouch < 700) {
            event.preventDefault(); event.stopImmediatePropagation(); return;
        }
        if (!suppressClick) return;
        suppressClick = false;
        event.preventDefault();
        event.stopImmediatePropagation();
    };
    const keydown = event => {
        suppressClick = false;
        if (!['ArrowUp', 'ArrowDown', 'Home', 'End'].includes(event.key)) return;
        event.preventDefault();
        close();
        const bounds = range(), top = host.getBoundingClientRect().top;
        move(event.key === 'Home' ? bounds.min : event.key === 'End' ? bounds.max : top + (event.key === 'ArrowUp' ? -24 : 24));
        save();
    };
    placeTab();
    trigger.addEventListener('touchstart', touchStart, { passive: false });
    trigger.addEventListener('pointerdown', pointerDown);
    trigger.addEventListener('pointermove', pointerMove);
    trigger.addEventListener('pointerup', finishPointer);
    trigger.addEventListener('pointercancel', finishPointer);
    trigger.addEventListener('lostpointercapture', finishPointer);
    trigger.addEventListener('click', click, true);
    trigger.addEventListener('keydown', keydown);
    popover.addEventListener('beforetoggle', opening);
    popover.addEventListener('toggle', toggled);
    window.addEventListener('resize', resized);
    window.visualViewport?.addEventListener('resize', resized);
    window.visualViewport?.addEventListener('scroll', resized);
    host.dataset.ready = '';
    return { close,
        dispose() {
        close();
        cancelAnimationFrame(frame);
        trigger.removeEventListener('touchstart', touchStart);
        detachTouch();
        trigger.removeEventListener('pointerdown', pointerDown);
        trigger.removeEventListener('pointermove', pointerMove);
        trigger.removeEventListener('pointerup', finishPointer);
        trigger.removeEventListener('pointercancel', finishPointer);
        trigger.removeEventListener('lostpointercapture', finishPointer);
        trigger.removeEventListener('click', click, true);
        trigger.removeEventListener('keydown', keydown);
        popover.removeEventListener('beforetoggle', opening);
        popover.removeEventListener('toggle', toggled);
        window.removeEventListener('resize', resized);
        window.visualViewport?.removeEventListener('resize', resized);
        window.visualViewport?.removeEventListener('scroll', resized);
    } };
}
