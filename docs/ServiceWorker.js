const cacheName = "DefaultCompany-FIT-1.0";
const contentToCache = [
    "Build/e5054e290e5f4567834e43329155f23c.loader.js",
    "Build/1ab213b6743ca9a8cc0e41192190b006.framework.js",
    "Build/e1f524b0bf577bcfbdde0906adb34ab5.data",
    "Build/83bedea8806f3c912557c9b298a3bf7b.wasm",
    "TemplateData/style.css"

];

self.addEventListener('install', function (e) {
    console.log('[Service Worker] Install');
    
    e.waitUntil((async function () {
      const cache = await caches.open(cacheName);
      console.log('[Service Worker] Caching all: app shell and content');
      await cache.addAll(contentToCache);
    })());
});

self.addEventListener('fetch', function (e) {
    e.respondWith((async function () {
      let response = await caches.match(e.request);
      console.log(`[Service Worker] Fetching resource: ${e.request.url}`);
      if (response) { return response; }

      response = await fetch(e.request);
      const cache = await caches.open(cacheName);
      console.log(`[Service Worker] Caching new resource: ${e.request.url}`);
      cache.put(e.request, response.clone());
      return response;
    })());
});
