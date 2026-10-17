/* BulletinBoard PWA Service Worker */

const CACHE_VERSION = "bb-pwa-v2"; // v2: 내비게이션 캐시 제거
const CORE_CACHE = `${CACHE_VERSION}-core`;
const RUNTIME_CACHE = `${CACHE_VERSION}-runtime`;

// ✅ install 단계에서 "반드시 200" 나오는 정적 리소스만 넣기
const CORE_ASSETS = [
    "/",
    "/manifest.json",
    "/images/ICON/church/pwa-192.png",
    "/images/ICON/church/pwa-512.png"
];

// 설치
self.addEventListener("install", (event) => {
    event.waitUntil(
        caches.open(CORE_CACHE).then((cache) => {
            // install 실패 방지: addAll은 하나라도 실패하면 전체 실패하므로
            // CORE_ASSETS는 반드시 존재/200인 것만 넣습니다.
            return cache.addAll(CORE_ASSETS);
        })
    );
    self.skipWaiting();
});

// 활성화(구버전 캐시 정리)
self.addEventListener("activate", (event) => {
    event.waitUntil(
        caches.keys().then((keys) =>
            Promise.all(
                keys.map((key) => {
                    if (!key.startsWith(CACHE_VERSION)) {
                        return caches.delete(key);
                    }
                })
            )
        )
    );
    self.clients.claim();
});

// fetch: 기본은 네트워크 우선(동적 화면), 정적은 캐시 우선
self.addEventListener("fetch", (event) => {
    const req = event.request;
    if (req.method !== "GET") return;

    // ✅ 내비게이션(HTML 페이지 이동)은 서비스워커 개입 없이 브라우저 직접 처리
    // — 동적 서버 렌더링 페이지는 캐시 불필요, clone+put 오버헤드만 발생
    if (req.mode === "navigate") return;

    const url = new URL(req.url);

    // 같은 오리진만 캐시
    if (url.origin !== self.location.origin) return;

    const isStaticAsset =
        url.pathname.startsWith("/css/") ||
        url.pathname.startsWith("/js/") ||
        url.pathname.startsWith("/images/") ||
        url.pathname.startsWith("/img/") ||
        url.pathname.endsWith(".png") ||
        url.pathname.endsWith(".jpg") ||
        url.pathname.endsWith(".jpeg") ||
        url.pathname.endsWith(".webp") ||
        url.pathname.endsWith(".svg") ||
        url.pathname.endsWith(".ico") ||
        url.pathname.endsWith(".woff") ||
        url.pathname.endsWith(".woff2") ||
        url.pathname.endsWith(".ttf");

    if (isStaticAsset) {
        // ✅ 정적 리소스: cache-first
        event.respondWith(
            caches.match(req).then((cached) => {
                if (cached) return cached;

                return fetch(req)
                    .then((res) => {
                        const copy = res.clone();
                        caches.open(RUNTIME_CACHE).then((cache) => cache.put(req, copy));
                        return res;
                    })
                    .catch(() => cached);
            })
        );
        return;
    }

    // ✅ 화면/API 등 동적: network-first (오프라인이면 캐시 fallback)
    event.respondWith(
        fetch(req)
            .then((res) => {
                const copy = res.clone();
                caches.open(RUNTIME_CACHE).then((cache) => cache.put(req, copy));
                return res;
            })
            .catch(() => caches.match(req).then((cached) => cached || caches.match("/")))
    );
});
