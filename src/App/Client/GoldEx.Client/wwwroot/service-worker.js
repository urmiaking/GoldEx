// In development, always fetch from network and never cache static assets or HTML
self.addEventListener('install', event => {
    self.skipWaiting();
});

self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(keys => {
            return Promise.all(
                keys.map(key => {
                    console.log('[Dev SW] Clearing cache:', key);
                    return caches.delete(key);
                })
            );
        }).then(() => self.clients.claim())
    );
});

self.addEventListener('fetch', event => {
    // Pass through all requests directly to the network in development
    event.respondWith(
        fetch(event.request).catch(err => {
            return new Response('Offline (Dev Mode)', {
                status: 503,
                statusText: 'Offline',
                headers: { 'Content-Type': 'text/plain' }
            });
        })
    );
});

self.addEventListener('message', event => {
    if (event.data && event.data.action === 'skipWaiting') {
        self.skipWaiting();
    }
});

