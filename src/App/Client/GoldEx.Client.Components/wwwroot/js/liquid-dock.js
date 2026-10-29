// Desktop-only GoldEx Liquid Dock. Original NavLinks remain owned by Blazor;
// overflow links are delegates; the connected glass lobe and active droplet
// are single shared elements, never per-link popovers.
export function bind(dock) {
    const media = matchMedia('(min-width: 960px)');
    const navigation = dock.querySelector('.gex-navigation-menu');
    const viewport = dock.querySelector('.gex-menu-scroll');
    const more = dock.querySelector('.gex-dock-more');
    const trigger = more.querySelector('.gex-dock-more-trigger');
    const flyout = more.querySelector('.gex-dock-overflow-panel');
    const outlet = more.querySelector('.gex-dock-overflow-links');
    const drop = dock.querySelector('.gex-dock-active-drop');
    const hoverSelector = '.gex-navigation-menu > a, .gex-sidebar-create, .gex-store-picker-trigger, .gex-identity-link, .gex-signout';
    let disposed = false, scheduled = 0, open = false;
    let selected = null, pendingSelection = null;

    const allLinks = () => [...navigation.querySelectorAll(':scope > a[href]')];
    const setOpen = value => {
        open = Boolean(value && media.matches);
        more.classList.toggle('is-open', open);
        trigger.setAttribute('aria-expanded', String(open));
        flyout.setAttribute('aria-hidden', String(media.matches && !open));
        flyout.inert = media.matches && !open;
    };
    const positionTop = element => Math.round(
        element.getBoundingClientRect().top - dock.getBoundingClientRect().top
    );
    const setLobe = element => {
        if (!media.matches || !element) {
            dock.classList.remove('gex-dock-has-hover');
            return;
        }
        const isStore = element.matches('.gex-store-picker-trigger');
        const isAccount = Boolean(element.closest('.gex-sidebar-identity'));
        const isCreate = element.matches('.gex-sidebar-create');
        const width = isStore ? 260 : isAccount ? 238 : isCreate ? 207 : 220;
        dock.style.setProperty('--gex-lobe-top', positionTop(element) + 'px');
        dock.style.setProperty('--gex-lobe-width', width + 'px');
        dock.classList.add('gex-dock-has-hover');
    };
    const setDrop = (element, animate = true) => {
        if (!media.matches || !element) {
            dock.classList.remove('gex-dock-has-selection', 'gex-dock-drop-traveling');
            selected = null;
            return;
        }
        if (animate && selected && selected !== element) {
            dock.classList.add('gex-dock-drop-traveling');
        }
        dock.style.setProperty('--gex-drop-top', (positionTop(element) + 2) + 'px');
        dock.classList.add('gex-dock-has-selection');
        selected = element;
    };
    const syncSelection = () => {
        const active = allLinks().find(link => link.classList.contains('active')
            || link.getAttribute('aria-current') === 'page');
        if (pendingSelection && active !== pendingSelection && pendingSelection.isConnected !== false) {
            setDrop(pendingSelection.getAttribute('data-dock-overflow') === 'true' ? trigger : pendingSelection);
            return;
        }
        pendingSelection = null;
        setDrop(active?.getAttribute('data-dock-overflow') === 'true' ? trigger : active);
    };

    const refresh = () => {
        if (disposed) return;
        if (!media.matches) {
            for (const link of allLinks()) link.removeAttribute('data-dock-overflow');
            outlet.replaceChildren();
            setOpen(false);
            setLobe(null);
            setDrop(null);
            return;
        }
        const links = allLinks();
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
                if (original.getAttribute('target') !== '_blank') {
                    pendingSelection = original;
                    setDrop(trigger);
                }
                original.click();
            });
            fragment.appendChild(proxy);
        }
        outlet.replaceChildren(fragment);
        more.classList.toggle('has-overflow', extras.length > 0);
        more.classList.toggle('has-many', extras.length > 5);
        more.classList.toggle('has-very-many', extras.length > 10);
        syncSelection();
    };
    const queue = () => {
        if (disposed || scheduled) return;
        scheduled = requestAnimationFrame(() => {
            scheduled = 0;
            refresh();
        });
    };
    const over = event => {
        if (!media.matches) return;
        const item = event.target.closest?.(hoverSelector);
        if (item && dock.contains(item)) setLobe(item);
    };
    const out = event => {
        if (!media.matches) return;
        const next = event.relatedTarget?.closest?.(hoverSelector);
        if (next && dock.contains(next)) setLobe(next);
        else setLobe(null);
    };
    const activate = event => {
        if (!media.matches || !event.target.closest) return;
        const link = event.target.closest('.gex-navigation-menu > a[href]');
        if (!link || link.getAttribute('target') === '_blank') return;
        pendingSelection = link;
        setDrop(link.getAttribute('data-dock-overflow') === 'true' ? trigger : link);
        setOpen(false);
    };
    const toggle = () => setOpen(!open);
    const outside = event => {
        if (open && !more.contains(event.target)) setOpen(false);
    };
    const keydown = event => {
        if (event.key !== 'Escape') return;
        if (open) {
            event.preventDefault();
            setOpen(false);
            trigger.focus({ preventScroll: true });
        } else setLobe(null);
    };
    const onNavigation = event => {
        if (event.target.closest('a[href]')) setOpen(false);
    };
    const onChange = () => {
        pendingSelection = null;
        setOpen(false);
        setLobe(null);
        queue();
    };
    const onTransitionEnd = event => {
        if (event.target === drop && event.propertyName === 'top') {
            dock.classList.remove('gex-dock-drop-traveling');
        }
    };
    trigger.addEventListener('click', toggle);
    document.addEventListener('pointerdown', outside);
    dock.addEventListener('keydown', keydown);
    dock.addEventListener('pointerover', over);
    dock.addEventListener('pointerout', out);
    dock.addEventListener('focusin', over);
    dock.addEventListener('focusout', out);
    drop.addEventListener('transitionend', onTransitionEnd);
    navigation.addEventListener('click', activate);
    navigation.addEventListener('click', onNavigation);
    media.addEventListener('change', onChange);
    window.addEventListener('resize', queue, { passive: true });
    window.addEventListener('popstate', onChange);

    const resize = new ResizeObserver(queue);
    resize.observe(viewport);
    const mutation = new MutationObserver(queue);
    mutation.observe(viewport, {
        childList: true, subtree: true, attributes: true,
        attributeFilter: ['class', 'href', 'aria-current']
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
            setLobe(null);
            setDrop(null);
            resize.disconnect();
            mutation.disconnect();
            trigger.removeEventListener('click', toggle);
            document.removeEventListener('pointerdown', outside);
            dock.removeEventListener('keydown', keydown);
            dock.removeEventListener('pointerover', over);
            dock.removeEventListener('pointerout', out);
            dock.removeEventListener('focusin', over);
            dock.removeEventListener('focusout', out);
            drop.removeEventListener('transitionend', onTransitionEnd);
            navigation.removeEventListener('click', activate);
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
