# 인수인계 프롬프트 — 잠실새내교회 유년부 사이트 · 최신 테마 (2026-10-08)

아래 전체를 새 창 첫 메시지로 붙여 넣으세요.

---

너는 잠실새내교회 유년부 사이트를 화면 하나씩 **최신 밝은 테마(DailyMission4 테마)** 로 바꾸는 작업을 이어서 한다. 아래 규칙과 테마를 그대로 따른다.

## 1. 프로젝트
- ASP.NET Core MVC (.NET 9), Razor Views, EF Core
- 경로: `C:\WorkSpace\yunbu\bulletin-board-master.3\bulletin-board-master\YunYunBu2025\YunYunBu2025`
- 빌드: `dotnet build -c Release` (Debug 산출물은 디스크가 부족하면 지워도 됨)
- 미리보기: `preview_start name=YunYunBu2025` (포트 5051, 뜨는 데 약 30초 → 기다린 뒤 navigate). 뷰를 고치면 서버를 멈추고 → 빌드 → 다시 시작
- PowerShell 5.1이 주 셸. Bash는 이 환경에서 자주 실패하니 파일 수정은 Edit 도구로 한다. `H`, `R`은 PowerShell 별칭이라 함수 이름으로 쓰지 않는다

## 2. 꼭 지킬 규칙
- 답은 **한국어**. **모든 답변 첫 줄에 "추천 모델: Opus/Sonnet — 이유 한 줄"** (작은 수정도)
  - Opus: 여러 파일·컨트롤러·서비스·DB까지 건드리는 설계 / 기획 판단이 많은 일
  - Sonnet: 화면 하나 디자인, 색 바꾸기, 시안 추가, 문구 수정, 정리 작업
- 허락 묻지 말고 진행 (CLAUDE.md). 다만 **디자인은 시안 먼저**: Artifact 시안 → 사용자가 "확정"/"○안으로" → 적용 → 빌드 → 게스트 확인 → 결과 보고
- 시간: `DateHelper.GetKoreaNow()` / `GetKoreaToday()`만. `DateTime.Now`, `DateTime.Today` 금지
- 글자 상수: "홈" 링크는 `@ConstatsCommon.ToHome` (값이 이미 "홈으로" — 뒤에 "으로"를 또 붙이지 말 것), "나의 도토리"는 `@ConstatsCommon.myDoToRi`
- 백업: 고치기 전 `파일.bak_before_작업이름` (같은 파일 두 번째면 이름을 다르게, 예: `_mint`, `_ogimage2`). 지운 파일은 `YunYunBu2025\Backup_removed_20261004`로
- 메뉴 삭제 요청이면 **뷰와 컨트롤러 액션만** 삭제
- 확인은 **게스트로만**. 로그인·비밀번호 입력 절대 금지. DB에 쓰는 버튼(저장·삭제·등록·보정·스티커 등)은 누르지 않는다. 관리자 화면은 "빌드 + 로그인 리다이렉트까지만 확인"이라고 솔직히 보고
- DB 변경은 **SQL 파일로만** 드리고 사용자가 실행: `SQL\` 폴더, **UTF-8 BOM 없음 + CRLF** (PowerShell `[IO.File]::WriteAllText(path, text, New-Object Text.UTF8Encoding($false))`). 운영 DB에 직접 붙지 않는다. 데이터를 읽어야 하면 로컬 전용(Development + loopback) 임시 액션을 만들고 확인 뒤 바로 지운다
- 게스트에게 안 보이는 부분(홈 카드 등)을 봐야 하면 같은 방식의 임시 액션(`PartialView("_Banner_xxx")`)으로 확인 → 삭제 → 다시 빌드

## 3. Razor 함정
- CSS 안: `@@media`, `@@keyframes`
- 한글이 바로 뒤에 오면 `@(x)`로 감싼다: `@(n)장`, `@(ConstatsCommon.ToHome)`
- 한글 바로 뒤 `@if` 금지 → 띄우거나 줄을 나눈다. `@i@if`도 줄을 나눈다
- 이메일처럼 읽힘: `class="a@(c)"` ✗ → `class="a @(c)"` ✓, `장@(...)` ✗ → `장 @(...)` ✓
- `IEnumerable`엔 `.Count ==` 대신 `!x.Any()`

## 4. 최신 테마 (기본 — 밝은 파랑)
글꼴: `"Noto Sans KR", "Malgun Gothic", system-ui, sans-serif`

| 토큰 | 값 | 쓰임 |
|---|---|---|
| blue | `#2F80ED` | 주 버튼, 링크 강조, 선택 |
| soft | `#EAF2FD` (hover `#DCEAFB`) | 알약 버튼 바탕, 아이콘 바탕 |
| ink | `#111827` | 제목 |
| tx | `#374151` | 본문, "← 홈으로" 링크 색 |
| dim | `#6B7280` / 더 흐림 `#9CA3AF` | 보조 글자, 날짜 |
| line | `#EEF0F3` | 줄 구분선 |
| grey | `#F4F6F9` | 페이지 바탕, 회색 띠 |
| num | `#7FA3DB` | 번호, › 화살표 |
| NEW 꼬리표 | 바탕 `#FBEFE7` / 글자 `#B4531F` | 7일 이내 |

