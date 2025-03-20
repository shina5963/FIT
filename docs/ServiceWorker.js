const cacheName = "DefaultCompany-FIT-1.0";
const contentToCache = [
    "Build/d2779af774c30fbec55ad595eb05d392.loader.js",
    "Build/1ab213b6743ca9a8cc0e41192190b006.framework.js",
    "Build/fb6b1d00bf86c6b24de9be0ba8c98db8.data",
    "Build/529de8b8afbf556d8b951f5c9d378148.wasm",
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
