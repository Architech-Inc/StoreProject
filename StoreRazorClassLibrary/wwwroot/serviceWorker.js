importScripts('./vendors/workbox/workbox-v6.5.4/workbox-sw.js');

workbox.setConfig({
    modulePathPrefix: './vendors/workbox/workbox-v6.5.4/',
});

const strategy = new workbox.strategies.NetworkFirst();

const cacheName = 'Clarkstore';
const urls = [
    './index.html',
    './styles/index.css',
    './scripts/index.js',
    './manifest.json',
    './images/logo.png',
    './serviceWorkers.js',
    './favicon.ico',
    './apple-touch-icon.png',
    './vendors/jquery-3.6.0.min.js',
    './vendors/tailwind/font.awesome.css',
    './vendors/tailwind/tailwind.elements.min.css',
    './vendors/tailwind/tailwind.elements.min.js',
    './vendors/tailwind/tailwindcss.css',
    './vendors/tailwind/tailwindcss.js',
    './vendors/workbox/workbox-v6.5.4/workbox-sw.js',
    './vendors/workbox/workbox-v6.5.4/workbox-strategies.prod.js',
    './vendors/workbox/workbox-v6.5.4/workbox-core.prod.js',
    './vendors/workbox/workbox-v6.5.4/workbox-recipes.prod.js',
    './vendors/workbox/workbox-v6.5.4/workbox-routing.prod.js',
    './vendors/workbox/workbox-v6.5.4/workbox-cacheable-response.prod.js',
    './vendors/workbox/workbox-v6.5.4/workbox-expiration.prod.js',
    './vendors/workbox/workbox-v6.5.4/precaching.prod.js'
];

workbox.recipes.warmStrategyCache({ urls, strategy})

workbox.routing.registerRoute(
    ({request}) => request.destination === 'image',
    strategy
)

self.addEventListener('fetch', event => {
    const { request } = event;
    const url = new URL(request.url);

    //if (url.origin === location.origin && url.pathname === '/') {
    event.respondWith(new workbox.strategies.StaleWhileRevalidate().handle({ event, request }));
    //}
});

//Demand to install
/* self.addEventListener('install', installEvent => {
    installEvent.waitUntil(
        caches.open(cacheName).then(cache => {
            return cache.addAll(staticAssets);
        })
    )
});

self.addEventListener('fetch', e => {
    const req = e.request;
    const url = new URL(req.url);

    if(url.origin === loacation.origin) {
        e.respondWith(cacheFirst(req));
    } else {
        e.respondWith(networkAndCache(req));
    }
});

// Serve cached content when offline 
self.addEventListener('active', e => {
    self.clients.claim();
});

async function cacheFirst(req) {
    const cache = await caches.open(cacheName);
    const cached = await cache.match(req);
    return cached || fetch(req);
}

async function networkAndCache(req) {
    const cache = await caches.open(cacheName);
    try {
        const fresh = await fetch(req);
        await cache.put(req, fresh.clone());
        return fresh;
    } catch (e) {
        const cached = await cache.match(req);
        return cached;
    }
} */
