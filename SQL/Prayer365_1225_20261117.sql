-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/25 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1225', N'아기 예수님의 영광을 찬양하게 하옵소서', N'눅 1:78-79',
 N'이는 우리 하나님의 긍휼을 인함이라 이로써 돋는 해가 위로부터 우리에게 임하여 어두움과 죽음의 그늘에 앉은 자에게 비취고 우리 발을 평강의 길로 인도하시리로다 하니라',
 N'영광의 하나님,
오늘은 기쁜 날입니다. 저희를 사랑하셔서, 이 땅에 예수님을 보내 주셨습니다. 약속대로 메시야가 오셔서 생명의 길을 열어 주셨으니 저의 아이가 성탄절 예배를 드리면서 감사드리게 하옵소서. 영광의 찬양을 하늘에 드릴 때, 천군과 천사들도 찬양하게 하옵소서. 처음 성탄절의 그 맑고 환한 밤과 같은 영광이 넘치게 하옵소서.
생명을 구원하시는 하나님,
오늘도 하나님께서 구원하시기로 작정된 이들이 주님께 돌아오기를 원합니다. 이 기쁜 성탄절에도 구원함에 이르는 날이 되기를 원하여 기도드리게 하옵소서. 교회로 가면서 전도할 애들을 보려는 마음을 갖게 해주옵소서. 예수님을 구주로 믿어 멸망치 않고 영생을 얻는 이들이 있게 하옵소서.
전능하신 하나님,
아기 예수님의 나심으로, 인류를 구원하시려는 하나님의 뜻이 이루어졌음에 찬양을 드리게 하옵소서. "하나님이 세상을 이처럼 사랑하사 독생자를 주셨으니" 감사드립니다. 이 기쁘고 복된 사건을 기리게 하옵소서. 주님의 사랑을 받는 저의 아이가 하나님을 경배하고 나신 아기께 영광을 드리도록 이끌어 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"그늘을 인합이라","To":"긍휼을 인함이라"},{"From":"어두 움과","To":"어두움과"},{"From":"않은 자에게","To":"앉은 자에게"},{"From":"평가의 길로","To":"평강의 길로"},{"From":"영광의 하나님","To":"영광의 하나님,"},{"From":"주 셨습니다","To":"주셨습니다"},{"From":"하 늘에","To":"하늘에"},{"From":"맑 고환한","To":"맑고 환한"},{"From":"기 도드리게","To":"기도드리게"},{"From":"갓게 해주읍소서","To":"갖게 해주옵소서"},{"From":"여생을","To":"영생을"},{"From":"있 게하옵소서","To":"있게 하옵소서"},{"From":"이루어 졌음에","To":"이루어졌음에"},{"From":"독 생자","To":"독생자"},{"From":"하읍소 서","To":"하옵소서"},{"From":"영 광을","To":"영광을"},{"From":"기도드립 니다. 아멘. 사고","To":"기도드립니다. 아멘."},{"From":"374 자녀를 위한 365일 축복 기도문","To":"(쪽 머리글 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1225';
