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

    /* ============================
     * 0. BYPASS FOR VITRINE & MEDIA
     *    Never intercept Vitrine routes, Vitrine assets, range or video requests.
     *    Returning early without calling event.respondWith allows direct browser network handling.
     * ============================ */
    const isRangeRequest = event.request.headers.has('range');
    const isVideoRequest =
        event.request.destination === 'video' ||
        /\.mp4$|\.webm$|\.ogg$|\.mov$|\.m4v$/i.test(url.pathname);
    const isVitrineAsset =
        url.pathname.includes('/assets/vitrine/') ||
        url.pathname.includes('/assets/fontawesome/') ||
        url.pathname.startsWith('/uploads/products/') ||
        url.pathname.startsWith('/api/vitrine/') ||
        url.pathname.startsWith('/api/v1/vitrine/');

    const isVitrineNavigation = (event.request.mode === 'navigate') && (() => {
        const segments = url.pathname.split('/').filter(Boolean);
        if (segments.length === 0) return false;
        const firstSegment = segments[0].toLowerCase();
        const reserved = [
            'account', 'dashboard', 'invoices', 'products', 'finances', 'base-info',
            'settings', 'quick-invoice', 'user-accounts', 'blogs', 'reporting',
            'customers', 'inventory-stocks', 'api', '_content', '_framework', 'ssr'
        ];
        return !reserved.includes(firstSegment);
    })();

    if (isRangeRequest || isVideoRequest || isVitrineAsset || isVitrineNavigation) {
        return;
    }

    event.respondWith((async () => {

        /* ============================
         * 1. API REQUESTS
         *    NETWORK FIRST
         * ============================ */
        if (url.pathname.startsWith('/api/')) {
            const cache = await caches.open(API_CACHE);

            try {
                const networkResponse = await fetch(event.request);

                if (networkResponse && networkResponse.ok) {
                    await cache.put(event.request, networkResponse.clone());
                }

                return networkResponse;
            } catch {
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
         * 2. Blazor framework files
         *    cache-first
         * ============================ */
        if (url.pathname.startsWith('/_framework/')) {
            const cleanUrl = new URL(event.request.url);
            cleanUrl.search = '';

            const cached =
                await caches.match(cleanUrl.toString()) ||
                await caches.match(event.request);

            if (cached) return cached;

            try {
                const response = await fetch(event.request);
                if (response.ok) {
                    const cache = await caches.open(CACHE_NAME);
                    await cache.put(event.request, response.clone());
                }
                return response;
            } catch {
                return new Response('Offline framework file missing', {
                    status: 503,
                    headers: { 'Content-Type': 'text/plain' }
                });
            }
        }

        /* ============================
         * 3. Navigation requests
         *    network-first
         * ============================ */
        if (event.request.mode === 'navigate') {
            try {
                return await fetch(event.request);
            } catch {
                const cache = await caches.open(CACHE_NAME);
                const fallback = await cache.match(BASE_PATH);
                if (fallback) return fallback;

                return new Response(
                    '<!DOCTYPE html><html lang="fa" dir="rtl"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>خطای ارتباط با سرور</title><style>body{font-family:system-ui,-apple-system,sans-serif;display:flex;align-items:center;justify-content:center;height:100vh;margin:0;background:#18181b;color:#f4f4f5;text-align:center;padding:20px}h1{font-size:1.4rem;color:#daa520;margin-bottom:8px}p{color:#a1a1aa;margin-bottom:20px;font-size:0.95rem}button{background:#daa520;border:none;color:#000;font-weight:bold;padding:10px 24px;border-radius:8px;cursor:pointer;font-size:14px}</style></head><body><div><h1>عدم برقراری ارتباط با سرور</h1><p>لطفاً اتصال اینترنت خود را بررسی نمایید.</p><button onclick="location.reload()">تلاش مجدد</button></div></body></html>',
                    {
                        status: 503,
                        headers: { 'Content-Type': 'text/html; charset=utf-8' }
                    }
                );
            }
        }

        /* ============================
         * 4. Static assets
         *    cache-first
         * ============================ */
        const cache = await caches.open(CACHE_NAME);
        const cached = await cache.match(event.request);
        if (cached) return cached;

        try {
            const response = await fetch(event.request);

            if (response.ok && response.status === 200) {
                await cache.put(event.request, response.clone());
            }

            return response;
        } catch {
            return new Response('', { status: 503, statusText: 'Offline' });
        }

    })());
});

/* ============================
 * MESSAGE (skipWaiting)
 * ============================ */
self.addEventListener('message', event => {
    if (event.data && event.data.action === 'skipWaiting') {
        self.skipWaiting();
    }
});

