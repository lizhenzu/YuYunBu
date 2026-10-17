/* ═══════════════════════════════════════════════════════════════════════════
   JUBO.JS — 어린이 주일학교 주보 스크립트
   ═══════════════════════════════════════════════════════════════════════════ */

'use strict';

// ─── Toast ──────────────────────────────────────────────────────────────────
const JuboToast = {
    show(msg, type = 'success', duration = 2800) {
        let t = document.getElementById('juboToast');
        if (!t) {
            t = document.createElement('div');
            t.id = 'juboToast';
            t.className = 'jubo-toast';
            document.body.appendChild(t);
        }
        t.textContent = msg;
        t.className = `jubo-toast ${type}`;
        setTimeout(() => t.classList.add('show'), 10);
        clearTimeout(t._timer);
        t._timer = setTimeout(() => t.classList.remove('show'), duration);
    }
};

// ─── Block Accordion ────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {

    // Toggle block open/close
    document.querySelectorAll('.jubo-block-header').forEach(header => {
        header.addEventListener('click', () => {
            const body = header.nextElementSibling;
            const btn  = header.querySelector('.jubo-block-toggle-btn');
            if (!body) return;
            const isOpen = body.classList.contains('open');
            body.classList.toggle('open', !isOpen);
            if (btn) btn.classList.toggle('open', !isOpen);
        });
    });

    // Template card selector
    document.querySelectorAll('.template-card').forEach(card => {
        card.addEventListener('click', () => {
            const val = card.dataset.value;
            document.querySelectorAll('.template-card').forEach(c => {
                c.classList.remove('active');
            });
            card.classList.add('active');
            const inp = document.querySelector('input[name="TemplateType"]');
            if (inp) inp.value = val;
        });
    });

    // Auto-resize textareas
    document.querySelectorAll('.jubo-textarea').forEach(ta => {
        ta.addEventListener('input', autoResize);
        autoResize.call(ta);
    });

    function autoResize() {
        this.style.height = 'auto';
        this.style.height = Math.max(100, this.scrollHeight) + 'px';
    }
});

// ─── Save Single Block (AJAX) ───────────────────────────────────────────────
function saveBlock(btn) {
    const item     = btn.closest('.jubo-block-item');
    const body     = item.querySelector('.jubo-block-body');
    const juboId   = parseInt(btn.dataset.juboId);
    const blockId  = parseInt(btn.dataset.blockId || '0');
    const blockType = btn.dataset.blockType;
    const sortOrder = parseInt(btn.dataset.sortOrder || '0');

    const titleInput   = body.querySelector('.jubo-block-title-input');
    const contentInput = body.querySelector('.jubo-block-content-input');
    const visToggle    = body.querySelector('.jubo-visibility-input');

    const payload = {
        id:           blockId,
        juboId:       juboId,
        blockType:    blockType,
        blockTitle:   titleInput   ? titleInput.value   : '',
        blockContent: contentInput ? contentInput.value : '',
        sortOrder:    sortOrder,
        isVisible:    visToggle    ? visToggle.checked  : true
    };

    btn.classList.add('saving');
    btn.textContent = '저장 중...';

    fetch('/Jubo/SaveBlock', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: JSON.stringify(payload)
    })
    .then(r => r.json())
    .then(data => {
        btn.classList.remove('saving');
        if (data.success) {
            btn.classList.add('saved');
            btn.textContent = '✓ 저장됨';
            // Update modified info
            const modInfo = item.querySelector('.jubo-block-modified');
            if (modInfo && data.modifiedBy) {
                modInfo.textContent = `${data.modifiedBy} · ${data.modifiedAt}`;
            }
            JuboToast.show('💾 저장되었어요!', 'success');
            setTimeout(() => {
                btn.classList.remove('saved');
                btn.textContent = '💾 저장';
            }, 3000);
        } else {
            btn.textContent = '💾 저장';
            JuboToast.show('저장 실패. 다시 시도해주세요.', 'error');
        }
    })
    .catch(() => {
        btn.classList.remove('saving');
        btn.textContent = '💾 저장';
        JuboToast.show('오류가 발생했습니다.', 'error');
    });
}

// ─── Save Meta (AJAX) ───────────────────────────────────────────────────────
function saveMeta(juboId) {
    const title        = document.getElementById('metaTitle')?.value?.trim();
    const season       = document.getElementById('metaSeason')?.value?.trim();
    const templateType = document.getElementById('metaTemplate')?.value;

    if (!title) {
        JuboToast.show('제목을 입력해주세요.', 'error');
        return;
    }

    const fd = new FormData();
    fd.append('id', juboId);
    fd.append('title', title);
    if (season) fd.append('season', season);
    fd.append('templateType', templateType || 'Normal');

    fetch('/Jubo/SaveMeta', {
        method: 'POST',
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        body: fd
    })
    .then(r => r.json())
    .then(data => {
        if (data.success) {
            JuboToast.show('✅ 기본 정보가 저장되었어요!', 'success');
            const titleDisp = document.getElementById('editPageTitle');
            if (titleDisp) titleDisp.textContent = title;
        } else {
            JuboToast.show('저장 실패.', 'error');
        }
    })
    .catch(() => JuboToast.show('오류가 발생했습니다.', 'error'));
}

