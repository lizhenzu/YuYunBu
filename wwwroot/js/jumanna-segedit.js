/* ══════════════════════════════════════════════════════════════
   매일성경 영상 구간 편집 (/JuManNa/SegmentEditor) — 2026-10-18
   줄(.jm-segrow)의 ▶ → 영상 창.  [시작 시점 설정] · [종료 시점 설정] 을 누르면
   지금 재생 위치(초, 버림)를 /JuManNa/SaveSegment 로 바로 저장하고 줄의 값을 바꾼다.
   sec 0 = 기본값(시작 → 영상 처음, 종료 → 영상 끝).  영상 창 모양은 jumanna-bright.css 의 .jm-yt
   ══════════════════════════════════════════════════════════════ */
(function () {
    'use strict';

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
    function $(id) { return document.getElementById(id); }

    var root, player = null, ready = false, apiAsked = false, clock = null, stopAt = 0, busy = false;
    var row = null;            // 지금 편집 중인 줄
    var seg = { id: 0, slot: 0, vid: '', len: 0, s: 0, e: 0 };

    function build() {
        root = document.createElement('div');
        root.className = 'jm-yt jm-se';
        root.innerHTML =
            '<div class="box" role="dialog" aria-modal="true" aria-label="영상 구간 편집">' +
            '<div class="hd"><span class="t"><b id="seT"></b><small id="seR"></small></span>' +
            '<button type="button" class="x" id="seX" aria-label="닫기">✕</button></div>' +
            '<div class="v"><div id="sePlayer"></div></div>' +
            '<div class="jm-se-now"><span>현재 위치</span><b id="seNow">0:00</b><small id="seLen"></small></div>' +
            '<div class="jm-se-row">' +
            '<button type="button" data-a="back">−1초</button>' +
            '<button type="button" data-a="fwd">+1초</button>' +
            '<button type="button" class="m" data-a="setS">⚑ 시작 시점 설정</button>' +
            '<button type="button" class="m" data-a="setE">⚑ 종료 시점 설정</button>' +
            '</div>' +
            '<div class="jm-se-row">' +
            '<button type="button" data-a="play">▶ 구간 재생 확인</button>' +
            '<button type="button" data-a="resetS">시작 → 처음으로</button>' +
            '<button type="button" data-a="resetE">종료 → 끝으로</button>' +
            '<a id="seOrigin" href="#" target="_blank" rel="noopener">유튜브 ↗</a>' +
            '</div>' +
            '<div class="jm-se-msg" id="seMsg" aria-live="polite"></div>' +
            '</div>';
        document.body.appendChild(root);
        root.addEventListener('click', function (e) {
            if (e.target === root) { close(); return; }
            var b = e.target.closest('[data-a]');
            if (b) act(b.getAttribute('data-a'));
        });
        $('seX').onclick = close;
        document.addEventListener('keydown', function (e) { if (e.key === 'Escape' && root.classList.contains('on')) close(); });
    }

    function msg(t, kind) {
        var m = $('seMsg');
        m.textContent = t || '';
        m.className = 'jm-se-msg' + (kind ? ' ' + kind : '');
    }
    function endOf() { return seg.e > 0 ? seg.e : seg.len; }
    function rangeText() { $('seR').textContent = '구간 ' + fmt(seg.s) + ' ~ ' + fmt(endOf()) + (seg.e > 0 ? '' : ' (끝)'); }

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
    function now() { return ready && typeof player.getCurrentTime === 'function' ? Math.floor(player.getCurrentTime() || 0) : 0; }

    function startClock() {
        stopClock();
        clock = setInterval(function () {
            if (!ready) return;
            var t = now();
            $('seNow').textContent = fmt(t);
            if (stopAt > 0 && t >= stopAt) { player.pauseVideo(); stopAt = 0; msg('구간 재생 끝', ''); }
        }, 250);
    }
    function stopClock() { if (clock) { clearInterval(clock); clock = null; } }

    function load() {
        ready = false;
        if (player && typeof player.loadVideoById === 'function') {
            player.loadVideoById({ videoId: seg.vid, startSeconds: seg.s });
            ready = true;
            return;
        }
        player = new YT.Player('sePlayer', {
            videoId: seg.vid,
            playerVars: { autoplay: 1, start: seg.s, controls: 1, rel: 0, modestbranding: 1, playsinline: 1 },
            events: { onReady: function (e) { ready = true; e.target.seekTo(seg.s, true); e.target.playVideo(); } }
        });
    }

    function open(r) {
        if (!root) build();
        row = r;
        seg.id = +r.getAttribute('data-id');
        seg.slot = +r.getAttribute('data-slot');
        seg.vid = ytId(r.getAttribute('data-yt'));
        seg.len = +r.getAttribute('data-len') || 0;
        seg.s = +r.getAttribute('data-s') || 0;
        seg.e = +r.getAttribute('data-e') || 0;
        if (!seg.vid) return;

        $('seT').textContent = r.getAttribute('data-title') || '영상 구간';
        $('seLen').textContent = seg.len ? '/ ' + fmt(seg.len) : '';
        $('seNow').textContent = fmt(seg.s);
        $('seOrigin').href = 'https://www.youtube.com/watch?v=' + encodeURIComponent(seg.vid);
        rangeText();
        msg('', '');
        stopAt = 0;
        root.classList.add('on');
        document.body.classList.add('jm-noscroll');
        api().then(function () { load(); startClock(); });
    }
    function close() {
        root.classList.remove('on');
        document.body.classList.remove('jm-noscroll');
        stopClock();
        stopAt = 0;
        if (player && typeof player.pauseVideo === 'function') try { player.pauseVideo(); } catch (e) { }
    }

    function act(a) {
        if (a === 'back' || a === 'fwd') {
            if (!ready) return;
            var t = Math.max(0, now() + (a === 'back' ? -1 : 1));
            if (seg.len) t = Math.min(seg.len, t);
            player.seekTo(t, true);
            $('seNow').textContent = fmt(t);
            return;
        }
        if (a === 'play') {
            if (!ready) return;
            player.seekTo(seg.s, true);
            player.playVideo();
            stopAt = seg.e > 0 ? seg.e : 0;
            msg(fmt(seg.s) + ' ~ ' + fmt(endOf()) + ' 구간 재생 중', '');
            return;
        }
        if (a === 'setS') return save('start', now());
        if (a === 'setE') return save('end', now());
        if (a === 'resetS') return save('start', 0);
        if (a === 'resetE') return save('end', 0);
    }

    // 서버와 같은 검사를 먼저 해서 헛걸음을 줄인다(최종 판단은 서버)
    function check(kind, sec) {
        if (seg.len && sec > seg.len) return '영상 길이(' + fmt(seg.len) + ')를 넘었어요';
        if (kind === 'start' && sec > 0 && sec >= endOf()) return '시작은 종료(' + fmt(endOf()) + ')보다 앞이어야 해요';
        if (kind === 'end' && sec > 0 && sec <= seg.s) return '종료는 시작(' + fmt(seg.s) + ')보다 뒤여야 해요';
        return '';
    }

    function save(kind, sec) {
        if (busy) return;
        if ((kind === 'start' || kind === 'end') && sec > 0 && !ready) { msg('영상이 아직 준비되지 않았어요', 'err'); return; }
        var bad = check(kind, sec);
        if (bad) { msg(bad, 'err'); return; }

        var tok = document.querySelector('#segTok input[name="__RequestVerificationToken"]');
        var fd = new FormData();
        fd.append('__RequestVerificationToken', tok ? tok.value : '');
        fd.append('id', seg.id);
        fd.append('slot', seg.slot);
        fd.append('kind', kind);
        fd.append('sec', sec);

        busy = true;
        msg('저장하는 중…', '');
        fetch('/JuManNa/SaveSegment', { method: 'POST', body: fd, credentials: 'same-origin' })
            .then(function (r) { if (!r.ok) throw new Error(r.status); return r.json(); })
            .then(function (j) {
                if (!j || !j.ok) { msg((j && j.msg) || '저장하지 못했어요', 'err'); return; }
                seg.s = j.start; seg.e = j.end; if (j.len) seg.len = j.len;
                paintRow(kind);
                rangeText();
                var label = kind === 'start' ? '시작' : '종료';
                msg(sec === 0
                    ? '저장됨 · ' + (kind === 'start' ? '시작을 영상 처음으로' : '종료를 영상 끝으로')
                    : '저장됨 · ' + label + ' ' + Math.floor(sec / 60) + '분 ' + (sec % 60) + '초', 'ok');
            })
            .catch(function () { msg('저장하지 못했어요. 로그인 상태를 확인하고 다시 눌러 주세요', 'err'); })
            .then(function () { busy = false; });
    }

    function paintRow(kind) {
        row.setAttribute('data-s', seg.s);
        row.setAttribute('data-e', seg.e);
        var sv = row.querySelector('[data-v="s"]'), ev = row.querySelector('[data-v="e"]');
        sv.textContent = fmt(seg.s); sv.classList.toggle('def', !(seg.s > 0));
        ev.textContent = fmt(endOf()); ev.classList.toggle('def', !(seg.e > 0));
        var el = kind === 'start' ? sv : ev;
        el.classList.remove('flash'); void el.offsetWidth; el.classList.add('flash');
    }

    document.addEventListener('click', function (e) {
        var b = e.target.closest('.jm-segplay');
        if (b && !b.classList.contains('off')) { e.preventDefault(); open(b.closest('.jm-segrow')); }
    });
})();
