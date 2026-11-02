// Native tabs. Scroll arrows describe physical screen directions, independent of RTL scrollLeft.
export function bind(root) {
    const bar = root.querySelector('.gex-calculator-tabbar');
    const viewport = bar.querySelector('.gex-calculator-tab-viewport');
    const track = bar.querySelector('[role="tablist"]');
    const left = bar.querySelector('[data-tab-scroll="left"]');
    const right = bar.querySelector('[data-tab-scroll="right"]');
    const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)');
    let frame = 0, disposed = false;
    const tabs = () => [...track.querySelectorAll('[role="tab"]')];
    const selected = () => track.querySelector('[role="tab"][aria-selected="true"]');
    const behavior = () => reducedMotion.matches ? 'auto' : 'smooth';
    const update = () => {
        if (disposed) return;
        // Measure the total bar before reserving arrow space to avoid a resize feedback loop.
        const overflow = track.scrollWidth > bar.clientWidth + 1;
        bar.classList.toggle('has-overflow', overflow);
        left.hidden = right.hidden = !overflow;
        const view = viewport.getBoundingClientRect();
        const content = track.getBoundingClientRect();
        left.disabled = !overflow || content.left >= view.left - 1;
        right.disabled = !overflow || content.right <= view.right + 1;
        if (!overflow && (document.activeElement === left || document.activeElement === right)) selected()?.focus({ preventScroll: true });
    };
    const schedule = () => {
        if (frame || disposed) return;
        frame = requestAnimationFrame(() => { frame = 0; update(); });
    };
    const reveal = tab => {
        if (!tab) return;
        const view = viewport.getBoundingClientRect(), bounds = tab.getBoundingClientRect();
        const delta = bounds.left < view.left ? bounds.left - view.left : bounds.right > view.right ? bounds.right - view.right : 0;
        if (delta) viewport.scrollBy({ left: delta, behavior: behavior() });
    };
    const normalize = () => {
        for (const tab of tabs()) tab.tabIndex = tab.getAttribute('aria-selected') === 'true' ? 0 : -1;
    };
    const scrollLeft = () => viewport.scrollBy({ left: -Math.max(120, viewport.clientWidth * .7), behavior: behavior() });
    const scrollRight = () => viewport.scrollBy({ left: Math.max(120, viewport.clientWidth * .7), behavior: behavior() });
    const keydown = event => {
        const tab = event.target.closest('[role="tab"]');
        if (!tab || !track.contains(tab)) return;
        const items = tabs(), index = items.indexOf(tab);
        const rtl = getComputedStyle(track).direction === 'rtl';
        let next;
        if (event.key === 'ArrowLeft') next = (index + (rtl ? 1 : -1) + items.length) % items.length;
        else if (event.key === 'ArrowRight') next = (index + (rtl ? -1 : 1) + items.length) % items.length;
        else if (event.key === 'Home') next = 0;
        else if (event.key === 'End') next = items.length - 1;
        else return; // Native button Enter/Space activation remains with Blazor's click handler.
        event.preventDefault();
        for (const item of items) item.tabIndex = -1;
        items[next].tabIndex = 0;
        items[next].focus({ preventScroll: true });
        reveal(items[next]);
    };
    const focusout = event => { if (!track.contains(event.relatedTarget)) normalize(); };
    const changed = () => { update(); normalize(); reveal(selected()); };
    const resized = () => { update(); reveal(selected()); };
    const resize = new ResizeObserver(resized);
    resize.observe(bar);
    resize.observe(track);
    const mutation = new MutationObserver(changed);
    mutation.observe(track, { childList: true, subtree: true, attributes: true, attributeFilter: ['aria-selected'] });
    left.addEventListener('click', scrollLeft);
    right.addEventListener('click', scrollRight);
    viewport.addEventListener('scroll', schedule, { passive: true });
    track.addEventListener('keydown', keydown);
    track.addEventListener('focusout', focusout);
    changed();
    return {
        dispose() {
            disposed = true;
            if (frame) cancelAnimationFrame(frame);
            resize.disconnect(); mutation.disconnect();
            left.removeEventListener('click', scrollLeft);
            right.removeEventListener('click', scrollRight);
            viewport.removeEventListener('scroll', schedule);
            track.removeEventListener('keydown', keydown);
            track.removeEventListener('focusout', focusout);
        }
    };
}
