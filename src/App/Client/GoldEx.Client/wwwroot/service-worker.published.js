//self.addEventListener('fetch', () => { });

self.importScripts('./service-worker-assets.js');

// Immediately activate and claim control of pages
self.addEventListener('install', event => {
    console.log('Service Worker installing...');
    self.skipWaiting();

    // Notify clients before waiting for the install process
    event.waitUntil((async () => {
        const clientsList = await self.clients.matchAll({ type: 'window' });
        for (const client of clientsList) {
            client.postMessage({ type: 'INSTALLING_NEW_VERSION' });
        }

        // Then run the actual install logic
        await onInstall(event);
    })());
});

self.addEventListener('activate', event => {
    console.log('Service Worker activating...');
    event.waitUntil(onActivate(event));
    self.clients.claim();

    // Notify all clients about the new version
    event.waitUntil((async () => {
        const allClients = await self.clients.matchAll({ type: 'window' });
        for (const client of allClients) {
            client.postMessage({ type: 'NEW_VERSION_AVAILABLE' });
        }
    })());
});

self.addEventListener('fetch', event => {
    event.respondWith(onFetch(event));
});

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [/\.dll$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/];
const offlineAssetsExclude = [/^service-worker\.js$/, /^GoldEx\.Client\.styles\.css$/i];

// Replace with your base path if you are hosting on a subfolder. Ensure there is a trailing '/'.
const base = "/";
const baseUrl = new URL(base, self.origin);
const manifestUrlList = self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href);

async function onInstall(event) {
    console.info('Service Worker: Installing');

    // Delete old caches first to avoid conflicts
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix))
        .map(key => {
            console.log(`Deleting old cache: ${key}`);
            return caches.delete(key);
        }));

    // Fetch and cache all matching items from the assets manifest
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => {
            // Use no-cache and reload to bypass any intermediate caches
            return new Request(asset.url, {
                cache: 'reload',
                credentials: 'omit'
            });
        });

    const cache = await caches.open(cacheName);

    // Cache assets individually to catch errors better
    for (const request of assetsRequests) {
        try {
            await cache.add(request);
        } catch (error) {
            console.error(`Failed to cache ${request.url}:`, error);
            // Continue with other assets instead of failing completely
        }
    }

    // Manually cache home page (root path)
    try {
        await cache.add(new Request(base, { cache: 'reload' }));
    } catch (err) {
        console.warn('Service worker: Failed to cache root path:', err);
    }
}

async function onActivate(event) {
    console.info('Service Worker: Activating');
    // Delete old caches
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => {
            console.log(`Deleting old cache during activation: ${key}`);
            return caches.delete(key);
        }));
}

async function onFetch(event) {
    if (event.request.method !== 'GET') {
        return fetch(event.request);
    }

    try {
        // Try to fetch from network first
        const response = await fetch(event.request);

        // Only cache successful responses
        if (response.ok) {
            const cache = await caches.open(cacheName);
            cache.put(event.request, response.clone());
        }

        return response;
    } catch (error) {
        // Fallback to cache
        const cache = await caches.open(cacheName);
        const cachedResponse = await cache.match(event.request);

        if (cachedResponse) {
            // Notify all clients about offline state
            event.waitUntil((async () => {
                const allClients = await self.clients.matchAll({ type: 'window' });
                for (const client of allClients) {
                    client.postMessage({ type: 'NETWORK_UNAVAILABLE' });
                }
            })());

            return cachedResponse;
        }

        // Return 404 if not cached
        return new Response('Not found', {
            status: 404,
            statusText: 'Resource Not Found',
            headers: {
                'Content-Type': 'text/plain'
            }
        });
    }
}