// wwwroot/js/family-team-search.js — 가족팀 이름 조회 (팀 있음 · 팀 없음 화면 공용)
//   #ftSearch 의 data 값
//     data-invite="1"  : 초대할 수 있다 (내가 팀원). 팀 없는 사람에게 [초대] 버튼
//     data-apply="1"   : 다른 팀에 신청할 수 있다 (팀이 없거나, 내 팀에 나 혼자)
//     data-solo="내 팀 이름" : 내 팀에 나 혼자 — 신청하면 내 팀이 지워진다고 알린다
//     data-blocked="1" : 이미 다른 팀에 신청 중
//     data-rels        : 관계 목록 JSON [{k:'dad', l:'아빠'}, ...]
(function () {
    var root = document.getElementById('ftSearch');
    if (!root) return;
    var q = document.getElementById('ftQ'), res = document.getElementById('ftRes');
    var canInvite = root.dataset.invite === '1', canApply = root.dataset.apply === '1', blocked = root.dataset.blocked === '1';
    var solo = root.dataset.solo || '';
    var rels = [];
    try { rels = JSON.parse(root.dataset.rels || '[]'); } catch (e) { }
    var tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
    var timer = 0, seq = 0, formNo = 0;

    function esc(s) { var d = document.createElement('div'); d.textContent = s == null ? '' : s; return d.innerHTML.replace(/"/g, '&quot;'); }

    function applyForm(h) {
        var name = 'ftRel' + (++formNo);
        var chips = rels.map(function (r, i) {
            return '<label><input type="radio" name="relation" value="' + esc(r.k) + '" required' + (rels.length === 1 ? ' checked' : '') + ' /><span>' + esc(r.l) + '</span></label>';
        }).join('');
        var warn = solo ? '<div class="ft-mem" style="margin-top:8px;color:var(--red)">신청하면 내가 만든 "' + esc(solo) + '"은 바로 삭제돼요.</div>' : '';
        var conf = solo ? ' data-confirm="' + esc(h.teamName + '에 가입 신청할까요?|내가 만든 "' + solo + '"은 바로 삭제돼요.|신청') + '"' : '';
        return '<details class="ft-ap"><summary><span class="ft-btn p" style="margin-top:8px;display:inline-block">이 팀에 가입 신청</span></summary>' +
            '<form class="box" method="post" action="/FamilyTeam/Apply" id="' + name + '"' + conf + '>' +
            '<input type="hidden" name="__RequestVerificationToken" value="' + esc(tokenEl ? tokenEl.value : '') + '" />' +
            '<input type="hidden" name="teamId" value="' + esc(h.teamId) + '" />' +
            '<input type="hidden" name="type" value="' + esc(root.dataset.mode === 'ALL' ? (h.teamType || 'FAMILY') : (root.dataset.mode || 'FAMILY')) + '" />' +
            (h.teamType === 'FRIEND' ? '' : '<div class="ft-q" style="margin-top:0">나는 이 팀의…</div><div class="ft-chips">' + chips + '</div>') + warn +
            '<div style="margin-top:12px"><button class="ft-btn p w" type="submit">신청 보내기</button></div>' +
            '</form></details>';
    }

    function row(h) {
        var mode = root.dataset.mode || 'FAMILY';
        if (mode === 'ALL') mode = h.teamType || 'ALL';   // 참가 화면 — 가족팀 · 친구팀 모두 찾는다
        var friendMode = mode === 'FRIEND';
        var kind = h.teamType === 'FRIEND' ? '친구팀' : '가족팀';
        var info = '', action = '';
        if (h.teamId) {
            info = '<div class="ft-mem"><b>' + esc(h.teamName) + '</b> · ' + kind + (h.isFull ? ' · 정원 찼음' : '') + '</div>' +
                   '<div class="ft-mem">구성원: ' + esc((h.members || []).join(' · ')) + '</div>';
        } else {
            info = '<div class="ft-mem">소속 팀 없음' + (friendMode && !h.isStudent ? ' · 학생이 아니에요' : '') + '</div>';
        }

        if (h.state === 'mine') {
            action = '<span class="ft-btn g">우리 팀</span>';
        } else if (h.state === 'invited') {
            action = '<span class="ft-btn g">초대 보냄</span>';
        } else if (h.state === 'ok') {
            // 역할 기준 — 교사 · 임원(2 · 3)은 팀에 초대할 수 없다. 학생은 '학생'으로 초대한다
            if (canInvite && (h.roleKind === 2 || h.roleKind === 3)) action = '<span class="ft-btn g">교사 · 임원</span>';
            else if (canInvite && (!friendMode || h.isStudent)) action = '<button type="button" class="ft-btn p" data-u="' + esc(h.userName) + '" data-st="' + (h.isStudent ? '1' : '0') + '">초대</button>';
        } else if (h.teamType !== mode) {
            action = '<span class="ft-btn g">다른 종류의 팀</span>';
        } else if (h.isFull) {
            action = '<span class="ft-btn g">정원 찼음</span>';
        } else if (h.requested) {
            info += '<div class="ft-mem" style="color:var(--blue);font-weight:700">이 팀에 가입 신청을 보냈어요</div>';
        } else if (canApply && blocked) {
            info += '<button type="button" class="ft-btn g" disabled aria-disabled="true" style="margin-top:8px;opacity:.55;cursor:not-allowed">이 팀에 가입 신청</button>' +
                    '<div class="ft-mem" style="margin-top:4px">다른 팀에 신청 중이에요. 그 신청을 취소하면 신청할 수 있어요.</div>';
        } else if (canApply) {
            info += applyForm(h);
        }

        return '<div class="ft-row" style="align-items:flex-start"><div class="l"><div class="ft-nm">' + esc(h.fullName) + '</div>' + info + '</div>' + action + '</div>';
    }

    // 찾은 사람이 없을 때 — 팀 만들기로 이어 준다 (data-create-url 이 있고, 다른 팀에 신청 중이 아닐 때)
    function emptyBox(v) {
        var url = root.dataset.createUrl || '';
        if (!url) return '<div class="ft-empty">찾은 사람이 없어요.</div>';   // 팀이 있는 화면(초대 찾기) — 만들기로 이어 주지 않는다
        var all = root.dataset.mode === 'ALL';
        var word = all ? '팀' : (root.dataset.mode === 'FRIEND') ? '친구팀' : '가족팀';
        var who = all ? '찾는 사람이' : (root.dataset.mode === 'FRIEND') ? '친구가' : '가족이';
        var btn = (url && !blocked) ? '<a class="ft-btn p w" href="' + esc(url) + '" style="margin-top:14px">새 ' + word + ' 만들기</a>' : '';
        return '<div class="ft-empty">"' + esc(v) + '" 님이 속한 팀이 없어요.' + (url ? '<br>' + who + ' 아직 팀을 만들지 않았나 봐요.' : '') + btn + '</div>';
    }

    q.addEventListener('input', function () {
        clearTimeout(timer);
        var v = q.value.trim();
        if (v.length < 2) { seq++; res.innerHTML = ''; return; }
        timer = setTimeout(function () {
            var my = ++seq;
            fetch('/FamilyTeam/Search?q=' + encodeURIComponent(v), { headers: { 'Accept': 'application/json' }, credentials: 'same-origin' })
                .then(function (r) { if (!r.ok) throw new Error('search'); return r.json(); })
                .then(function (list) {
                    if (my !== seq) return;
                    res.innerHTML = list.length ? list.map(row).join('') : emptyBox(v);
                })
                .catch(function () { if (my === seq) res.innerHTML = '<div class="ft-empty">조회하지 못했어요. 잠시 후 다시 해 주세요.</div>'; });
        }, 250);
    });
    // 검색 버튼(#ftGo) · 엔터 → 입력할 때와 같은 찾기를 다시 돌린다 (2026-11-08, 홈 "말씀 찾기" 모양 검색창)
    function rerun() { q.dispatchEvent(new Event('input')); q.focus(); }
    var go = document.getElementById('ftGo');
    if (go) go.addEventListener('click', rerun);
    q.addEventListener('keydown', function (e) { if (e.key === 'Enter') { e.preventDefault(); rerun(); } });

    // [초대] — 화면의 초대 폼(관계 칩이 있는 곳)으로 보낸다
    res.addEventListener('click', function (e) {
        var b = e.target.closest('button[data-u]');
        if (!b) return;
        var form = document.getElementById('invForm'), tgt = document.getElementById('invTarget');
        if (!form || !tgt) return;
        var rel = document.getElementById('invRel');
        if (b.getAttribute('data-st') === '1' && rel) {
            // 학생은 언제나 '학생' 관계 — 고른 칩은 보내지 않는다
            form.querySelectorAll('input[type=radio][name=relation]').forEach(function (x) { x.checked = false; });
            rel.setAttribute('name', 'relation');
        } else {
            if (rel) rel.removeAttribute('name');
            if (!form.reportValidity()) return;   // 학부모는 관계를 먼저 고른다
        }
        tgt.value = b.getAttribute('data-u');
        b.disabled = true;
        form.submit();
    });
})();
