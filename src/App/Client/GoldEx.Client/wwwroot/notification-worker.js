// Shared by development and published service workers, adapted from MeetFlow.
const safeNotificationUrl = value => {
    try {
        const target = new URL(typeof value === 'string' ? value : '/', self.location.origin);
        return target.origin === self.location.origin ? target.pathname + target.search + target.hash : '/';
    } catch { return '/'; }
};
self.addEventListener('push', event => {
    let payload = {};
    try { payload = event.data?.json() ?? {}; } catch { }
    event.waitUntil(self.registration.showNotification(
        typeof payload.title === 'string' && payload.title.trim() ? payload.title : 'GoldEx', {
            body: typeof payload.body === 'string' ? payload.body : 'اعلان جدیدی دارید.',
            icon: '/apple-touch-icon-180x180.png',
            tag: typeof payload.tag === 'string' ? payload.tag : 'goldex-notification',
            dir: 'rtl', lang: 'fa',
            data: { url: safeNotificationUrl(payload.url) }
        }));
});
self.addEventListener('notificationclick', event => {
    event.notification.close();
    const target = new URL(safeNotificationUrl(event.notification.data?.url), self.location.origin).href;
    event.waitUntil((async () => {
        const windows = await self.clients.matchAll({ type: 'window', includeUncontrolled: true });
        for (const client of windows) {
            if (new URL(client.url).origin !== self.location.origin) continue;
            if ('navigate' in client) await client.navigate(target);
            if ('focus' in client) return client.focus();
        }
        return self.clients.openWindow(target);
    })());
});