카드 공통
```css
.card { background:#fff; border:1px solid #E2E8F0; border-radius:8px; box-shadow:0 1px 6px rgba(0,0,0,.06); padding:4px 16px; }
.cardHd { display:flex; justify-content:space-between; align-items:center; padding:14px 0 6px; }
.cardHd b { font-size:16px; font-weight:800; color:#111827; }
.more { padding:4px 12px; border-radius:999px; background:#EAF2FD; color:#2F80ED; font-size:12.5px; font-weight:800; text-decoration:none; }
.row { display:flex; gap:12px; padding:14px 0; border-top:1px solid #EEF0F3; color:#374151; text-decoration:none; }
.row b { font-size:15px; font-weight:800; line-height:1.4; color:#111827; word-break:keep-all; }
.meta { font-size:12px; color:#6B7280; font-variant-numeric:tabular-nums; }
.ex { font-size:13px; line-height:1.6; color:#374151; -webkit-line-clamp:2; }   /* 미리보기 두 줄 */
```
- 홈 카드 모서리는 모두 **8px** (Home/Index의 `--home-card-r: 8px` + 맨 끝 JS walker가 맞춰 줌)
- 위로 가기 링크는 **"← 홈으로"** (`‹` 아님), 색 `#374151`
- 어두운 그라데이션 상자, 반투명 카드, 이모지 남발, 큰 그림자는 쓰지 않는다 (사용자가 "찍어 바른 느낌", "탁하다"고 싫어함)
- `Html.Raw`로 글 내용을 찍지 않는다 → 태그를 걷어 낸 글자만
- 휴대폰 폭(375px)에서 가로 스크롤 없게. 확인은 `resize_window mobile`
- 공용 CSS: `quiz-bright.css`(qb-*), `jumanna-bright.css`(jm-*), `present-bright.css`(.pbar), `pray-bright.css` — 같은 계열 화면이면 재사용

## 5. 보조 테마 — 민트 (가족 사진 콘테스트 전용, 2026-10-06~07 확정)
홈 카드 `Views/Shared/_Banner_FamilyPhotoContest.cshtml`, 리뷰 보드 `Views/FamilyPhoto/Judge.cshtml`, 카톡 이미지에 씀. **이 영역은 민트 유지.**

| 토큰 | 값 |
|---|---|
| 버튼·포인트 | `#12A883` (hover `#0E9474`) |
| 진한 글자 | `#0E9F7E` / 아주 진한 `#1F3D35` |
| 연한 바탕 | `#E9F8F2`, `#F2FBF8` |
| 선 | `#D5EFE6` |
| 보조 글자 | `#5B7A70` |
| AI 동그라미 | `#14B88F` 흰 글자, 바깥 링 `#D5F3E9` |
| 카드 바탕 | `linear-gradient(180deg,#F2FBF8 0%,#FFFFFF 60%)` |

- 갈색·누런 바탕(예전 "밀빛")은 쓰지 않는다
- 카톡 대표 이미지(1200×630)는 Edge 헤드리스로 HTML → PNG:
  `msedge.exe --headless=new --hide-scrollbars --force-device-scale-factor=1 --window-size=1200,630 --virtual-time-budget=10000 --user-data-dir=<scratch>\edge-prof --screenshot=<out.png> file:///<html>`
  이미지를 바꾸면 `?v=날짜번호`를 올린다 (카톡 캐시). 아이 실제 사진은 공유 이미지에 넣지 않는다
- 지금 리뷰 보드 이미지: `wwwroot/images/og/family-photo-review-note.png?v=2026100701` (L안 손글씨 칭찬 쪽지)

## 6. 시안(Artifact) 만들 때
- 사이트 실제 문구·데이터로 만든다 (lorem 금지). 맨 위 검은 띠에 "지금 모습 / A안 / B안 …" 버튼, 아래에 안마다 설명 한 단락
- 한 번에 2~3안, 사용자가 "더 보여줘"면 같은 시안 파일에 안을 추가
- 보고는 짧게: 무엇이 바뀌는지, 추천안 하나

## 7. 최근 상태 (참고)
- 가족 사진 콘테스트: "결과 보기"+"AI 심사평"을 **AI 리뷰 보드** 하나로 합침 (`/FamilyPhoto/Result`는 발표 뒤 Judge로 이동). 점수·등수·시상대는 화면에 내지 않는다. 사진마다 별명 배지(5종, 배지당 최대 사진수/5+1장) + AI 칭찬 + ❤️ 좋아요 수 + 스티커 🥰😂👍🙏
- **사용자가 실행해야 할 SQL**: `SQL\FamilyPhotoSticker_20261006.sql` (스티커 표 — 실행 전엔 스티커 줄이 숨겨짐). `SQL\FixPageNo_BessingWife_20261004.sql`도 실행 여부 미확인
- 주소록(`/AddressBook`) 전화번호 옆 버튼은 `/ApplyEvent/Branch` 생일 배너와 같은 "전화" 알약(`.callIc`)으로 바꿈. 아래 줄 "전화" 버튼과 중복 — 지울지 사용자 답 대기
- 홈: 햄버거 메뉴(왼쪽) + 교회명 + 날짜 상단바, 인사 카드(히어로), 기도 카드 통합(`_Banner_TodayPrayer`), 도움 카드 3종(`_Banner_HelpCards`), 미션북 카드, 최신 게시(`_Banner_NoticeBoardTops`) — 모두 새 테마 적용 완료

이해했으면 "준비됐어요"라고만 답하고, 내가 다음 화면을 말할 때까지 기다려.
