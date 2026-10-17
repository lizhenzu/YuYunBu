# 인수인계 프롬프트 — 자녀를 위한 365일 축복 기도문 입력 (2026-10-10)

아래 전체를 새 창 첫 메시지로 붙여 넣으세요.

---

너는 잠실새내교회 유년부 사이트의 **"자녀를 위한 365일 축복 기도문"** 입력 작업을 이어서 한다. 아래 규칙을 그대로 따른다.

## 1. 프로젝트
- ASP.NET Core MVC (.NET 9), EF Core · 경로 `C:\WorkSpace\yunbu\bulletin-board-master.3\bulletin-board-master\YunYunBu2025\YunYunBu2025`
- 답은 **한국어**. **모든 답변 첫 줄에 "추천 모델: Opus/Sonnet — 이유 한 줄"** (기도문 교정·SQL 만들기는 보통 Sonnet, 화면·코드를 고치면 Opus)
- 시간은 `DateHelper.GetKoreaNow()` / `GetKoreaToday()`만 (`DateTime.Now`/`Today` 금지)
- 허락 묻지 말고 진행 (CLAUDE.md). 단 코드·화면을 바꿀 때는 시안 먼저

## 2. 무엇을 하는가
- 책: 「자녀를 위한 365일 축복 기도문」 **한치호 지음 · 도서출판 두돌비**
- 날짜(월일)마다 한 편을 DB에 넣고, 그날 기도문을 매일 보여 준다
- 입력은 **B 방식**: 내가 대화창에 기도문 **텍스트**(또는 책 쪽 사진)를 올리면 → 너는 **교정**해서 → **SQL 파일**로 만들어 준다 → 내가 SSMS에서 실행한다
- 서버 OCR · 외부 API 키는 쓰지 않는다

## 3. 교정 규칙 (가장 중요)
- **있는 그대로** 둔다. **오타와 이상한 문구만** 고친다
- 확대 해석 · 문장 다듬기 · 바꾸기 · 덧붙이기 **금지**
- 띄어쓰기 · 문장부호도 **틀린 것만**
- 옛 성경 말투(하옵소서, 주옵소서 등)는 그대로
- 확실하지 않으면 고치지 말고 나에게 묻는다
- 고친 곳은 **"원문 → 교정" 목록**으로 따로 보여 준다
- 책 쪽수 줄("286 자녀를 위한…"), 달 표지 줄("10월")은 본문이 아니므로 뺀다
- 성경 구절 주소가 틀리면 개역한글 기준으로 바로잡고 목록에 적는다 (예: 수 21:1 → 수 24:14)

## 4. 사진을 여러 장 올릴 때
- 날짜별로 읽어 한꺼번에 처리한다
- 내가 말한 범위 밖 날짜 사진은 넣지 말고 알려 준다
- 머리 날짜가 이미 입력된 날짜와 겹치면 덮어쓰지 말고, 다음 빈 날짜에 넣은 뒤 나에게 확인받는다

## 5. DB 표 `dbo.Prayer365` (이미 있음)
| 열 | 형식 | 설명 |
|---|---|---|
| MonthDay | CHAR(4) PK | '0101'~'1231', '0229' 포함 |
| Title | NVARCHAR(200) | 제목 |
| VerseRef | NVARCHAR(100) | 구절 주소 (예: 잠 22:6) |
| VerseText | NVARCHAR(1000) | 구절 본문 |
| Body | NVARCHAR(MAX) | 기도문 본문 — **한 줄이 한 문단**(호칭 "○○하나님,"도 한 줄), 줄바꿈 한 번씩. 끝은 "아멘." (화면이 아멘을 따로 붙임). 구절 따옴표는 넣지 않음 (VerseText 는 따옴표 없이, 화면이 “ ” 를 붙임). 실제 예: `SQL\Prayer365_1112-1118_20261010.sql` |
| Fixes | NVARCHAR(MAX) | 교정 기록 JSON `[{"From":"…","To":"…"}]` (없으면 `N'[]'`) |
| IsReviewed | BIT | **넣을 때 바로 1** (검수 완료로 저장 — 따로 "검수완료" 말 안 해도 됨) |
| UpdatedAt | DATETIME2 | 한국 시간 (`DATEADD(HOUR, 9, SYSUTCDATETIME())`) |
| UpdatedBy | NVARCHAR(100) | `N'Claude'` |

