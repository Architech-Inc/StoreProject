/**
 * ClexAn Foods POS Service Worker
 * Provides offline shell caching, asset caching, and background sync triggers.
 */

const CACHE_NAME = 'clexan-pos-v1';
const ASSETS_TO_CACHE = [
  '/',
  '/Pos',
  '/css/pos.css',
  '/css/site.css',
  '/css/modules/sales.css',
  '/js/site.js',
  '/js/pos-offline.js'
];

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => {
      return cache.addAll(ASSETS_TO_CACHE).catch((err) => {
        console.warn('[SW] Pre-caching non-fatal warning:', err);
      });
    }).then(() => self.skipWaiting())
  );
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((keys) => {
      return Promise.all(
        keys.map((key) => {
          if (key !== CACHE_NAME) {
            return caches.delete(key);
          }
        })
      );
    }).then(() => self.clients.claim())
  );
});

self.addEventListener('fetch', (event) => {
  const request = event.request;

  // For navigation requests to POS, try network first, then fall back to cache
  if (request.mode === 'navigate') {
    event.respondWith(
      fetch(request).catch(() => caches.match('/Pos'))
    );
    return;
  }

  // For static CSS/JS/images, cache first with network fallback
  if (request.destination === 'style' || request.destination === 'script' || request.destination === 'image') {
    event.respondWith(
      caches.match(request).then((cachedResponse) => {
        if (cachedResponse) {
          return cachedResponse;
        }
        return fetch(request).then((networkResponse) => {
          if (networkResponse && networkResponse.status === 200) {
            const responseClone = networkResponse.clone();
            caches.open(CACHE_NAME).then((cache) => cache.put(request, responseClone));
          }
          return networkResponse;
        });
      })
    );
    return;
  }

  // For API or checkout handler requests, network only (managed by IndexedDB offline queue on client)
  event.respondWith(fetch(request));
});
