const cacheName = "DefaultCompany-FIT-1.0";
const contentToCache = [
    "Build/3cce54889b523666956c99ad121652b2.loader.js",
    "Build/1ab213b6743ca9a8cc0e41192190b006.framework.js",
    "Build/6e6bb18104f988c2001ba813e649ca20.data",
    "Build/70498f04f7c044412387b87f070e534b.wasm",
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
