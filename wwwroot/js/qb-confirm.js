// wwwroot/js/qb-confirm.js
// 퀴즈 · 말씀 정리의 밝은 화면(quiz-bright.css)용 확인 팝업 — 2026-11-07 (가운데 카드)
//   qbConfirm(showCancelButton, messageArray, imageUrl, confirmButtonText, cancelButtonText)
//   - 예전 confirmMessage(common2.js)와 같은 모양으로 부르고, 같은 값('y' = 확인, 'c' = 취소)을 돌려준다
//   - 이 파일을 common2.js 다음에 불러오면 그 화면의 confirmMessage 가 이 팝업으로 바뀐다 (다른 화면은 그대로)
//   - 첫 줄 = 제목, 나머지 줄 = 설명.  글은 모두 textContent 로 넣어서 태그가 실행되지 않는다
//   - 모양: "삭제" 가 들어가면 빨간 삭제 모양, "보내" 가 들어가면 보내기 모양, 취소 버튼이 없으면 안내 모양
//   - 키보드: Esc = 취소, Tab 은 팝업 안에서만 돈다.  삭제 팝업은 처음에 취소 버튼에 초점이 간다
(function () {
    var CSS =
        '.qbc-ov{position:fixed;inset:0;z-index:10000;display:flex;align-items:center;justify-content:center;padding:16px;background:rgba(17,24,39,.5);animation:qbcFade .14s ease-out}' +
        '.qbc{box-sizing:border-box;width:100%;max-width:360px;padding:20px 18px 16px;border-radius:20px;background:#fff;color:#111827;font-family:inherit;animation:qbcPop .16s ease-out}' +
        '.qbc *{box-sizing:border-box}' +
        '.qbc-ic{width:40px;height:40px;border-radius:12px;display:flex;align-items:center;justify-content:center;background:#EAF2FD;color:#2F80ED}' +
        '.qbc-ic svg{width:22px;height:22px}' +
        '.qbc.danger .qbc-ic{background:#FDECEC;color:#D9534F}' +
        '.qbc-t{margin:12px 0 0;font-size:17px;font-weight:800;line-height:1.45;word-break:keep-all;white-space:pre-line}' +
        '.qbc-d{margin:6px 0 0;font-size:13.5px;line-height:1.7;color:#6B7280;word-break:keep-all;white-space:pre-line;max-height:40vh;overflow:auto}' +
        '.qbc-d p{margin:0 0 4px}' +
        '.qbc-b{display:grid;grid-template-columns:1fr 1.6fr;gap:8px;margin-top:18px}' +
        '.qbc-b.one{grid-template-columns:1fr}' +
        '.qbc-b button{height:46px;border:0;border-radius:14px;font:inherit;font-size:14.5px;font-weight:800;cursor:pointer;background:#F4F6F9;color:#6B7280}' +
        '.qbc-b button:hover{background:#E9EDF3}' +
        '.qbc-b button.ok{background:#2F80ED;color:#fff}' +
        '.qbc-b button.ok:hover{background:#1F6FDB}' +
        '.qbc.danger .qbc-b button.ok{background:#D9534F}' +
        '.qbc.danger .qbc-b button.ok:hover{background:#C2403C}' +
        '.qbc-b button:focus-visible{outline:2px solid #2F80ED;outline-offset:2px}' +
        '@keyframes qbcFade{from{opacity:0}to{opacity:1}}' +
        '@keyframes qbcPop{from{opacity:0;transform:scale(.96)}to{opacity:1;transform:none}}' +
        '@media (prefers-reduced-motion:reduce){.qbc-ov,.qbc{animation:none}}';

    var ICONS = {
        send: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M22 2 11 13"/><path d="M22 2 15 22l-4-9-9-4 20-7z"/></svg>',
        trash: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M3 6h18"/><path d="M8 6V4h8v2"/><path d="M19 6l-1 14H6L5 6"/><path d="M10 11v6M14 11v6"/></svg>',
        info: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="12" cy="12" r="9"/><path d="M12 11v5"/><path d="M12 8h.01"/></svg>',
        ask: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="12" cy="12" r="9"/><path d="M9.5 9.5a2.5 2.5 0 0 1 5 .5c0 1.5-2.5 2-2.5 3.5"/><path d="M12 17h.01"/></svg>'
    };

    function addStyle() {
        if (document.getElementById('qbc-style')) return;
        var st = document.createElement('style');
        st.id = 'qbc-style';
        st.textContent = CSS;
        document.head.appendChild(st);
    }

    function el(tag, cls, text) {
        var e = document.createElement(tag);
        if (cls) e.className = cls;
        if (text != null) e.textContent = text;
        return e;
    }

    function qbConfirm(showCancelButton, messageArray, imageUrl, confirmButtonText, cancelButtonText) {
        return new Promise(function (resolve) {
            addStyle();

            var lines = [];
            (Array.isArray(messageArray) ? messageArray : [messageArray]).forEach(function (m) {
                String(m == null ? '' : m).split('\n').forEach(function (s) { lines.push(s); });
            });
            if (lines.length === 0) lines.push('정상적으로 처리되었습니다.');
            var title = lines[0];
            var body = lines.slice(1).filter(function (s) { return s.trim() !== ''; });

            var ask = showCancelButton !== false;
            var okText = confirmButtonText || '확인';
            var noText = cancelButtonText || '취소';
            var danger = /삭제/.test(title) || /삭제/.test(okText);
            var kind = !ask ? 'info' : danger ? 'trash' : (/보내/.test(okText) || /플랫폼/.test(title)) ? 'send' : 'ask';
            if (danger && okText === '진행') okText = '삭제';

            var prevFocus = document.activeElement;

            var ov = el('div', 'qbc-ov');
            var box = el('div', 'qbc' + (danger ? ' danger' : ''));
            box.setAttribute('role', ask ? 'alertdialog' : 'dialog');
            box.setAttribute('aria-modal', 'true');
            box.setAttribute('aria-labelledby', 'qbcTitle');

            var ic = el('div', 'qbc-ic');
            ic.innerHTML = ICONS[kind];
            var t = el('h2', 'qbc-t', title);
            t.id = 'qbcTitle';
            box.appendChild(ic);
            box.appendChild(t);
            if (body.length) {
                var d = el('div', 'qbc-d');
                body.forEach(function (s) { d.appendChild(el('p', null, s)); });
                box.appendChild(d);
            }

            var bar = el('div', 'qbc-b' + (ask ? '' : ' one'));
            var noBtn = null;
            if (ask) {
                noBtn = el('button', null, noText);
                noBtn.type = 'button';
                bar.appendChild(noBtn);
            }
            var okBtn = el('button', 'ok', okText);
            okBtn.type = 'button';
            bar.appendChild(okBtn);
            box.appendChild(bar);
            ov.appendChild(box);

            function close(value) {
                document.removeEventListener('keydown', onKey, true);
                if (ov.parentNode) ov.parentNode.removeChild(ov);
                try { if (prevFocus && prevFocus.focus) prevFocus.focus(); } catch (e) { }
                resolve(value);
            }
            function onKey(e) {
                if (e.key === 'Escape') { e.preventDefault(); e.stopPropagation(); close(ask ? 'c' : 'y'); return; }
                if (e.key !== 'Tab') return;
                var f = ask ? [noBtn, okBtn] : [okBtn];
                var i = f.indexOf(document.activeElement);
                var next = e.shiftKey ? (i <= 0 ? f.length - 1 : i - 1) : (i === f.length - 1 ? 0 : i + 1);
                e.preventDefault();
                f[next].focus();
            }

            okBtn.addEventListener('click', function () { close('y'); });
            if (noBtn) noBtn.addEventListener('click', function () { close('c'); });
            ov.addEventListener('click', function (e) { if (e.target === ov) close(ask ? 'c' : 'y'); });
            document.addEventListener('keydown', onKey, true);

            document.body.appendChild(ov);
            (danger && noBtn ? noBtn : okBtn).focus();
        });
    }

    window.qbConfirm = qbConfirm;
    window.confirmMessage = qbConfirm;   // common2.js 의 옛 팝업을 이 화면에서는 이것으로 바꾼다
})();
