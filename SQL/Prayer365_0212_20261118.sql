-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/12 (1편)
-- 2026-11-18 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0212', N'더러운 것에 마음을 내어주지 않게 하옵소서', N'대상 22:13',
 N'네가 만일 여호와께서 모세로 이스라엘에게 명하신 모든 율례와 규례를 삼가 행하면 형통하리니 강하고 담대하여 두려워 말고 놀라지 말찌어다',
 N'말씀의 하나님,
한 주간의 생활을 시작하면서 주님의 이름으로 저의 아이를 축복합니다. 그를 위하여 주신 생명의 말씀으로 풍성한 하루를 살게 하옵소서. 주님의 말씀을 입으로 고백하는 활용의 은총을 허락하옵소서. 힘들 때마다 그 말씀을 자신에게 선포하여 강건함을 누리기 원합니다. 큰 소리로 성경을 읽어 자신의 귀에 능력의 말씀이 선포되게 하옵소서.
기적의 하나님,
병자를 치유하셨던 능력의 말씀이 저희에게 있음을 감사드립니다. 그 치유의 말씀으로 명하노니, 오늘 저의 아이들이 건강하게 지낼 것을 믿습니다. 공부의 중압감에 눌려 있는 마음이 주님의 보혈로 씻음 받음을 믿습니다. 피곤하고 지친 마음에 성령님의 은혜로 새 힘이 공급됨을 믿습니다. 하나님의 말씀이 우레처럼 들려서 힘차게 하옵소서.
세워주시는 하나님,
저의 아이의 귀를 축복합니다. 그가 귀로 듣는 것에 조심하도록 분별의 은혜를 내려 주옵소서. 자신이 하나님의 사람으로 살아가기에 훼방을 하는 이야기는 거절하게 하시고, 더러운 것에 마음을 내어주도록 꼬이는 말은 듣지 않게 하옵소서. 듣는 것이 마음과 생각을 지배한다는 사실을 잊지 않아, 거룩함을 추구하는 것만 듣게 하옵소서. 불의한 말은 듣지 말게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"휴","To":"(날짜 앞 잡글자 — 뺌)"},{"From":"형 통하리너","To":"형통하리니"},{"From":"축복합니 다","To":"축복합니다"},{"From":"주 님의","To":"주님의"},{"From":"성 경을","To":"성경을"},{"From":"믿 습니다","To":"믿습니다"},{"From":"분별 의은혜를","To":"분별의 은혜를"},{"From":"주읍소서","To":"주옵소서"},{"From":"하음소서,","To":"하옵소서."},{"From":"들는 것이","To":"듣는 것이"},{"From":"있지 않아","To":"잊지 않아"},{"From":"거룩합을","To":"거룩함을"},{"From":"는게 하음소서","To":"듣게 하옵소서"},{"From":"말 게","To":"말게"},{"From":"아민","To":"아멘"},{"From":"게 사","To":"(잡글자 — 뺌)"},{"From":"58 자녀를 위한 365일 축복 기도문","To":"(쪽 머리글 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0212';
