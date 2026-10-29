// Single 272px-wide glass silhouette. The right 64px remain the rail;
// the left edge smoothly deforms. Same commands enable clip-path interpolation.
function dockSilhouette(height, top, extent, bottomRow = false) {
    const H = Math.max(200, Math.round(height));
    const B = 208, W = 272, R = 31;
    const reach = Math.max(0, Math.min(196, extent));
    const k = reach / 196, left = B - reach;
    const account = bottomRow && reach > 0;
    // The account must not clamp 106px above the Dock bottom: it would cover More.
    const row = Math.max(34, Math.min(H - (account ? 52 : 58), Math.round(top)));
    const upper = row + 1, lower = row + 49;
    const shoulder = 13 * k, radius = Math.min(23, reach);
    const baseLeft = account ? left : B;
    const bottomJoin = account ? H - R : lower + shoulder;
    const d = [
        'M ' + (B+R) + ' 0',
        'H ' + (W-R) + ' Q ' + W + ' 0 ' + W + ' ' + R,
        'V ' + (H-R) + ' Q ' + W + ' ' + H + ' ' + (W-R) + ' ' + H,
        'H ' + (baseLeft+R) + ' Q ' + baseLeft + ' ' + H + ' ' + baseLeft + ' ' + (H-R),
        'V ' + bottomJoin,
        'C ' + baseLeft + ' ' + bottomJoin + ' ' + (account ? left : B-2*shoulder) + ' ' + (account ? bottomJoin : lower) + ' ' + (account ? left : B-2*shoulder) + ' ' + (account ? bottomJoin : lower),
        'L ' + (account ? left : left+radius) + ' ' + (account ? H-R : lower),
        'Q ' + left + ' ' + (account ? H-R : lower) + ' ' + left + ' ' + (account ? H-R : lower-radius),
        'L ' + left + ' ' + (upper+radius),
        'Q ' + left + ' ' + upper + ' ' + (left+radius) + ' ' + upper,
        'L ' + (B-2*shoulder) + ' ' + upper,
        'C ' + (B-shoulder) + ' ' + upper + ' ' + B + ' ' + (upper-shoulder) + ' ' + B + ' ' + (upper-2*shoulder),
        'V ' + R + ' Q ' + B + ' 0 ' + (B+R) + ' 0 Z'
    ].join(' ');
    return 'path("' + d + '")';
}

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
    const glass = dock.querySelector('.gex-dock-glass');
    let lastHullTop = 120;
    const hoverSelector = '.gex-navigation-menu > a, .gex-navigation-menu > button.gex-nav-release, .gex-sidebar-create, .gex-store-picker-trigger, .gex-identity-link, .gex-signout';
    let disposed = false, scheduled = 0, open = false;
    let selected = null, pendingSelection = null;

    const allLinks = () => [...navigation.querySelectorAll(':scope > a[href], :scope > button.gex-nav-release')];
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
    const measureLabel = element => {
        const target = element?.querySelector?.('.gex-signout-label')
            || element?.querySelector?.('.gex-store-picker-copy strong')
            || element?.querySelector?.('span');
        if (!target) return 65;
        // Range measures actual glyphs, ignoring the old 123px minimum label box.
        const range = document.createRange?.();
        if (range) {
            range.selectNodeContents(target);
            const width = range.getBoundingClientRect().width;
            range.detach?.();
            if (Number.isFinite(width) && width > 0) return width;
        }
        return Math.max(30, (target.textContent?.trim().length || 8) * 7.5);
    };
    const expandedWidth = (element, kind) => {
        const glyphs = measureLabel(element);
        const external = Boolean(element?.getAttribute?.('target') === '_blank');
        const extra = kind === 'account' ? 69 : kind === 'store' ? 81 : 75;
        return Math.round(Math.max(kind === 'account' ? 142 : 108,
            Math.min(kind === 'store' ? 252 : 245, glyphs + extra + (external ? 20 : 0))));
    };
    const sizeActions = () => {
        for (const anchor of allLinks()) {
            anchor.style?.setProperty?.('--gex-item-expanded-width', expandedWidth(anchor, 'nav') + 'px');
        }
        const create = dock.querySelector('.gex-sidebar-create');
        if (create) create.style.setProperty('--gex-item-expanded-width', expandedWidth(create, 'create') + 'px');
        const store = dock.querySelector('.gex-store-picker-trigger');
        if (store) store.style.setProperty('--gex-item-expanded-width', expandedWidth(store, 'store') + 'px');
        const signout = dock.querySelector('.gex-signout');
        if (signout) signout.style.setProperty('--gex-item-expanded-width', expandedWidth(signout, 'account') + 'px');
    };
    const renderHull = (element = null) => {
        const height = dock.getBoundingClientRect().height || dock.offsetHeight || 760;
        const identity = element?.closest?.('.gex-sidebar-identity');
        const isAccount = Boolean(identity);
        const isStore = Boolean(element?.matches?.('.gex-store-picker-trigger'));
        const isCreate = Boolean(element?.matches?.('.gex-sidebar-create'));
        // A filled Primary action does not expand the translucent glass underneath.
        const isActiveRoute = Boolean(element?.classList?.contains('active')
            || element?.getAttribute?.('aria-current') === 'page');
        // Like New Invoice, the selected route is a filled Primary capsule:
        // the glass surface itself stays narrow instead of protruding behind it.
        const shapeTarget = isCreate || isActiveRoute ? null : element;
        const width = shapeTarget ? expandedWidth(
            isAccount ? (identity?.querySelector?.('.gex-signout') || element) : element,
            isAccount ? 'account' : isStore ? 'store' : 'nav'
        ) : 0;
        const measuredRow = identity?.getBoundingClientRect
            ? positionTop(identity) : element ? positionTop(element) : lastHullTop;
        const rowTop = Math.round(shapeTarget ? measuredRow : lastHullTop);
        if (shapeTarget) lastHullTop = rowTop;
        // Add room for the account pill's curved left cap: its label and hit
        // target must remain INSIDE the shared glass rather than hanging out.
        const reach = shapeTarget && media.matches ? Math.min(196, width - 48 + (isAccount ? 44 : 0)) : 0;
        glass.style.clipPath = dockSilhouette(height, rowTop, reach, isAccount);
        dock.classList.toggle('gex-dock-is-account', Boolean(shapeTarget && isAccount));
    };
    const setLobe = element => {
        if (!media.matches || !element) {
            dock.classList.remove('gex-dock-has-hover');
            renderHull();
            return;
        }
        renderHull(element);
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
        if (!dock.classList.contains('gex-dock-has-hover')) renderHull();
        sizeActions();
        const links = allLinks();
        // Release Notes is a secondary action: always belongs to More, never
        // consumes a Dock slot or appears twice as a visible icon and proxy.
        const forceOverflow = link => Boolean(link.matches?.('button.gex-nav-release'));
        const routeLinks = links.filter(link => !forceOverflow(link));
        const storePicker = viewport.querySelector('.gex-store-picker');
        const storeHeight = storePicker ? Math.ceil(storePicker.getBoundingClientRect().height + 9) : 0;
        const available = Math.max(0, viewport.clientHeight - storeHeight);
        const maxVisible = Math.max(0, Math.floor((available - 4) / 52));
        const keep = Math.min(routeLinks.length, maxVisible);
        const extras = [...routeLinks.slice(keep), ...links.filter(forceOverflow)];

        for (const link of links) {
            link.setAttribute('data-dock-overflow', String(extras.includes(link)));
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
                // Only actual navigation links move the route selection drop.
                // Command buttons (Release Notes) must not change the active page.
                if (original.matches?.('a[href]') && original.getAttribute('target') !== '_blank') {
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
    // Web fonts can finish after the first Blazor render; measure their real glyph widths.
    document.fonts?.ready?.then(queue);

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
