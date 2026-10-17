function showLoading() {
    // Show the loading overlay and apply animations
    document.getElementById('loadingOverlay').style.display = 'block';
    document.body.style.pointerEvents = 'none'; // Disable all inputs
}

function hideLoading() {
    // Hide the loading overlay and remove input blocking
    document.getElementById('loadingOverlay').style.display = 'none';
    document.body.style.pointerEvents = 'auto'; // Re-enable inputs
}
async function doProcess(data, apiName, reloadYN) {

    showLoading();

    const url = `${window.location.origin}/api/PrayFreind/${apiName}`;

    console.log("요청 URL:", url);
    console.log("보내는 데이터:", data);

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json; charset=utf-8'
            },
            body: JSON.stringify(data)
        });

        hideLoading();
        alert('-----------')

        if (!response.ok) {
            const errorData = await response.json();
            console.error("서버 응답 오류:", errorData);
            throw new Error(errorData.message || "서버 오류 발생");
        }

        const responseData = await response.json(); // ✅ 이름 변경

        console.log("서버 응답:", responseData);

        if (responseData.message === "Success") {

           if (String(reloadYN).trim().toLowerCase() === 'n') {
                // confirmMessage 생략 → 바로 새로고침             
                return;
            }
            let result = await confirmMessage(false, ['정상 처리 되었습니다.']);
            if (result === 'y') {
                // 작업 계속
            } else if (result === 'c') {
                // 작업 취소
            }
            alert('cation.reloa');
            location.reload();
        } else {
            alert(`오류 발생: ${responseData.message}`);
        }
    } catch (error) {
        hideLoading();
        console.error("네트워크 오류:", error);
        alert(`네트워크 오류: ${error.message}`);
    }
}

        async function sendToServer(bibleText,fromJang,fromZul,toJang,toZul,userId) {

            let result = await confirmMessage(true, ['이 페이지를 다 읽으셨을까요?']);

            const data = {
                BibleText: bibleText,
                FromJang: fromJang,
                FromZul: fromZul,
                ToJang: toJang,
                ToZul: toZul,
                Id: userId
            };

            if (result === 'y') {
                doProcess(data, 'InsertUserBibleReadRange');
            }
        }
        

        async function sendToServerUpdateUserSettings(data, confirmMessageArray) {
            let result = await confirmMessage(true,confirmMessageArray);
            if (result === 'y') {
                doProcess(data, 'UpdateUserSettings');
            }
        }
     async function sendToServerUpdateUserCookieSettings(data) {
            doProcess(data, 'UpsertUserCookieSettings');
        }
                                