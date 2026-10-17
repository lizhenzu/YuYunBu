let player;

window.onYouTubeIframeAPIReady = function () {
    // 초기 생성은 생략 가능하거나, placeholder용 player 생성
};

function playVideo(videoId, loopStart, loopEnd) {
    document.getElementById("youtubeModal").style.display = "flex";

    if (player && typeof player.destroy === 'function') {
        player.destroy();
    }

    player = new YT.Player('youtubeFrame', {
        height: '360',
        width: '640',
        videoId: videoId,
        playerVars: {
            autoplay: 1,
            controls: 1,
            modestbranding: 1,
            start: loopStart,
            end: loopEnd
        },
        events: {
            'onStateChange': function (event) {
                onPlayerStateChange(event, loopStart, loopEnd);
            }
        }
    });
}

function onPlayerStateChange(event, loopStart, loopEnd) {
    if (event.data === YT.PlayerState.PLAYING) {
        const interval = setInterval(function () {
            if (player && player.getCurrentTime() >= loopEnd) {
                player.seekTo(loopStart);
            }
        }, 1000);
    }
}

function closeYoutubeModal() {
    if (player && typeof player.destroy === 'function') {
        player.destroy();
        player = null;
    }
    document.getElementById("youtubeModal").style.display = "none";
}

















// playVideoSequence 함수 (videos는 글로벌 변수로 참조)
function playVideoSequence() {
    document.getElementById("youtubeModal").style.display = "flex";

    let current = 0;

    function playNext() {
        if (player && typeof player.destroy === 'function') {
            player.destroy();
        }

        let vid = videos[current];
        player = new YT.Player('youtubeFrame', {
            height: '360',
            width: '640',
            videoId: vid.videoId,
            playerVars: {
                autoplay: 1,
                controls: 1,
                modestbranding: 1,
                start: vid.start,
                end: vid.end
            },
            events: {
                'onStateChange': function (event) {
                    if (event.data === YT.PlayerState.ENDED) {
                        current++;
                        if (current >= videos.length) {
                            current = 0; // 루프: 처음부터 다시
                        }
                        playNext();
                    }
                }
            }
        });
    }

    playNext(); // 최초 실행
}
 

 

    function playVideoSequence2() {
        document.getElementById("youtubeModal").style.display = "flex";

        let current = 0;

        function playNext() {
            // 현재 videoId가 없으면 → 처음부터 다시
            if (!videos[current].videoId) {
                current = 0;
            }

            let vid = videos[current];

            // 비디오 객체 파괴
            if (player && typeof player.destroy === 'function') {
                player.destroy();
            }

            player = new YT.Player('youtubeFrame', {
                height: '360',
                width: '640',
                videoId: vid.videoId,
                playerVars: {
                    autoplay: 1,
                    controls: 1,
                    modestbranding: 1,
                    start: vid.start,
                    end: vid.end
                },
                events: {
                    'onStateChange': function (event) {
                        if (event.data === YT.PlayerState.ENDED) {
                            current++;
                            // 다음 영상이 없거나 videoId가 없으면 처음부터
                            if (current >= videos.length || !videos[current].videoId) {
                                current = 0;
                            }
                            playNext();
                        }
                    }
                }
            });
        }

        playNext();
    }
 