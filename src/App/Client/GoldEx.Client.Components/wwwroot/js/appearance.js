let activeTransition;

export function sync(dark, palette) {
    const root = document.documentElement;
    root.dataset.gexMode = dark ? 'dark' : 'light';
    root.dataset.gexPalette = palette;
    root.style.colorScheme = dark ? 'dark' : 'light';
    const background = getComputedStyle(root).getPropertyValue('--mud-palette-background').trim();
    if (background) document.querySelector('meta[name="theme-color"]')?.setAttribute('content', background);
}

export async function change(receiver, dark, palette, source) {
    const root = document.documentElement;
    const reverse = root.dataset.gexMode === 'dark' && !dark;
    let applied = false;
    const apply = async () => {
        if (applied) return;
        await receiver.invokeMethodAsync('ApplyAppearanceAsync', dark, palette);
        applied = true;
        sync(dark, palette);
    };
    activeTransition?.skipTransition();
    if (!source?.isConnected || !document.startViewTransition || document.visibilityState === 'hidden' ||
        matchMedia('(prefers-reduced-motion: reduce)').matches) {
        await apply();
        return;
    }

    const bounds = source.getBoundingClientRect();
    const x = bounds.left + bounds.width / 2;
    const y = bounds.top + bounds.height / 2;
    const radius = Math.ceil(Math.hypot(Math.max(x, innerWidth - x), Math.max(y, innerHeight - y)));
    root.dataset.gexAppearanceTransition = reverse ? 'contract' : 'expand';
    let transition;
    let reveal;
    try {
        transition = document.startViewTransition(apply);
        activeTransition = transition;
        await transition.ready;
        const clips = [`circle(0px at ${x}px ${y}px)`, `circle(${radius}px at ${x}px ${y}px)`];
        reveal = root.animate({ clipPath: reverse ? clips.reverse() : clips }, {
            duration: 800,
            easing: reverse ? 'cubic-bezier(.64,0,.78,0)' : 'cubic-bezier(.22,1,.36,1)',
            pseudoElement: reverse ? '::view-transition-old(root)' : '::view-transition-new(root)',
            fill: 'both'
        });
        await reveal.finished;
    } catch {
        transition?.skipTransition();
        if (transition) await transition.updateCallbackDone.catch(() => {});
        await apply();
    } finally {
        // Retain the final clip until snapshots disappear, preventing an old-theme flash.
        if (transition) await transition.finished.catch(() => {});
        reveal?.cancel();
        if (!transition || activeTransition === transition) {
            activeTransition = null;
            delete root.dataset.gexAppearanceTransition;
        }
    }
}
