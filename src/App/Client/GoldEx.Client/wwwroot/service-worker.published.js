/// <reference no-default-lib="true" />
/// <reference lib="webworker" />

self.importScripts('./service-worker-assets.js');

/*
 * Cache naming
 */
const CACHE_PREFIX = 'blazor-cache-';
const CACHE_NAME = `${CACHE_PREFIX}${self.assetsManifest.version}`;
const API_CACHE = `${CACHE_NAME}-api`;

/*
 * Files safe to pre-cache
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
    /\.ico$/,
    /\.wasm$/,
    /\.dll$/,
    /\.dat$/,
    /\.blat$/
];

const PRECACHE_EXCLUDE = [
    /^service-worker\.js$/
];

const BASE_PATH = '/';

/* ============================
 * INSTALL
 * ============================ */
self.addEventListener('install', event => {
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

        await Promise.allSettled(requests.map(r => cache.add(r)));

        // Cache root
        try {
            await cache.add(new Request(BASE_PATH, { cache: 'reload' }));
        } catch { }
    })());
});

/* ============================
 * ACTIVATE
 * ============================ */
self.addEventListener('activate', event => {
    event.waitUntil((async () => {
        const keys = await caches.keys();

        await Promise.all(
            keys
                .filter(k => k.startsWith(CACHE_PREFIX) && !k.includes(self.assetsManifest.version))
                .map(k => caches.delete(k))
        );

        await self.clients.claim();
    })());
});

/* ============================
 * FETCH
 * ============================ */
self.addEventListener('fetch', event => {
    if (event.request.method !== 'GET') return;

    const url = new URL(event.request.url);

    event.respondWith((async () => {

        /* ============================
         * 0. API REQUESTS
         *    NETWORK FIRST (NEW)
         * ============================ */
        if (url.pathname.startsWith('/api/')) {
            const cache = await caches.open(API_CACHE);

            try {
                const networkResponse = await fetch(event.request);

                // Cache only valid responses
                if (networkResponse && networkResponse.ok) {
                    await cache.put(event.request, networkResponse.clone());
                }

                return networkResponse;
            } catch {
                // OFFLINE FALLBACK
                const cached = await cache.match(event.request);
                if (cached) return cached;

                return new Response(
                    JSON.stringify({ error: 'Offline and no cached data' }),
                    {
                        status: 503,
                        headers: { 'Content-Type': 'application/json' }
                    }
                );
            }
        }

        /* ============================
         * 1. Blazor framework files
         * cache-first (Optimized)
         * ============================ */
        if (url.pathname.startsWith('/_framework/')) {
            // OPTIMIZATION 1: Strip query string manually for an instant O(1) lookup
            // This avoids the massive performance penalty of ignoreSearch: true
            const cleanUrl = new URL(event.request.url);
            cleanUrl.search = '';

            // OPTIMIZATION 2: Use global caches.match to avoid opening the cache 100+ times
            const cached = await caches.match(cleanUrl.toString()) || await caches.match(event.request);
            if (cached) return cached;

            try {
                const response = await fetch(event.request);
                if (response.ok) {
                    // Only open the cache if we actually need to save a missing file
                    const cache = await caches.open(CACHE_NAME);
                    await cache.put(event.request, response.clone());
                }
                return response;
            } catch (error) {
                return new Response('Offline framework file missing', {
                    status: 503,
                    headers: { 'Content-Type': 'text/plain' }
                });
            }
        }

        /* ============================
         * 2. Navigation requests
         *    network-first
         * ============================ */
        if (event.request.mode === 'navigate') {
            try {
                return await fetch(event.request);
            } catch {
                const cache = await caches.open(CACHE_NAME);
                const fallback = await cache.match(BASE_PATH);
                return fallback || new Response('Offline', { status: 503 });
            }
        }

        /* ============================
         * 3. Static assets
         *    cache-first
         * ============================ */
        const cache = await caches.open(CACHE_NAME);
        const cached = await cache.match(event.request);
        if (cached) return cached;

        try {
            const response = await fetch(event.request);
            if (response.ok) {
                await cache.put(event.request, response.clone());
            }
            return response;
        } catch {
            return new Response('Offline', { status: 503 });
        }

    })());
});