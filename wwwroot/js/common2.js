function confirmMessage(showCancelButton, messageArray = ['정상적으로 처리되었습니다.'], imageUrl = null, confirmButtonText = '확인', cancelButtonText = '취소') {
    return new Promise((resolve, reject) => {
        var alertBox = document.createElement('div');
        alertBox.style.position = 'fixed';
        alertBox.style.left = '50%';
        alertBox.style.top = '50%';
        alertBox.style.transform = 'translate(-50%, -50%)';
        alertBox.style.padding = '20px';
        alertBox.style.backgroundColor = '#fff';
        alertBox.style.border = '1px solid #ccc';
        alertBox.style.borderRadius = '5px';
        alertBox.style.zIndex = '9999';
        alertBox.style.textAlign = 'center';
        alertBox.style.width = '80%'; // 알림 창 너비 지정

        // 이미지 추가 (imageUrl이 있는 경우)
        if (imageUrl) {
            var image = document.createElement('img');
            image.src = imageUrl;
            image.style.position = 'absolute';
            image.style.top = '70px'; // 팝업창 내 최상단
            image.style.left = '10px'; // 팝업창 내 최좌측
            image.style.width = '50px'; // 이미지 크기 설정
            image.style.height = 'auto'; // 이미지 크기 설정
            alertBox.appendChild(image);
        }

        // "알림" 메시지 추가
        var alertTitle = document.createElement('div');
        alertTitle.innerText = '알림';
        alertTitle.style.fontSize = '14px';
        alertTitle.style.color = '#fff';
        alertTitle.style.backgroundColor = '#007BFF'; // 배경 색상 지정
        alertTitle.style.padding = '10px';
        alertTitle.style.width = '100%'; // 좌우 100% 차게
        alertTitle.style.margin = '0px'; // 상단 여백 
        alertTitle.style.marginBottom = '23px'; // 하단 여백
        alertTitle.style.borderRadius = '5px';

         // 기존 alertBox의 textAlign을 제거
        alertBox.style.display = 'flex';
        alertBox.style.flexDirection = 'column'; // 세로로 요소 정렬
        alertBox.style.alignItems = 'center'; // 가로 정렬 가운데
        alertBox.style.justifyContent = 'center'; // 세로 정렬 가운데


        // 메세지 배열을 줄바꿈으로 추가
        var messageContainer = document.createElement('div');
        messageArray.forEach((msg, index) => {
            var message = document.createElement('p');
            message.innerText = `${msg}`;
            message.style.fontSize = '15px'; // 글씨 크기를 줄임
            message.style.fontWeight = 'bold';
            messageContainer.appendChild(message);
        });

        // 버튼들을 감쌀 컨테이너 추가 (가로 정렬을 위해)
        var buttonContainer = document.createElement('div');
        buttonContainer.style.display = 'flex';
        buttonContainer.style.justifyContent = 'center';
        buttonContainer.style.marginTop = '10px'; // 버튼 상단 간격 추가

        // 확인 버튼 추가
        var confirmButton = document.createElement('button');
        confirmButton.innerText = confirmButtonText; // 확인 버튼명 변경
        confirmButton.style.fontSize = '15px';
        confirmButton.style.padding =  '5px 20px 5px 20px';
        confirmButton.style.backgroundColor = '#4CAF50';
        confirmButton.style.color = '#fff';
        confirmButton.style.border = 'none';
        confirmButton.style.borderRadius = '5px';
        confirmButton.style.cursor = 'pointer';
        confirmButton.style.marginRight = '10px'; // 버튼 간격 추가

        // 취소 버튼 추가
        var cancelButton = document.createElement('button');
        cancelButton.innerText = cancelButtonText; // 취소 버튼명 변경
        cancelButton.style.padding = '5px 20px 5px 20px';
        cancelButton.style.backgroundColor = '#2e8b57' ; //'#f44336';
        cancelButton.style.color = '#fff';
        cancelButton.style.border = 'none';
        cancelButton.style.borderRadius = '5px';
        cancelButton.style.cursor = 'pointer';

        // 확인 버튼 클릭 시 'y' 반환
        confirmButton.onclick = function () {
            document.body.removeChild(alertBox);
            document.body.removeChild(overlay);
            resolve('y');
        };

        // 취소 버튼 클릭 시 'c' 반환
        cancelButton.onclick = function () {
            document.body.removeChild(alertBox);
            document.body.removeChild(overlay);
            resolve('c');
        };

        // 버튼을 컨테이너에 추가
        buttonContainer.appendChild(confirmButton);
        if (showCancelButton) {
            buttonContainer.appendChild(cancelButton); // 취소 버튼 추가
        }

        alertBox.appendChild(alertTitle); // 알림 메시지를 먼저 추가
        alertBox.appendChild(messageContainer); // 배열 메세지 추가
        alertBox.appendChild(buttonContainer); // 버튼 컨테이너 추가

        var overlay = document.createElement('div');
        overlay.style.position = 'fixed';
        overlay.style.top = '0';
        overlay.style.left = '0';
        overlay.style.width = '100%';
        overlay.style.height = '100%';
        overlay.style.backgroundColor = 'rgba(0, 0, 0, 0.9)';
        overlay.style.zIndex = '9998';

        document.body.appendChild(overlay);
        document.body.appendChild(alertBox);
    });
}

    function goBack() {
            window.history.back();
        }
        function redirectToPage(URL) {
            window.location.href = URL;
        }
        function goHome() {
            window.location.href = '/';
        }
