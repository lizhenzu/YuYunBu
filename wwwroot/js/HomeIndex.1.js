 
    a {
        text-decoration: none;
        color: inherit;
    }

    header {
        background: radial-gradient(circle at top left, #5bbf8c 0, #2f8f6a 40%, #5bbf8c 100%);
        color: #fff;
        padding: 1.5rem 1.2rem 1.8rem;
        /*   position: sticky; */
        top: 0;
        z-index: 20;
        box-shadow: 0 4px 14px rgba(8, 38, 20, 0.25);
    }


    .header-inner {
        max-width: 1100px;
        margin: 0 auto;
        display: flex;
        flex-direction: column; /* ⭐ 세로 정렬 */
        align-items: center; /* ⭐ 가운데 정렬 */
        justify-content: center;
        text-align: center;
        gap: 1rem;
    }


    .logo-circle {
        width: 56px;
        height: 56px;
        border-radius: 50%;
        background: rgba(255, 255, 255, 0.2);
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 1.8rem;
        flex-shrink: 0;
    }

    .header-text {
        text-align: center; /* 텍스트 가운데 */
        display: flex;
        flex-direction: column; /* 요소 세로 배치 */
        align-items: center; /* ⭐ 이미지(아바타) 포함 모두 가운데 */
        justify-content: center;
    }

        .header-text h1 h5 {
            font-size: 1.6rem;
            font-weight: 800;
            letter-spacing: 0.02em;
            margin-bottom: 0.2rem;
            text-align: center;
            align-items: center;
            justify-content: center;
        }

        .header-text p {
            font-size: 0.9rem;
            opacity: 0.95;
            text-align: center;
            align-items: center;
            justify-content: center;
        }

    .tagline {
        margin-top: 0.4rem;
        display: inline-flex;
        align-items: center;
        gap: 0.35rem;
        padding: 0.25rem 0.7rem;
        background: rgba(255, 255, 255, 0.2);
        border-radius: 999px;
        font-size: 0.78rem;
    }

    .main-wrapper {
        max-width: 1100px;
        margin: 1.5rem auto 3rem;
        padding: 0 1rem 1rem;
    }

    /* Quick nav */
    .quick-nav-wrapper {
        margin-bottom: 1.5rem;
    }

    .quick-nav-title {
        font-size: 0.9rem;
        font-weight: 700;
        margin-bottom: 0.4rem;
        color: var(--text-soft);
    }

    .quick-nav {
        display: flex;
        gap: 0.4rem;
        overflow-x: auto;
        padding: 0.5rem 0.1rem 0.3rem;
        scrollbar-width: thin;
    }

        .quick-nav::-webkit-scrollbar {
            height: 5px;
        }

        .quick-nav::-webkit-scrollbar-thumb {
            background: rgba(0, 0, 0, 0.15);
            border-radius: 999px;
        }

    .quick-nav-btn {
        flex: 0 0 auto;
        padding: 0.45rem 0.75rem;
        font-size: 1.08rem;
        border-radius: 999px;
        border: none;
        background: #ffffff;
        box-shadow: 0 3px 8px rgba(0, 0, 0, 0.05);
        display: inline-flex;
        align-items: center;
        gap: 0.25rem;
        cursor: pointer;
        white-space: nowrap;
    }

        .quick-nav-btn span.emoji {
            font-size: 1rem;
        }

        .quick-nav-btn:hover {
            background: var(--primary-soft);
        }

    /* Sections & cards */
    section {
        margin-bottom: 1.8rem;
    }

    .section-label {
        font-size: 0.78rem;
        text-transform: uppercase;
        letter-spacing: 0.08em;
        color: var(--text-soft);
        margin-bottom: 0.15rem;
    }

    h1.section-title {
        font-size: 1.45rem;
        margin-bottom: 0.6rem;
        display: flex;
        align-items: center;
        gap: 0.4rem;
    }

    h2.section-title {
        font-size: 1.15rem;
        margin-bottom: 0.6rem;
        display: flex;
        align-items: center;
        gap: 0.4rem;
    }

    h1 .title-tag {
        font-size: 0.75rem;
        padding: 0.1rem 0.6rem;
        border-radius: 999px;
        background: var(--primary-soft);
        color: #1f2933;
    }

    h2 .title-tag {
        font-size: 0.75rem;
        padding: 0.1rem 0.6rem;
        border-radius: 999px;
        background: var(--primary-soft);
        color: #1f2933;
    }

    .section-description {
        font-size: 0.85rem;
        color: var(--text-soft);
        margin-bottom: 0.75rem;
    }

    .card-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(230px, 1fr));
        gap: 0.85rem;
    }

    .card {
        background: var(--card-bg);
        border-radius: var(--radius-lg);
        padding: 0.9rem 0.95rem 0.95rem;
        box-shadow: var(--shadow-soft);
        position: relative;
        overflow: hidden;
    }

        .card::before {
            content: "";
            position: absolute;
            inset: 0;
            background: radial-gradient(circle at top right, rgba(255, 255, 255, 0.8), transparent 55%);
            opacity: 0.7;
            pointer-events: none;
        }

    .card-header {
        display: flex;
        align-items: center;
        gap: 0.6rem;
        margin-bottom: 0.35rem;
    }

    .card-icon {
        width: 32px;
        height: 32px;
        border-radius: 13px;
        background: linear-gradient(135deg, #d9f3ea, #b8e4ce);
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 1.2rem;
        flex-shrink: 0;
    }


    .card-title {
        font-size: 0.98rem;
        font-weight: 700;
    }

    .card-text {
        font-size: 0.83rem;
        color: var(--text-soft);
        margin-bottom: 0.6rem;
    }

    .badge-row {
        display: flex;
        flex-wrap: wrap;
        gap: 0.3rem;
        margin-bottom: 0.55rem;
    }

    .badge {
        font-size: 0.73rem;
        padding: 0.15rem 0.5rem;
        border-radius: 999px;
        background: #e4f3eb; /* 연한 그린 */
        color: #355343;
    }

        .badge.badge-hero {
            background: #ffe9f1;
            color: #c14c7b;
        }

    .card-actions {
        display: flex;
        flex-wrap: wrap;
        gap: 0.4rem;
    }

    .btn {
        border-radius: 999px;
        border: none;
        font-size: 0.78rem;
        padding: 0.35rem 0.65rem;
        cursor: pointer;
        display: inline-flex;
        align-items: center;
        gap: 0.25rem;
        background: #f3f4ff;
    }

    .btn-primary {
        background: var(--primary);
        color: #fff;
    }

    .btn-outline {
        background: transparent;
        border: 1px solid rgba(99, 115, 255, 0.4);
    }

    .btn-soft-green {
        background: var(--accent-green);
        color: #fff;
    }


    .btn-soft-pink {
        background: var(--accent-pink);
        color: #fff;
    }

    .btn span.emoji {
        font-size: 1rem;
    }

    .notice-highlight,
    .board-highlight {
        background: #fff4d9; /* 따뜻한 하이라이트 배경 */
        border-left: 6px solid #ffb347; /* 강조 표시 */
        padding: 0.7rem 1rem;
        margin: 0.5rem 0 0.8rem;
        font-size: 0.88rem;
        font-weight: 700;
        color: #1f4633;
        border-radius: 10px;
        box-shadow: 0 2px 8px rgba(0,0,0,0.05);
    }

    .my-message-highlight {
        background: #fff4d9; /* 따뜻한 하이라이트 배경 */
        border-left: 6px solid #ffb347; /* 강조 표시 */
        padding: 0.8rem 1rem;
        margin: 0.8rem 0 1rem;
        font-size: 0.9rem;
        font-weight: 700;
        color: #7a4b00; /* 눈에 띄는 강조 텍스트 */
        border-radius: 10px;
        box-shadow: 0 3px 10px rgba(0,0,0,0.06);
    }

    footer {
        margin-top: 2rem;
        padding: 1.5rem 1rem 2rem;
        background: #e9f5ef; /* 연한 힐링 그린 */
        border-top: 1px solid #cde5d8;
        font-size: 0.8rem;
        color: var(--text-soft);
    }

    .footer-inner {
        max-width: 1100px;
        margin: 0 auto;
        display: flex;
        flex-direction: column;
        gap: 0.3rem;
    }

    .footer-shortcuts {
        display: flex;
        flex-wrap: wrap;
        gap: 0.3rem;
        margin-top: 0.3rem;
    }

    .footer-chip {
        padding: 0.2rem 0.55rem;
        border-radius: 999px;
        border: 1px dashed rgba(102, 126, 255, 0.5);
        font-size: 0.75rem;
        cursor: pointer;
    }

    .top-btn {
        position: fixed;
        right: 1rem;
        bottom: 1rem;
        border-radius: 999px;
        border: none;
        padding: 0.45rem 0.7rem;
        font-size: 0.8rem;
        background: #ffffff;
        box-shadow: var(--shadow-soft);
        cursor: pointer;
        display: flex;
        align-items: center;
        gap: 0.2rem;
        opacity: 0.95;
    }

    @@media (min-width: 768px) {
        header {
            padding: 1.6rem 1.5rem 1.9rem;
        }

        .header-text h1 {
            font-size: 1.9rem;
        }

        .header-text p {
            font-size: 1rem;
        }

        .main-wrapper {
            padding: 0 1.5rem 2rem;
        }
    }

    .mission-status-box {
        background: #e6f6f0; /* 힐링 초록 배경 */
        border: 1px solid #cde7db;
        padding: 1rem;
        border-radius: 14px;
        margin: 0.8rem 0 1.2rem;
        box-shadow: 0 3px 10px rgba(0,0,0,0.04);
    }

    .mission-item {
        padding: 0.45rem 0.7rem;
        margin-bottom: 0.4rem;
        border-radius: 8px;
        font-size: 0.88rem;
        font-weight: 600;
        display: flex;
        align-items: center;
        gap: 0.4rem;
    }

        .mission-item:last-child {
            margin-bottom: 0;
        }

        /* 완료 */
        .mission-item.done {
            background: #d0f3db; /* 부드러운 초록 */
            color: #236b3d;
            border-left: 6px solid #2f8f6a;
        }

        /* 미완료 */
        .mission-item.not-done {
            background: #fff3e6; /* 연한 오렌지 */
            color: #a15a0a;
            border-left: 6px solid #ffb347;
        }


    .bible-progress-box {
        background: #e6f6f0; /* 힐링 초록 배경 */
        border: 1px solid #cde7db;
        padding: 0.9rem 1rem;
        border-radius: 14px;
        margin: 0.8rem 0 1.2rem;
        box-shadow: 0 3px 10px rgba(0, 0, 0, 0.04);
        font-size: 0.9rem;
        color: #264735;
    }

    .bible-progress-header {
        margin-bottom: 0.6rem;
        font-weight: 700;
    }

    .bible-progress-bar-wrap {
        display: flex;
        align-items: center;
        gap: 0.6rem;
    }

    .bible-progress-bar {
        flex: 1;
        height: 10px;
        border-radius: 999px;
        background: #d5e8dd;
        overflow: hidden;
    }

    .bible-progress-fill {
        height: 100%;
        border-radius: 999px;
        background: linear-gradient(90deg, #34b38a, #2f8f6a);
        transition: width 0.4s ease;
    }

    .bible-progress-percent {
        min-width: 2.5rem;
        text-align: right;
        font-size: 0.85rem;
        font-weight: 700;
        color: #246247;
    }



    .profile-avatar {
        width: 120px;
        height: 120px;
        border-radius: 50%;
        overflow: hidden;
        border: 3px solid rgba(255, 255, 255, 0.85);
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.18);
        flex-shrink: 0;
        display: flex;
        align-items: center;
        justify-content: center;
    }

        .profile-avatar img {
            width: 100%;
            height: 100%;
            object-fit: cover;
            display: block;
        }


    .card-header-text {
        display: flex;
        gap: 0.15rem;
    }

    .mission-status-badge {
        display: inline-block;
        font-size: 0.75rem;
        padding: 0.1rem 0.55rem;
        border-radius: 999px;
        font-weight: 600;
    }

        /* 완료 (말씀 읽기) */
        .mission-status-badge.done {
            background: #d0f3db;
            color: #236b3d;
            border: 1px solid #2f8f6a;
        }

        /* 미완료 (퀴즈/암송/기도) */
        .mission-status-badge.not-done {
            background: #fff3e6;
            color: #a15a0a;
            border: 1px solid #ffb347;
        }

        .mission-status-badge.fail {
            background: #ffe4e4; /* 연한 빨간 배경 */
            color: #b3261e; /* 진한 빨간 글자 */
            border: 1px solid #ff6b6b; /* 살짝 강한 빨간 테두리 */
        }

    @@media (max-width: 640px) {
        /* header 전체를 세로 정렬 */
        .header-inner {
            flex-direction: column;
            text-align: center;
            gap: 1rem;
        }
        /* 프로필 박스를 세로 3단처럼 */
        .profile-box {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 0.35rem;
            width: 100%;
        }

        .profile-avatar {
            align-items: center;
            width: 120px;
            height: 120px;
        }

        .profile-caption {
            font-size: 0.75rem;
            margin-top: 0.2rem;
            opacity: 0.9;
        }
    }

    .mission-total-point {
        margin: 0.7rem 0 1.2rem;
        padding: 0.75rem 1rem;
        border-radius: 999px;
        background: linear-gradient(90deg, #ffe9f1, #fff4d9);
        display: inline-flex;
        align-items: center;
        gap: 0.4rem;
        font-size: 0.9rem;
        font-weight: 700;
        color: #7a3b00;
        box-shadow: 0 3px 10px rgba(0,0,0,0.06);
    }

        .mission-total-point .emoji {
            font-size: 1.1rem;
        }


    .section-title.clickable {
        cursor: pointer;
        position: relative;
        transition: color .25s ease;
    }

        .section-title.clickable:hover {
            color: #1d5e46; /* 살짝 더 진한 그린톤 */
        }

        .section-title.clickable::after {
            content: "➡️";
            font-size: 1.1rem;
            margin-left: 0.45rem;
            opacity: 0.8;
            transition: transform .25s ease, opacity .25s ease;
        }

        .section-title.clickable:hover::after {
            transform: translateX(4px);
            opacity: 1;
        }

    <
    @@media (max-width: 640px) {
        .quick-nav

    {
        flex-wrap: wrap; /* 여러 줄로 줄바꿈 */
        overflow-x: visible; /* 가로 스크롤 제거 */
    }

    .quick-nav-btn {
        flex: 0 0 auto; /* ⭐ 텍스트 길이만큼만 너비 */
        padding: 0.55rem; /* ⭐ 여백 확 줄이기 */
        font-size: 1.08rem; /* 필요하면 0.75로 더 줄여도됨 */
    }

        .quick-nav-btn .emoji {
            margin-right: 0.15rem; /* 아이콘과 글자 간격 살짝만 */
        }

    }

    /* 전체 텍스트 선택 방지 (PC + 모바일) */
    html, body {
        -webkit-user-select: none; /* 크롬/사파리/모바일 */
        -moz-user-select: none; /* 파이어폭스 */
        -ms-user-select: none; /* IE/엣지 구버전 */
        user-select: none; /* 표준 */
        -webkit-touch-callout: none; /* iOS 롱클릭 메뉴 방지 */
    }

    /* 혹시 입력창에서는 선택 가능하게 하고 싶으면 이렇게 예외 처리 */
    input, textarea, [contenteditable="true"] {
        -webkit-user-select: text !important;
        -moz-user-select: text !important;
        -ms-user-select: text !important;
        user-select: text !important;
        -webkit-touch-callout: default !important;
    }


    .notice-inline {
        margin: 0.45rem 0 0.7rem;
        padding: 0.45rem 0.6rem;
        border-radius: 10px;
        background: #fff7e6; /* 카드 안이라 조금 더 부드럽게 */
        border: 1px dashed #ffb347; /* 공지 느낌 나는 점선 테두리 */
        font-size: 0.9rem;
        color: #7a4b00;
        display: flex;
        font-weight: 700;
        align-items: flex-start;
        gap: 0.35rem;
    }

        .notice-inline .label {
            font-weight: 700;
            margin-right: 0.15rem;
        }

        .notice-inline .text {
            flex: 1;
            word-break: break-word;
        }



    /* 홈 상단 요약 박스 */
    .home-summary {
        margin: 0.5rem 0 1.5rem;
        padding: 0.9rem 1rem 1.1rem;
        border-radius: 18px;
        background: linear-gradient(135deg, #fff7e6, #fdf1ff);
        box-shadow: 0 6px 18px rgba(0,0,0,0.06);
        border: 1px solid rgba(255, 179, 71, 0.25);
    }

    .home-summary-header {
        display: flex;
        align-items: center;
        gap: 0.4rem;
        font-size: 0.95rem;
        font-weight: 800;
        color: #7a3b00;
        margin-bottom: 0.5rem;
    }

        .home-summary-header .emoji {
            font-size: 1.1rem;
        }

    .home-summary-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(210px, 1fr));
        gap: 0.55rem;
    }

    .home-summary-item {
        background: rgba(255, 255, 255, 0.92);
        border-radius: 14px;
        padding: 0.55rem 0.65rem 0.6rem;
        display: flex;
        flex-direction: column;
        gap: 0.25rem;
        border: 1px dashed rgba(255, 179, 71, 0.4);
        cursor: pointer;
        transition: transform .18s ease, box-shadow .18s ease, border-color .18s ease;
    }

        .home-summary-item:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 14px rgba(0,0,0,0.06);
            border-color: rgba(47, 143, 106, 0.55);
        }

    .home-summary-item-header {
        display: flex;
        align-items: center;
        gap: 0.35rem;
        font-size: 0.86rem;
        font-weight: 700;
        color: #5b3a00;
    }

        .home-summary-item-header .emoji {
            font-size: 1rem;
        }

    .home-summary-item-body {
        font-size: 0.8rem;
        color: #5c5c5c;
        line-height: 1.35;
        word-break: break-word;
    }

    .home-summary-pill {
        display: inline-flex;
        align-items: center;
        gap: 0.2rem;
        padding: 0.1rem 0.45rem;
        border-radius: 999px;
        font-size: 0.75rem;
        background: #e6f6f0;
        color: #236b3d;
        margin-top: 0.2rem;
    }

        .home-summary-pill span.emoji {
            font-size: 0.95rem;
        }

    @@media (max-width: 640px) {
        .home-summary {
            padding: 0.85rem 0.85rem 1rem;
        }
    }