// ─── Toggle Visibility ───────────────────────────────────────────────────────
function toggleVisibility(checkbox, blockType) {
    const item = checkbox.closest('.jubo-block-item');
    const label = checkbox.closest('.jubo-visibility-toggle')?.querySelector('.vis-label');
    if (label) {
        label.textContent = checkbox.checked ? '👁 표시됨' : '🙈 숨김';
    }
}

// ─── Attendance ──────────────────────────────────────────────────────────────
function addClassRow(juboDate) {
    const container = document.getElementById('classAttendContainer');
    if (!container) return;

    const idx = container.querySelectorAll('.class-attend-card').length;
    const card = document.createElement('div');
    card.className = 'class-attend-card';
    card.dataset.id = '0';
    card.innerHTML = `
        <div style="flex:1; min-width:130px;">
            <input type="text" class="jubo-input class-name-input" placeholder="반 이름 (예: 초등 1반)"
                   id="cn_${idx}" style="font-weight:700;">
        </div>
        <div class="class-count-inputs">
            <div class="count-input-group">
                <span class="count-input-label">총원</span>
                <input type="number" class="count-input total-input" value="0" min="0" id="ct_${idx}">
            </div>
            <div class="count-input-group">
                <span class="count-input-label">출석</span>
                <input type="number" class="count-input present-input" value="0" min="0" id="cp_${idx}">
            </div>
            <div class="count-input-group">
                <span class="count-input-label">새친구</span>
                <input type="number" class="count-input newcomer-input" value="0" min="0" id="cw_${idx}">
            </div>
        </div>
        <div style="flex:1; min-width:100px;">
            <input type="text" class="jubo-input" placeholder="메모" id="cn_note_${idx}" style="font-size:0.82rem;">
        </div>
        <button type="button" class="btn-jubo btn-jubo-save"
                onclick="saveClassRow(this, '${juboDate}')">💾 저장</button>
        <button type="button" class="btn-jubo btn-jubo-danger"
                onclick="removeClassRow(this)">🗑</button>
    `;
    container.appendChild(card);
}

function saveClassRow(btn, juboDate) {
    const card = btn.closest('.class-attend-card');
    const id   = parseInt(card.dataset.id || '0');
    const name = card.querySelector('.class-name-input')?.value?.trim();

    if (!name) {
        JuboToast.show('반 이름을 입력해주세요.', 'error');
        return;
    }

    const payload = {
        id:            id,
        juboDate:      juboDate,
        className:     name,
        totalCount:    parseInt(card.querySelector('.total-input')?.value   || '0'),
        presentCount:  parseInt(card.querySelector('.present-input')?.value || '0'),
        newComerCount: parseInt(card.querySelector('.newcomer-input')?.value|| '0'),
        note:          card.querySelector('[placeholder="메모"]')?.value || null
    };

    btn.textContent = '저장 중...';
    fetch('/Jubo/SaveAttendanceClass', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'X-Requested-With': 'XMLHttpRequest' },
        body: JSON.stringify(payload)
    })
    .then(r => r.json())
    .then(data => {
        if (data.success) {
            btn.textContent = '✓ 저장됨';
            JuboToast.show('출석이 저장되었어요! 🎉', 'success');
            updateAttendSummary();
            setTimeout(() => { btn.textContent = '💾 저장'; }, 2500);
        } else {
            btn.textContent = '💾 저장';
            JuboToast.show('저장 실패. 잠금 상태일 수 있어요.', 'error');
        }
    })
    .catch(() => {
        btn.textContent = '💾 저장';
        JuboToast.show('오류가 발생했습니다.', 'error');
    });
}

