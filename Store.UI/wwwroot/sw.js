/// <reference lib="webworker" />
/**
 * UX-01 — ClexAn POS service worker.
 *
 * Provides:
 *   - Offline shell: caches static assets so the POS launches without network.
 *   - Stale-while-revalidate for the JS/CSS bundles.
 *   - Network-first for API calls (POS MUST hit fresh data).
 *
 * Update strategy: bumped CACHE_VERSION triggers the install event to
 * replace the cache. The activate event deletes any old cache entries.
 */
const CACHE_VERSION = 'clexan-v2';
const STATIC_CACHE = `${CACHE_VERSION}-static`;
const RUNTIME_CACHE = `${CACHE_VERSION}-runtime`;

// Critical offline shell. Add to this list when you ship a new page that
// MUST work offline (POS landing is the obvious one). Everything else uses
// stale-while-revalidate.
const OFFLINE_SHELL = [
    '/Pos',
    '/Login',
    '/',
    '/manifest.json',
    '/images/icon-192.svg',
    '/images/icon-512.svg',
    '/css/site.css',
    '/css/components.css',
    '/css/tokens.css',
    '/css/operations.css',
    '/css/modules/sales.css',
    '/css/pwa.css',
    '/js/site.js',
    '/js/toast-bus.js',
    '/js/command-palette.js',
    '/js/pwa-install.js',
    '/js/pos-offline.js'
];

self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(STATIC_CACHE).then((cache) => {
            // Use { cache: 'reload' } so the install step always grabs fresh
            // copies of the offline shell from the network, not the HTTP cache.
            return Promise.allSettled(
                OFFLINE_SHELL.map((url) => cache.add(new Request(url, { cache: 'reload' })))
            );
        }).then(() => self.skipWaiting())
    );
});

self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys().then((keys) => {
            return Promise.all(
                keys.filter((k) => !k.startsWith(CACHE_VERSION))
                    .map((k) => caches.delete(k))
            );
        }).then(() => self.clients.claim())
    );
});

self.addEventListener('fetch', (event) => {
    const req = event.request;
    if (req.method !== 'GET') return;

    const url = new URL(req.url);
    if (url.origin !== self.location.origin) return;

    // Network-first for API calls. POS MUST hit fresh data.
    if (url.pathname.startsWith('/api/')) {
        event.respondWith(
            fetch(req)
                .then((res) => {
                    // Cache successful GETs for offline read-only paths.
                    if (res.ok && res.status === 200) {
                        const copy = res.clone();
                        caches.open(RUNTIME_CACHE).then((cache) => cache.put(req, copy));
                    }
                    return res;
                })
                .catch(() => caches.match(req))
        );
        return;
    }

    // Stale-while-revalidate for static assets.
    if (req.destination === 'script' || req.destination === 'style' ||
        req.destination === 'image' || req.destination === 'font') {
        event.respondWith(
            caches.match(req).then((cached) => {
                const fetched = fetch(req).then((res) => {
                    if (res.ok) {
                        const copy = res.clone();
                        caches.open(RUNTIME_CACHE).then((cache) => cache.put(req, copy));
                    }
                    return res;
                }).catch(() => cached);
                return cached || fetched;
            })
        );
        return;
    }

    // Network-first for HTML pages (so back/forward don't serve stale views).
    if (req.mode === 'navigate' || (req.headers.get('accept') || '').includes('text/html')) {
        event.respondWith(
            fetch(req).then((res) => {
                if (res.ok) {
                    const copy = res.clone();
                    caches.open(RUNTIME_CACHE).then((cache) => cache.put(req, copy));
                }
                return res;
            }).catch(() => caches.match(req).then((cached) => cached || caches.match('/Pos')))
        );
        return;
    }
});

// Allow the page to force a skipWaiting from the registration.
self.addEventListener('message', (event) => {
    if (event.data && event.data.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }
});