 
        function toggleRead(btn, VerseId, UserId) {
            // 중복 방지: 버튼 비활성화
            btn.disabled = true;

            // 메시지 박스 보이기
            const box = document.getElementById('processingBox');
            box.style.display = 'block';

            var baseUrl = "/DailyMission/ReadDOneUnDone";

            var fullURL = `${baseUrl}?VerseId=${encodeURIComponent(VerseId)}&UserId=${encodeURIComponent(UserId)}`

            fetch(fullURL, {
                method: 'GET'
            })

                .then(data => {
                    box.style.display = 'none';

                    const card = btn.closest('.bible-card');
                    const isNowRead = card.classList.toggle('read');

                    const verseLine = card.querySelector('.bible-line');

                    if (isNowRead) {
                        if (!verseLine.innerHTML.includes('읽음')) {
                            //verseLine.innerHTML += '(읽음😄)';
                            btn.innerHTML = '📖';
                        }
                    } else {
                        //verseLine.innerHTML = verseLine.innerHTML.replace(/\s*\(읽음😄\)/g, '');
                        btn.innerHTML = '📗';
                    }

                    btn.disabled = false; // 버튼 다시 활성화
                })
                .catch(error => {
                    console.error('에러:', error);
                    alert(error)
                    //div.textContent = `에러 ${error}`
                })
                .finally(() => {
                    //div.classList.remove('pressing');
                });
        }


        function addBookmark(btn, VerseId, UserId) {

            btn.disabled = true;

            // 메시지 박스 보이기
            const box = document.getElementById('processingBox');
            box.style.display = 'block';

            var baseUrl = "/DailyMission/AddBookMark";

            var fullURL = `${baseUrl}?VerseId=${encodeURIComponent(VerseId)}&UserId=${encodeURIComponent(UserId)}`

            fetch(fullURL, {
                method: 'GET'
            })
                .then(response => response.json())
                .then(data => {
                    box.style.display = 'none';

                    btn.innerHTML = '📚';
                    btn.title = '북마크로 이동';
                    btn.onclick = function () {
                        location.href = `/bibleCenter/bibleBookMarkDetail?VerseId=${VerseId}`;
                    };

                    btn.disabled = false; // 버튼 다시 활성화
                })
                .catch(error => {
                    console.error('에러:', error);
                    alert(error)
                    //div.textContent = `에러 ${error}`
                })
                .finally(() => {
                    //div.classList.remove('pressing');
                });
        }

        function goToBookmark(btn) {
            const id = btn.closest('.bible-card').dataset.id;
            alert("🔖 북마크 이동: " + id);
        }
    
  