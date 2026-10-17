        (function () {
            // 사용자가 '움직임 감소'를 선호하면 효과 생략
            const reduceMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
            function launchConfetti(durationMs = 3000) {
                if (reduceMotion || typeof confetti !== 'function') return;

                const end = Date.now() + durationMs;
                const defaults = { startVelocity: 35, spread: 360, ticks: 90, zIndex: 9999 };

                const timer = setInterval(function () {
                    const timeLeft = end - Date.now();
                    if (timeLeft <= 0) { clearInterval(timer); return; }
                    const particleCount = Math.max(20, 60 * (timeLeft / durationMs));

                    // 좌/우에서 동시에 발사
                    confetti({ ...defaults, particleCount, origin: { x: Math.random() * 0.2 + 0.1, y: Math.random() * 0.3 } });
                    confetti({ ...defaults, particleCount, origin: { x: Math.random() * 0.2 + 0.7, y: Math.random() * 0.3 } });
                }, 250);

                // 처음에 강한 한 방
                confetti({ ...defaults, particleCount: 200, origin: { y: 0.4 } });
            }

            // 페이지 로드 시 1회 자동 실행
            document.addEventListener('DOMContentLoaded', function () {
                launchConfetti(3000); // ← 지속시간(ms) 조정 가능
            });

            // 헤더 클릭 시 재생(옵션)
            const header = document.querySelector('header');
            if (header) {
                header.style.cursor = 'pointer';
                header.title = '클릭하면 축하 폭죽이 다시 터집니다!';
                header.addEventListener('click', () => launchConfetti(2000));
            }
        })();

