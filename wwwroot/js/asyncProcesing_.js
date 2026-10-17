    (() => {
      
        document.querySelectorAll('.pressable').forEach(div => {
            let timer = null;
            let isTouchMoving = false;

            const start = e => {
                isTouchMoving = false;

                div.classList.add('pressing');
                timer = setTimeout(() => {
                    if (!isTouchMoving) {
                        trigger(div);
                    }
                }, 1000);
            };

            const cancel = () => {
                clearTimeout(timer);
                div.classList.remove('pressing');
            };

            const onTouchMove = () => {
                isTouchMoving = true;  // 스크롤 시작됨 → 롱프레스 취소
                cancel();
            };

            // 마우스 이벤트
            div.addEventListener('mousedown', start);
            div.addEventListener('mouseup', cancel);
            div.addEventListener('mouseleave', cancel);

            // 터치 이벤트 (스크롤 대응 포함)
            div.addEventListener('touchstart', start);
            div.addEventListener('touchend', cancel);
            div.addEventListener('touchmove', onTouchMove); // 👈 스크롤 감지용
        });

        function trigger(div) {
            const VerseId = div.getAttribute('data-id');
            const UserId = div.getAttribute('data-userID');
            const baseUrl = div.getAttribute('data-url'); // ✅ 동적 URL
            if (!VerseId) {
                div.textContent = 'ID 없음';
                return;
            }

            div.textContent = '처리중 입니다...';
            var fullURL = `${baseUrl}?VerseId=${encodeURIComponent(VerseId)}&UserId=${encodeURIComponent(UserId)}`
 
            fetch(fullURL, {
                method: 'GET'
            })
                .then(response => response.json())
                .then(data => {
                    div.textContent = data.success ? data.value : '실패';
                })
                .catch(error => {
                    console.error('에러:', error);
                    div.textContent = '에러';
                })
                .finally(() => {
                    div.classList.remove('pressing');
                });
        }
    })();

