document.addEventListener("DOMContentLoaded", function () {

  function postJson(url, payload) {
    return fetch(url, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload || {})
    }).then(function (res) {
      if (!res.ok) throw new Error("HTTP " + res.status);
      return res.json().catch(function () { return {}; });
    });
  }

  function initGlobalCookieSection(sectionEl) {
    // ✅ 이미 초기화했으면 재실행 방지
    if (sectionEl.dataset.gcInited === "1") return;
    sectionEl.dataset.gcInited = "1";

    var cookieKey = sectionEl.getAttribute("data-cookie-key");
    if (!cookieKey) {
      console.warn("data-cookie-key 없음", sectionEl);
      return;
    }

    console.log("[GC] init:", cookieKey);

    postJson("/GlobalCookie/ShouldShow", { cookieKey: cookieKey })
      .then(function (data) {
        console.log("[GC] ShouldShow:", cookieKey, data);

        // 명시적으로 false일 때만 숨김
        if (data && data.shouldShow === false) {
          sectionEl.style.display = "none";
        } else {
          sectionEl.style.display = "";
        }
      })
      .catch(function (err) {
        console.error("[GC] ShouldShow ERROR:", cookieKey, err);
        // ✅ 실패해도 보여주기(랜덤 미표시 방지)
        sectionEl.style.display = "";
      });

    var hideButtons = sectionEl.querySelectorAll(".js-global-cookie-hide-btn");
    hideButtons.forEach(function (btn) {
      btn.addEventListener("click", function () {
        var duration = this.getAttribute("data-hide");

        postJson("/GlobalCookie/Hide", {
          cookieKey: cookieKey,
          durationCode: duration
        }).catch(function (err) {
          console.error("[GC] Hide ERROR:", cookieKey, err);
        });

        sectionEl.style.display = "none";
      });
    });
  }

  // ✅ 1) 현재 DOM에 있는 섹션 초기화
  function initAll() {
    document.querySelectorAll(".js-global-cookie-section").forEach(initGlobalCookieSection);
  }
  initAll();

  // ✅ 2) 나중에 DOM에 추가되는 섹션도 자동 초기화 (Partial/AJAX 대비)
  var observer = new MutationObserver(function (mutations) {
    mutations.forEach(function (m) {
      m.addedNodes && m.addedNodes.forEach(function (node) {
        if (!(node instanceof Element)) return;

        // node 자체가 대상이거나, 내부에 대상이 있으면 init
        if (node.matches && node.matches(".js-global-cookie-section")) {
          initGlobalCookieSection(node);
        } else if (node.querySelectorAll) {
          node.querySelectorAll(".js-global-cookie-section").forEach(initGlobalCookieSection);
        }
      });
    });
  });

  observer.observe(document.body, { childList: true, subtree: true });

});
