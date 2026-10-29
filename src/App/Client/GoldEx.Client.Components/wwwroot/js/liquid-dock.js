// Desktop GoldEx Liquid Dock: dynamic no-scroll overflow and accessible flyout.
// Mobile drawer and its existing modal/focus-lock lifecycle are unchanged.
export function bind(dock) {
    const media = matchMedia('(min-width: 960px)');
    const navigation = dock.querySelector('.gex-navigation-menu');
    const viewport = dock.querySelector('.gex-menu-scroll');
    const more = dock.querySelector('.gex-dock-more');
    const trigger = more.querySelector('.gex-dock-more-trigger');
    const flyout = more.querySelector('.gex-dock-overflow-panel');
    const outlet = more.querySelector('.gex-dock-overflow-links');
    let disposed = false;
    let scheduled = 0;
    let open = false;

    const allLinks = () => [...navigation.querySelectorAll(':scope > a[href]')];
    const setOpen = value => {
        open = Boolean(value && media.matches);
        more.classList.toggle('is-open', open);
        trigger.setAttribute('aria-expanded', String(open));
        flyout.setAttribute('aria-hidden', String(media.matches && !open));
        flyout.inert = media.matches && !open;
    };

    // The full list remains owned by Blazor. Overflow links are presentation
    // proxies that click the original anchor; no NavLink or event handler is moved.
    const refresh = () => {
        if (disposed) return;
        if (!media.matches) {
            for (const link of allLinks()) link.removeAttribute('data-dock-overflow');
            outlet.replaceChildren();
            setOpen(false);
            return;
        }
        const links = allLinks();
        // 52px icon row including gap. Leave the overflow control and an
        // unobstructed bottom account action; never create an inner scrollbar.
        const storePicker = viewport.querySelector('.gex-store-picker');
        const storeHeight = storePicker ? Math.ceil(storePicker.getBoundingClientRect().height + 9) : 0;
        const available = Math.max(0, viewport.clientHeight - storeHeight);
        const maxVisible = Math.max(0, Math.floor((available - 4) / 52));
        const keep = Math.min(links.length, maxVisible);
        const extras = links.slice(keep);

        for (let index = 0; index < links.length; index++) {
            const link = links[index];
            link.setAttribute('data-dock-overflow', String(index >= keep));
            const label = link.querySelector('span')?.textContent?.trim() || link.textContent.trim();
            link.setAttribute('aria-label', label);
            link.title = label;
        }
        const fragment = document.createDocumentFragment();
        for (const original of extras) {
            const proxy = original.cloneNode(true);
            proxy.removeAttribute('data-dock-overflow');
            proxy.classList.add('gex-dock-overflow-link');
            proxy.removeAttribute('id');
            proxy.addEventListener('click', event => {
                event.preventDefault();
                setOpen(false);
                original.click();
            });
            fragment.appendChild(proxy);
        }
        outlet.replaceChildren(fragment);
        more.classList.toggle('has-overflow', extras.length > 0);
        more.classList.toggle('has-many', extras.length > 5);
        more.classList.toggle('has-very-many', extras.length > 10);
    };
    const queue = () => {
        if (disposed || scheduled) return;
        scheduled = requestAnimationFrame(() => {
            scheduled = 0;
            refresh();
        });
    };
    const toggle = () => setOpen(!open);
    const outside = event => {
        if (open && !more.contains(event.target)) setOpen(false);
    };
    const keydown = event => {
        if (event.key !== 'Escape' || !open) return;
        event.preventDefault();
        setOpen(false);
        trigger.focus({ preventScroll: true });
    };
    const onNavigation = event => {
        if (event.target.closest('a[href]')) setOpen(false);
    };
    const onChange = () => {
        setOpen(false);
        queue();
    };
    trigger.addEventListener('click', toggle);
    document.addEventListener('pointerdown', outside);
    dock.addEventListener('keydown', keydown);
    navigation.addEventListener('click', onNavigation);
    media.addEventListener('change', onChange);
    window.addEventListener('resize', queue, { passive: true });
    window.addEventListener('popstate', onChange);

    const resize = new ResizeObserver(queue);
    resize.observe(viewport);
    const mutation = new MutationObserver(queue);
    mutation.observe(viewport, {
        childList: true, subtree: true, attributes: true,
        attributeFilter: ['class', 'href']
    });
    setOpen(false);
    queue();

    return {
        recalculate: refresh,
        close: () => setOpen(false),
        dispose() {
            disposed = true;
            if (scheduled) cancelAnimationFrame(scheduled);
            setOpen(false);
            resize.disconnect();
            mutation.disconnect();
            trigger.removeEventListener('click', toggle);
            document.removeEventListener('pointerdown', outside);
            dock.removeEventListener('keydown', keydown);
            navigation.removeEventListener('click', onNavigation);
            media.removeEventListener('change', onChange);
            window.removeEventListener('resize', queue);
            window.removeEventListener('popstate', onChange);
            for (const link of allLinks()) {
                link.removeAttribute('data-dock-overflow');
                link.removeAttribute('title');
            }
            outlet.replaceChildren();
        }
    };
}