function removeClassRow(btn) {
    const card = btn.closest('.class-attend-card');
    const id   = parseInt(card.dataset.id || '0');

    if (id > 0) {
        if (!confirm('이 출석 기록을 삭제할까요?')) return;
        fetch(`/Jubo/DeleteAttendanceClass?id=${id}`, {
            method: 'POST',
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
        .then(r => r.json())
        .then(data => {
            if (data.success) {
                card.remove();
                JuboToast.show('삭제되었습니다.', 'success');
                updateAttendSummary();
            } else {
                JuboToast.show('잠금된 기록은 삭제할 수 없어요.', 'error');
            }
        });
    } else {
        card.remove();
    }
}

function updateAttendSummary() {
    let total = 0, present = 0, newcomer = 0;
    document.querySelectorAll('.class-attend-card').forEach(card => {
        total    += parseInt(card.querySelector('.total-input')?.value   || '0');
        present  += parseInt(card.querySelector('.present-input')?.value || '0');
        newcomer += parseInt(card.querySelector('.newcomer-input')?.value|| '0');
    });
    const elTotal    = document.getElementById('sumTotal');
    const elPresent  = document.getElementById('sumPresent');
    const elAbsent   = document.getElementById('sumAbsent');
    const elNewcomer = document.getElementById('sumNewcomer');
    if (elTotal)    elTotal.textContent    = total;
    if (elPresent)  elPresent.textContent  = present;
    if (elAbsent)   elAbsent.textContent   = total - present;
    if (elNewcomer) elNewcomer.textContent = newcomer;
}

// ─── New Comer ───────────────────────────────────────────────────────────────
function addNewComer(attendDate) {
    const name   = document.getElementById('nc_name')?.value?.trim();
    if (!name) { JuboToast.show('이름을 입력해주세요.', 'error'); return; }

    const payload = {
        attendanceDate: attendDate,
        name:        name,
        age:         parseInt(document.getElementById('nc_age')?.value || '0') || null,
        grade:       document.getElementById('nc_grade')?.value?.trim()   || null,
        parentName:  document.getElementById('nc_parent')?.value?.trim()  || null,
        phoneNumber: document.getElementById('nc_phone')?.value?.trim()   || null,
        address:     document.getElementById('nc_address')?.value?.trim() || null,
        note:        document.getElementById('nc_note')?.value?.trim()    || null
    };

    fetch('/Jubo/SaveNewComer', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'X-Requested-With': 'XMLHttpRequest' },
        body: JSON.stringify(payload)
    })
    .then(r => r.json())
    .then(data => {
        if (data.success) {
            JuboToast.show('새친구가 등록되었어요! 🌟', 'success');
            appendNewComerCard(data.id, payload);
            clearNewComerForm();
        } else {
            JuboToast.show('등록 실패. 다시 시도해주세요.', 'error');
        }
    })
    .catch(() => JuboToast.show('오류가 발생했습니다.', 'error'));
}

function appendNewComerCard(id, data) {
    const list = document.getElementById('newComerList');
    if (!list) return;
    const card = document.createElement('div');
    card.className = 'newcomer-card';
    card.dataset.id = id;
    card.innerHTML = `
        <div style="flex:1;">
            <div class="newcomer-name">🌟 ${escHtml(data.name)}</div>
            <div class="newcomer-info">
                ${data.age ? data.age + '세' : ''}
                ${data.grade ? ' · ' + escHtml(data.grade) : ''}
                ${data.parentName ? ' · 학부모: ' + escHtml(data.parentName) : ''}
                ${data.phoneNumber ? ' · ' + escHtml(data.phoneNumber) : ''}
            </div>
        </div>
        <button type="button" class="btn-jubo btn-jubo-danger"
                onclick="deleteNewComer(this, ${id})">🗑 삭제</button>
    `;
    list.appendChild(card);
    // Update newcomer count
    const nc = list.querySelectorAll('.newcomer-card').length;
    const badge = document.getElementById('newComerCount');
    if (badge) badge.textContent = nc;
}

function deleteNewComer(btn, id) {
    if (!confirm('새친구 기록을 삭제할까요?')) return;
    fetch(`/Jubo/DeleteNewComer?id=${id}`, {
        method: 'POST',
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
    })
    .then(r => r.json())
    .then(data => {
        if (data.success) {
            btn.closest('.newcomer-card').remove();
            JuboToast.show('삭제되었습니다.', 'success');
            const list  = document.getElementById('newComerList');
            const badge = document.getElementById('newComerCount');
            if (badge && list) badge.textContent = list.querySelectorAll('.newcomer-card').length;
        }
    });
}

function clearNewComerForm() {
    ['nc_name','nc_age','nc_grade','nc_parent','nc_phone','nc_address','nc_note']
        .forEach(id => {
            const el = document.getElementById(id);
            if (el) el.value = '';
        });
}

// ─── Delete Jubo ─────────────────────────────────────────────────────────────
function confirmDelete(id, title) {
    if (confirm(`"${title}" 주보를 삭제할까요?\n이 작업은 되돌릴 수 없어요.`)) {
        document.getElementById('deleteForm_' + id)?.submit();
    }
}

// ─── Print ────────────────────────────────────────────────────────────────────
function printJubo() {
    window.print();
}

// ─── Create Snapshot ─────────────────────────────────────────────────────────
function createSnapshot(juboId) {
    fetch('/Jubo/CreateSnapshot?id=' + juboId, {
        method: 'POST',
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
    })
    .then(r => r.json())
    .then(data => {
        if (data.success) JuboToast.show('📸 스냅샷이 저장되었어요!', 'success');
    });
}

// ─── Helpers ──────────────────────────────────────────────────────────────────
function escHtml(str) {
    if (!str) return '';
    return str.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}
