// Invoice detail panels exist in desktop and mobile markup trees; fetch only
// for the instance that actually enters the visible viewport.
export function observeVisible(element, dotNetRef) {
    let disposed = false;
    const observer = new IntersectionObserver(entries => {
        if (disposed || !entries.some(entry => entry.isIntersecting)) return;
        observer.disconnect();
        void dotNetRef.invokeMethodAsync('ActivateAsync').catch(() => {});
    }, { threshold: 0.01 });
    observer.observe(element);
    return { dispose() { disposed = true; observer.disconnect(); } };
}
