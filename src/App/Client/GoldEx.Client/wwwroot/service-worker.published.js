/// <reference no-default-lib="true" />
/// <reference lib="webworker" />

self.importScripts('./service-worker-assets.js');

/*
 * Cache naming
 */
const CACHE_PREFIX = 'blazor-cache-';
const CACHE_NAME = `${CACHE_PREFIX}${self.assetsManifest.version}`;

/*
 * Files safe to pre-cache
 * (DO NOT pre-cache _framework/*.wasm)
 */
const PRECACHE_INCLUDE = [
    /\.html$/,
    /\.js$/,
    /\.css$/,
    /\.json$/,
    /\.woff2?$/,
    /\.png$/,
    /\.jpe?g$/,
    /\.gif$/,
    /\.ico$/
];

const PRECACHE_EXCLUDE = [
    /^service-worker\.js$/,
    /^_framework\/.*\.wasm$/,
    /^_framework\/.*\.dll$/,
    /^_framework\/.*\.dat$/,
    /^_framework\/.*\.blat$/
];

/*
 * Base path (adjust if hosted in subfolder)
 */
const BASE_PATH = '/';

/* ============================
 * INSTALL
 * ============================ */
self.addEventListener('install', event => {
    console.info('[SW] Installing');

    self.skipWaiting();

    event.waitUntil((async () => {
        const cache = await caches.open(CACHE_NAME);

        const requests = self.assetsManifest.assets
            .filter(asset =>
                PRECACHE_INCLUDE.some(r => r.test(asset.url)) &&
                !PRECACHE_EXCLUDE.some(r => r.test(asset.url))
            )
            .map(asset =>
                new Request(asset.url, {
                    cache: 'reload',
                    credentials: 'omit'
                })
            );

        for (const request of requests) {
            try {
                await cache.add(request);
            } catch (err) {
                console.warn('[SW] Failed to pre-cache:', request.url);
            }
        }

        // Cache root document
        try {
            await cache.add(new Request(BASE_PATH, { cache: 'reload' }));
        } catch { }

        // Notify open tabs
        const clients = await self.clients.matchAll({ type: 'window' });
        for (const client of clients) {
            client.postMessage({ type: 'INSTALLING_NEW_VERSION' });
        }
    })());
});

/* ============================
 * ACTIVATE
 * ============================ */
self.addEventListener('activate', event => {
    console.info('[SW] Activating');

    event.waitUntil((async () => {
        // Remove old caches
        const keys = await caches.keys();
        await Promise.all(
            keys
                .filter(k => k.startsWith(CACHE_PREFIX) && k !== CACHE_NAME)
                .map(k => caches.delete(k))
        );

        await self.clients.claim();

        // Notify clients about new version
        const clients = await self.clients.matchAll({ type: 'window' });
        for (const client of clients) {
            client.postMessage({ type: 'NEW_VERSION_AVAILABLE' });
        }
    })());
});

/* ============================
 * FETCH
 * ============================ */
self.addEventListener('fetch', event => {
    if (event.request.method !== 'GET') {
        return;
    }

    const url = new URL(event.request.url);

    event.respondWith((async () => {

        /*
         * 1. Cache-first for Blazor framework files
         *    (prevents multiple WASM downloads)
         */
        if (url.pathname.startsWith('/_framework/')) {
            const cache = await caches.open(CACHE_NAME);
            const cached = await cache.match(event.request);

            if (cached) {
                return cached;
            }

            const response = await fetch(event.request);
            if (response.ok) {
                try {
                    await cache.put(event.request, response.clone());
                } catch { }
            }

            return response;
        }

        /*
         * 2. Navigation requests: network-first with offline fallback
         */
        if (event.request.mode === 'navigate') {
            try {
                return await fetch(event.request);
            } catch {
                const cache = await caches.open(CACHE_NAME);
                return await cache.match(BASE_PATH);
            }
        }

        /*
         * 3. Static assets: cache-first
         */
        const cache = await caches.open(CACHE_NAME);
        const cached = await cache.match(event.request);

        if (cached) {
            return cached;
        }

        try {
            const response = await fetch(event.request);
            if (response.ok) {
                try {
                    await cache.put(event.request, response.clone());
                } catch { }
            }
            return response;
        } catch {
            // Offline notification
            const clients = await self.clients.matchAll({ type: 'window' });
            for (const client of clients) {
                client.postMessage({ type: 'NETWORK_UNAVAILABLE' });
            }

            return new Response('Offline', {
                status: 503,
                headers: { 'Content-Type': 'text/plain' }
            });
        }

    })());
});
