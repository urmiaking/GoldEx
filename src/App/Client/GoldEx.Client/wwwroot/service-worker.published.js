self.importScripts('./service-worker-assets.js');

// Immediately activate and claim control of pages
self.addEventListener('install', event => {
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
const offlineAssetsInclude = [/\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/];
const offlineAssetsExclude = [/^service-worker\.js$/, /^GoldEx\.Client\.styles\.css$/i];

// Replace with your base path if you are hosting on a subfolder. Ensure there is a trailing '/'.
const base = "/";
const baseUrl = new URL(base, self.origin);
const manifestUrlList = self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href);

async function onInstall(event) {
    // Fetch and cache all matching items from the assets manifest
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));

    const cache = await caches.open(cacheName);
    await cache.addAll(assetsRequests);

    // Manually cache home page (root path)
    try {
        await cache.add(new Request(base, { cache: 'no-cache' }));
    } catch (err) {
        console.warn('Service worker: Failed to cache root path:', err);
    }
}

async function onActivate(event) {
    // Delete old caches
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));
}

async function onFetch(event) {
    if (event.request.method !== 'GET') {
        return fetch(event.request);
    }

    try {
        // Try to fetch from network first
        const response = await fetch(event.request);

        // Cache the response if successful
        const cache = await caches.open(cacheName);
        cache.put(event.request, response.clone());

        return response;
    } catch (error) {
        // Fallback to cache
        const cache = await caches.open(cacheName);
        const cachedResponse = await cache.match(event.request);

        if (cachedResponse) {
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