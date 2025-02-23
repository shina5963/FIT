const cacheName = "DefaultCompany-FIT-1.0";
const contentToCache = [
    "Build/3c0f853f17e03fdaa6dd4ed860004e67.loader.js",
    "Build/1ab213b6743ca9a8cc0e41192190b006.framework.js",
    "Build/f83aea4229cac8c269f86e534f36321f.data",
    "Build/4545b341c0fec5d9dc8eaa6ab78efa5a.wasm",
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
