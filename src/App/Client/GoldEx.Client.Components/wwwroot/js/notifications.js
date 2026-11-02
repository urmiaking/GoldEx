// Mkx-style native modal; browser modality owns focus containment and background inertness.
export function bind(panel) {
    let previousFocus;
    const release = () => {
        if (panel.open) return;
        document.documentElement.classList.remove('gex-notifications-open');
        if (previousFocus?.isConnected) previousFocus.focus({ preventScroll: true });
        previousFocus = null;
    };
    const close = () => { if (panel.open) panel.close(); release(); };
    const open = () => {
        if (panel.open) return;
        previousFocus = document.activeElement;
        for (const popover of document.querySelectorAll('.gex-palette-sheet:popover-open')) popover.hidePopover();
        panel.showModal();
        document.documentElement.classList.add('gex-notifications-open');
    };
    const click = event => {
        if (event.target.closest('[data-notification-close]')) { close(); return; }
        if (event.target !== panel) return;
        const bounds = panel.getBoundingClientRect();
        if (event.clientX < bounds.left || event.clientX > bounds.right || event.clientY < bounds.top || event.clientY > bounds.bottom) close();
    };
    panel.addEventListener('click', click);
    panel.addEventListener('close', release);
    return { open, close, dispose() { close(); panel.removeEventListener('click', click); panel.removeEventListener('close', release); } };
}

const optOutKey = 'GoldEx.PushDisabled';
const bounded = promise => new Promise((resolve, reject) => {
    const timer = setTimeout(() => reject(new Error('Push service timed out.')), 20000);
    Promise.resolve(promise).then(value => { clearTimeout(timer); resolve(value); }, error => { clearTimeout(timer); reject(error); });
});
const supported = () => window.isSecureContext && 'serviceWorker' in navigator && 'PushManager' in window && 'Notification' in window;
const registration = async () => {
    const result = await navigator.serviceWorker.register('/service-worker.js', { updateViaCache: 'none' });
    // Ready resolves only after activation, including the first installation.
    await bounded(navigator.serviceWorker.ready);
    return result;
};
export async function getState() {
    if (!supported()) return { supported: false, permission: 'unsupported', subscribed: false, endpoint: null };
    const current = await navigator.serviceWorker.getRegistration('/');
    const subscription = await current?.pushManager.getSubscription();
    return { supported: true, permission: Notification.permission, subscribed: !!subscription && Notification.permission === 'granted', endpoint: subscription?.endpoint ?? null };
}
export async function subscribe(publicKey, prompt) {
    const state = await getState();
    if (!state.supported || (!prompt && localStorage.getItem(optOutKey) === 'true')) return { ...state, endpoint: null };
    // Request permission before awaiting network/worker operations, preserving the user's click gesture.
    const permission = prompt ? await bounded(Notification.requestPermission()) : Notification.permission;
    if (permission !== 'granted') return { ...state, permission, endpoint: null };
    const keyBytes = Uint8Array.from(atob(publicKey.replace(/-/g, '+').replace(/_/g, '/').padEnd(Math.ceil(publicKey.length / 4) * 4, '=')), c => c.charCodeAt(0));
    const worker = await registration();
    let subscription = await worker.pushManager.getSubscription();
    const oldKey = subscription?.options.applicationServerKey;
    if (subscription && oldKey && (oldKey.byteLength !== keyBytes.length || new Uint8Array(oldKey).some((byte, index) => byte !== keyBytes[index]))) {
        await subscription.unsubscribe(); subscription = null;
    }
    subscription ??= await bounded(worker.pushManager.subscribe({ userVisibleOnly: true, applicationServerKey: keyBytes }));
    if (prompt) localStorage.removeItem(optOutKey);
    const json = subscription.toJSON();
    return { supported: true, permission, endpoint: json.endpoint, p256dh: json.keys.p256dh, auth: json.keys.auth };
}
export async function disable() {
    localStorage.setItem(optOutKey, 'true');
    const worker = await navigator.serviceWorker.getRegistration('/');
    const subscription = await worker?.pushManager.getSubscription();
    if (subscription) await subscription.unsubscribe();
    for (const notification of await worker?.getNotifications() ?? []) notification.close();
}
