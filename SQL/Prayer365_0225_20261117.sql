-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/25 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0225', N'주님의 자비하심을 찬송하게 하옵소서', N'창 24:27',
 N'가로되 나의 주인 아브라함의 하나님 여호와를 찬송하나이다 나의 주인에게 주의 인자와 성실을 끊이지 아니하셨사오며 여호와께서 길에서 나를 인도하사 내 주인의 동생집에 이르게 하셨나이다 하니라',
 N'거룩하신 하나님,
여호와 하나님의 날을 맞이해서 아침의 이른 시간에 예배당으로 가게 하시니 감사드립니다. 저희 가정을 복되게 하시고, 믿음으로 사는 자녀를 주신 은혜를 찬양합니다. 저의 아이에게 몸으로만 예배당으로 향하지 말게 하시고, 마음으로 우리 주님을 찬송하게 하옵소서. 언제나 곁에 계셔주셨으며, 자비하심으로 함께 하신 손길을 찬송하게 하옵소서.
하나님 아버지,
어려서부터 주일을 구별하여 하나님께 돌려드리는 습관을 배우게 하셨습니다. 오늘 예배와 성경공부 그리고 교회의 여러 가지 활동들을 통해서 천국에서의 하루를 살게 하옵소서. 교회에서 경험하는 활동으로 말미암아 신령함에 대한 도전을 받게 하옵소서. 하나님을 영화롭게 해드리는 가운데 복스러운 소망으로 보내게 하옵소서.
저의 아이가 어려운 집안 형편으로 말미암아 낙심하지 않도록 하옵소서. 가난하기 때문에 해보고 싶은 것들을 하지 못할 때, 괴로워하지 않기를 소망합니다. 존번연은 어려운 집안에서 태어나 학교도 다녀보지 못하였으나 천로역정이라는 훌륭한 책을 썼던 것같이, 저의 아이에게도 번연이 가졌던 은혜를 지니게 하옵소서. 가정의 형편보다도 하나님의 도우심이 우선이라는 은혜를 깨닫게 하시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"인자 와","To":"인자와"},{"From":"동생집 에","To":"동생집에"},{"From":"자녀 를","To":"자녀를"},{"From":"계 셔주셨으며","To":"계셔주셨으며"},{"From":"하읍소서","To":"하옵소서"},{"From":"하 셨습니다","To":"하셨습니다"},{"From":"통 해서","To":"통해서"},{"From":"말 미암아","To":"말미암아"},{"From":"해드리 는","To":"해드리는"},{"From":"하음소 서","To":"하옵소서"},{"From":"않기 를","To":"않기를"},{"From":"못 하였으나","To":"못하였으나"},{"From":"천로역정 이라는","To":"천로역정이라는"},{"From":"홀륭한","To":"훌륭한"},{"From":"히옵소서","To":"하옵소서"},{"From":"도 우심이","To":"도우심이"},{"From":"이름으 로기도드립니다","To":"이름으로 기도드립니다"},{"From":"71","To":"(쪽 번호 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0225';
