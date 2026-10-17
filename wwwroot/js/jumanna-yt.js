/* ══════════════════════════════════════════════════════════════
   매일성경 영상 구간 재생 창 (/JuManNa/IndexByID · IndexGroupByYYYYMMSerial 공용) — 2026-10-04
   버튼에 data-yt-url · data-start(초) · data-end(초, 0 이면 끝까지) · data-title · data-label 을 달면
   눌렀을 때 화면 안 영상 창에서 그 구간만 재생한다.
   body 에 data-yt-loop="1" 이 있으면 구간 끝에서 처음으로 돌아가 반복(상세 화면 예전 동작),
   없으면 구간 끝에서 멈춘다(영상 구간 목록 예전 동작).
   ══════════════════════════════════════════════════════════════ */
(function () {
    'use strict';

    // 주소 · 영상 ID 어느 쪽이 와도 ID 만 뽑는다
    function ytId(s) {
        s = (s || '').trim();
        if (!s) return '';
        if (s.indexOf('/') < 0 && s.indexOf('?') < 0 && s.indexOf('&') < 0) return s;
        var m = s.match(/[?&]v=([^&#]+)/) || s.match(/youtu\.be\/([^?&#/]+)/) || s.match(/\/(?:embed|shorts|live)\/([^?&#/]+)/);
        return m ? m[1] : s;
    }
    function fmt(sec) {
        sec = Math.max(0, Math.floor(sec || 0));
        return Math.floor(sec / 60) + ':' + String(sec % 60).padStart(2, '0');
    }

    var root, player = null, tick = null, apiAsked = false;
    var cur = { id: '', start: 0, end: 0 };
    var loop = false;

    function build() {
        root = document.createElement('div');
        root.className = 'jm-yt';
        root.innerHTML =
            '<div class="box" role="dialog" aria-modal="true" aria-label="영상 구간 재생">' +
            '<div class="hd"><span class="t"><b id="jmYtT"></b><small id="jmYtS"></small></span>' +
            '<button type="button" class="x" id="jmYtX" aria-label="닫기">✕</button></div>' +
            '<div class="v"><div id="jmYtPlayer"></div></div>' +
            '<div class="fb"><span id="jmYtState">준비 중</span>' +
            '<button type="button" id="jmYtRe">처음부터 다시</button>' +
            '<a id="jmYtOrigin" href="#" target="_blank" rel="noopener">유튜브에서 보기 ↗</a></div></div>';
        document.body.appendChild(root);
        root.addEventListener('click', function (e) { if (e.target === root) close(); });
        document.getElementById('jmYtX').onclick = close;
        document.getElementById('jmYtRe').onclick = restart;
        document.addEventListener('keydown', function (e) { if (e.key === 'Escape' && root.classList.contains('on')) close(); });
    }
    function state(t) { document.getElementById('jmYtState').textContent = t; }

    function api() {
        return new Promise(function (resolve) {
            if (window.YT && window.YT.Player) { resolve(); return; }
            var prev = window.onYouTubeIframeAPIReady;
            window.onYouTubeIframeAPIReady = function () { if (prev) try { prev(); } catch (e) { } resolve(); };
            if (!apiAsked) {
                apiAsked = true;
                var s = document.createElement('script');
                s.src = 'https://www.youtube.com/iframe_api';
                document.head.appendChild(s);
            } else {
                var w = setInterval(function () { if (window.YT && window.YT.Player) { clearInterval(w); resolve(); } }, 100);
            }
        });
    }

    function stopTick() { if (tick) { clearInterval(tick); tick = null; } }
    function startTick() {
        stopTick();
        if (!(cur.end > 0)) return;
        tick = setInterval(function () {
            if (!player || typeof player.getCurrentTime !== 'function') return;
            if (player.getCurrentTime() >= cur.end) {
                if (loop) { player.seekTo(cur.start, true); }
                else { player.pauseVideo(); state('구간 재생 완료'); stopTick(); }
            }
        }, 250);
    }

    function play() {
        if (player && typeof player.loadVideoById === 'function') {
            stopTick();
            player.loadVideoById({ videoId: cur.id, startSeconds: cur.start });
            return;
        }
        player = new YT.Player('jmYtPlayer', {
            videoId: cur.id,
            playerVars: { autoplay: 1, start: cur.start, controls: 1, rel: 0, modestbranding: 1, playsinline: 1 },
            events: {
                onReady: function (e) { e.target.seekTo(cur.start, true); e.target.playVideo(); },
                onStateChange: function (e) {
                    if (e.data === YT.PlayerState.PLAYING) { state('재생 중'); startTick(); }
                    else if (e.data === YT.PlayerState.PAUSED) { if (tick) state('일시정지'); }
                    else if (e.data === YT.PlayerState.ENDED) { state('끝'); stopTick(); }
                }
            }
        });
    }

    function open(btn) {
        var id = ytId(btn.getAttribute('data-yt-url'));
        if (!id) return;
        if (!root) build();
        cur.id = id;
        cur.start = Math.max(0, parseInt(btn.getAttribute('data-start') || '0', 10) || 0);
        cur.end = Math.max(0, parseInt(btn.getAttribute('data-end') || '0', 10) || 0);
        if (cur.end <= cur.start) cur.end = 0;
        loop = document.body.getAttribute('data-yt-loop') === '1';

        document.getElementById('jmYtT').textContent = btn.getAttribute('data-title') || '영상 구간';
        document.getElementById('jmYtS').textContent = fmt(cur.start) + ' ~ ' + (cur.end ? fmt(cur.end) : '끝까지') + ' 구간';
        document.getElementById('jmYtOrigin').href = 'https://www.youtube.com/watch?v=' + encodeURIComponent(id) + (cur.start ? '&t=' + cur.start + 's' : '');
        root.classList.add('on');
        document.body.classList.add('jm-noscroll');
        state('불러오는 중');
        api().then(play);
    }
    function restart() {
        if (!player || typeof player.seekTo !== 'function') return;
        player.seekTo(cur.start, true);
        player.playVideo();
    }
    function close() {
        root.classList.remove('on');
        document.body.classList.remove('jm-noscroll');
        stopTick();
        if (player && typeof player.pauseVideo === 'function') try { player.pauseVideo(); } catch (e) { }
    }

    document.addEventListener('click', function (e) {
        var b = e.target.closest('[data-yt-url]');
        if (b) { e.preventDefault(); open(b); }
    });
    window.jmYtId = ytId;
})();
