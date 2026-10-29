// Single 272px-wide glass silhouette. The right 64px remain the rail;
// the left edge smoothly deforms. Same commands enable clip-path interpolation.
function dockSilhouette(height, top, extent, bottomRow = false) {
    const H = Math.max(200, Math.round(height));
    const B = 208, W = 272, R = 31;
    const reach = Math.max(0, Math.min(196, extent));
    const k = reach / 196, left = B - reach;
    const nearEnd = bottomRow && reach > 0;
    const yTop = Math.max(35, Math.min(H - 106, Math.round(top)));
    const yBottom = yTop + 54;
    const joinEnd = nearEnd ? H - R : yBottom + 24 * k;
    const lowLip = nearEnd ? H - R : yBottom + 6 * k;
    const lowerShoulder = nearEnd ? left : B - 58 * k;
    const baseAtBottom = nearEnd ? left : B;
    const round = 18 * k;
    const lowerRound = nearEnd ? 0 : round;
    const bottomControl = nearEnd ? joinEnd : joinEnd - 12 * k;
    const shoulderControl = nearEnd ? left : lowerShoulder + 16 * k;
    const d = [
        'M ' + (B + R) + ' 0',
        'H ' + (W - R) + ' Q ' + W + ' 0 ' + W + ' ' + R,
        'V ' + (H - R) + ' Q ' + W + ' ' + H + ' ' + (W - R) + ' ' + H,
        'H ' + (baseAtBottom + R) + ' Q ' + baseAtBottom + ' ' + H + ' ' + baseAtBottom + ' ' + (H - R),
        'V ' + joinEnd,
        'C ' + baseAtBottom + ' ' + bottomControl + ' ' + shoulderControl + ' ' + lowLip + ' ' + lowerShoulder + ' ' + lowLip,
        'L ' + (left + lowerRound) + ' ' + lowLip,
        'Q ' + left + ' ' + lowLip + ' ' + left + ' ' + (lowLip - 17*k),
        'L ' + left + ' ' + (yTop + 13*k),
        'Q ' + left + ' ' + (yTop - 7*k) + ' ' + (left + round) + ' ' + (yTop - 7*k),
        'L ' + (B - 58*k) + ' ' + (yTop - 7*k),
        'C ' + (B - 18*k) + ' ' + (yTop - 7*k) + ' ' + B + ' ' + (yTop - 13*k) + ' ' + B + ' ' + (yTop - 32*k),
        'V ' + R + ' Q ' + B + ' 0 ' + (B + R) + ' 0 Z'
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
    const renderHull = (element = null) => {
        const height = dock.getBoundingClientRect().height || dock.offsetHeight || 760;
        const isAccount = Boolean(element?.closest?.('.gex-sidebar-identity'));
        const isStore = Boolean(element?.matches?.('.gex-store-picker-trigger'));
        const isCreate = Boolean(element?.matches?.('.gex-sidebar-create'));
        const width = isStore ? 260 : isAccount ? 238 : isCreate ? 207 : 220;
        const rowTop = element ? positionTop(element) : lastHullTop;
        if (element) lastHullTop = rowTop;
        dock.style.setProperty('--gex-lobe-top', rowTop + 'px');
        dock.style.setProperty('--gex-lobe-width', width + 'px');
        const reach = element && media.matches ? Math.min(196, width - 48) : 0;
        glass.style.clipPath = dockSilhouette(height, rowTop, reach, isAccount);
        dock.classList.toggle('gex-dock-is-account', Boolean(element && isAccount));
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