## 6. SQL 파일 만드는 법
- 위치: `SQL\Prayer365_날짜범위_YYYYMMDD.sql` (예: `SQL\Prayer365_1112-1120_20261010.sql`)
- **UTF-8 BOM 없음 + CRLF**. PowerShell로 저장:
  `[IO.File]::WriteAllText($path, $text, (New-Object Text.UTF8Encoding($false)))`
  (BOM이 있으면 SSMS에서 "Incorrect syntax near '﻿'" 오류가 남)
- 한 날짜당 `MERGE` 한 덩어리. 같은 날짜를 다시 올리면 **덮어쓴다**
- 모든 문자열은 `N'…'`, 작은따옴표는 `''`로 두 번
- 파일 맨 위 `SET XACT_ABORT ON; BEGIN TRAN;`, 맨 끝 `COMMIT;` + 확인용 SELECT
```sql
MERGE dbo.Prayer365 AS t
USING (SELECT '1112' AS MonthDay) AS s ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title=N'…', VerseRef=N'…', VerseText=N'…', Body=N'…', Fixes=N'[…]', IsReviewed=1,
     UpdatedAt=DATEADD(HOUR,9,SYSUTCDATETIME()), UpdatedBy=N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay,Title,VerseRef,VerseText,Body,Fixes,IsReviewed,UpdatedAt,UpdatedBy)
     VALUES ('1112',N'…',N'…',N'…',N'…',N'[…]',1,DATEADD(HOUR,9,SYSUTCDATETIME()),N'Claude');
```
- 만든 뒤 파일 맨 앞 3바이트가 EF BB BF가 아닌지 확인하고, 파일이 0바이트가 아닌지 확인
- 실행은 내가 한다 — SSMS **파일 → 열기**로 열어 실행 (붙여 넣기 하면 글자가 깨질 수 있음)
- 너는 운영 DB에 직접 붙지 않는다 (`appsettings`에 원격 접속 정보가 있어도 쓰지 않는다)

## 7. 이어서 시작할 때
- 2026-09-27 기준 **9/27 ~ 11/11 마흔여섯 편** 입력 완료. 그 뒤로 더 넣었을 수 있으니, 먼저 아래 확인 SQL을 나에게 주고 결과를 받아 마지막 날짜를 확인한다
```sql
SELECT COUNT(*) AS 입력수, MIN(MonthDay) AS 처음, MAX(MonthDay) AS 마지막 FROM dbo.Prayer365;
SELECT MonthDay, Title, IsReviewed FROM dbo.Prayer365 ORDER BY MonthDay;
```
- 받아 둔 1월 1일 사진은 범위 밖이라 아직 안 넣음

## 8. 화면 (이미 만들어져 있음 — 이번 작업은 입력만)
- `/Prayer365` 오늘의 기도 · `/Prayer365/Admin` 365일 달력판 · `/Prayer365/Edit?md=1112` 확인·오타 수정
- 코드: `Controllers/Prayer365Controller.cs`, `Services/Prayer365/Prayer365Service.cs`, `Models/Prayer365/Prayer365Models.cs`, `Views/Prayer365/*`, 홈 배너 `Views/Shared/_Banner_Prayer365.cshtml`
- `Prayer365Config.StaffOnly = true` — 출판사 · 목사님 허락 전이라 **관리자만** 본다. 허락받으면 false로 바꾼다 (내가 말할 때만)
- 저작권: 책 본문은 출판사 허락이 필요하다 (내 책임). 너는 내가 준 텍스트·사진만 교정해서 옮긴다
- 확인은 게스트로만, 로그인·비밀번호 입력 금지

## 9. 매번 보고 형식
1. 처리한 날짜 목록 (예: 11/12 ~ 11/20, 9편)
2. 날짜별 "원문 → 교정" 목록 (고친 곳이 없으면 "고친 곳 없음")
3. 물어볼 것 (확실하지 않은 곳)
4. 만든 SQL 파일 경로 + "SSMS 파일 → 열기로 실행해 주세요"

이해했으면 "준비됐어요"라고 답하고, 위 7번의 확인 SQL을 보여 준 뒤 기다려.
