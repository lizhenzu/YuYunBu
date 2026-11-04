// wwwroot/js/content-push.js
// 도전!!! 성경말씀 플랫폼으로 한 달치를 하루씩 차례로 보내며 진행 막대를 그린다 (말씀 · 퀴즈 공용)
//   ContentPush.run({ panel, url, token, items: [{ id, date }], label })
//   - url 에 id 와 __RequestVerificationToken 을 POST 하고, 응답 { ok, skipped, fatal, message } 를 받는다
//   - fatal(401 · 404 · 503)이면 남은 날은 보내지 않고 멈춘다
//   - 성공한 날도 플랫폼이 돌려준 내용(예: 구간 2개로 바꿨어요)을 목록에 적는다
//   - 끝나면 실패한 날만 다시 보내는 버튼을 보여 준다
(function () {
    var CSS =
        '.cpu{margin-top:12px}' +
        '.cpu-bar{height:10px;border-radius:999px;background:#E5E7EB;overflow:hidden}' +
        '.cpu-bar i{display:block;height:100%;width:0;background:#2F80ED;transition:width .2s}' +
        '.cpu-cnt{margin-top:8px;display:flex;justify-content:space-between;gap:8px;font-size:12.5px;color:#374151}' +
        '.cpu-cnt b{font-size:14px;color:#111827}' +
        '.cpu-sum{margin-top:6px;font-size:12px;color:#6B7280}' +
        '.cpu-sum .ok{color:#15803D;font-weight:700}.cpu-sum .sk{color:#B45309;font-weight:700}.cpu-sum .er{color:#B42318;font-weight:700}' +
        '.cpu-stop{margin-top:8px;padding:9px 12px;border-radius:10px;background:#FDECEC;color:#B42318;font-size:12.5px;font-weight:700;line-height:1.5}' +
        '.cpu-log{margin:8px 0 0;padding:0;list-style:none;display:grid;gap:4px;max-height:240px;overflow:auto}' +
        '.cpu-log li{font-size:12px;line-height:1.5;padding:6px 10px;border-radius:10px;background:#fff;color:#374151;word-break:break-all}' +
        '.cpu-log li.er{color:#B42318}.cpu-log li.sk{color:#B45309}.cpu-log li.ok{color:#15803D}' +
        '.cpu-retry{margin-top:10px;width:100%;padding:10px;border:0;border-radius:12px;font:inherit;font-size:13px;font-weight:800;background:#fff;color:#2F80ED;box-shadow:inset 0 0 0 1px #C9DDF8;cursor:pointer}';

    function addStyle() {
        if (document.getElementById('cpu-style')) return;
        var st = document.createElement('style');
        st.id = 'cpu-style';
        st.textContent = CSS;
        document.head.appendChild(st);
    }

    async function pushOne(url, token, item) {
        var body = new URLSearchParams({ id: item.id, __RequestVerificationToken: token });
        try {
            var res = await fetch(url, { method: 'POST', body: body, credentials: 'same-origin' });
            if (!res.ok) return { ok: false, message: '우리 서버 오류 (' + res.status + ')' };
            var d = await res.json();
            return { ok: !!d.ok, skipped: !!d.skipped, fatal: !!d.fatal, message: d.message || '' };
        } catch (e) {
            return { ok: false, message: '우리 서버에 연결하지 못했어요' };
        }
    }

    var busy = false;

    async function run(opt) {
        if (busy) return;
        busy = true;
        addStyle();

        var list = opt.items.slice();
        var panel = opt.panel;
        panel.innerHTML =
            '<div class="cpu">' +
            '<div class="cpu-bar"><i></i></div>' +
            '<div class="cpu-cnt"><span class="st">보내는 중…</span><span><span class="tt">0</span>개 중 <b class="dn">0</b>개 보냄</span></div>' +
            '<div class="cpu-sum">성공 <span class="ok">0</span> · 건너뜀 <span class="sk">0</span> · 실패 <span class="er">0</span></div>' +
            '<div class="cpu-stop" hidden></div>' +
            '<ul class="cpu-log"></ul>' +
            '</div>';
        var q = function (s) { return panel.querySelector(s); };
        var done = 0, ok = 0, sk = 0, er = 0, failed = [], stopped = null;
        q('.tt').textContent = list.length;

        function paint() {
            q('.dn').textContent = done;
            q('.ok').textContent = ok; q('.sk').textContent = sk; q('.er').textContent = er;
            q('.cpu-bar i').style.width = (list.length ? done / list.length * 100 : 100) + '%';
        }
        function log(cls, text) {
            var li = document.createElement('li');
            li.className = cls;
            li.textContent = text;
            q('.cpu-log').appendChild(li);
        }
        paint();

        for (var i = 0; i < list.length; i++) {
            var item = list[i];
            var r = await pushOne(opt.url, opt.token, item);
            done++;
            if (r.ok) { ok++; log('ok', item.date + ' · ' + (r.message || '성공')); }
            else if (r.skipped) { sk++; log('sk', item.date + ' · ' + r.message); }
            else { er++; failed.push(item); log('er', item.date + ' · ' + r.message); }
            paint();
            if (r.fatal) {
                stopped = r.message;
                for (var k = i + 1; k < list.length; k++) failed.push(list[k]);
                break;
            }
        }

        if (stopped) {
            q('.st').textContent = '멈췄어요';
            var box = q('.cpu-stop');
            box.hidden = false;
            box.textContent = '남은 ' + (list.length - done) + '일은 보내지 않았어요. ' + stopped;
        } else {
            q('.st').textContent = er === 0 ? '다 보냈어요' : '끝났어요 · 실패한 날이 있어요';
        }

        if (failed.length > 0) {
            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'cpu-retry';
            btn.textContent = '실패한 ' + failed.length + '일만 다시 보내기';
            btn.onclick = function () { run(Object.assign({}, opt, { items: failed })); };
            panel.querySelector('.cpu').appendChild(btn);
        }
        busy = false;
        if (opt.onDone) opt.onDone({ ok: ok, skipped: sk, failed: er, stopped: !!stopped });
    }

    window.ContentPush = { run: run, isBusy: function () { return busy; } };
})();
