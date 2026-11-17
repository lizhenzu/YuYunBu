-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/15 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1215', N'어려운 애들과 함께 하게 하옵소서', N'사 40:3-4',
 N'외치는 자의 소리여 가로되 너희는 광야에서 여호와의 길을 예비하라 사막에서 우리 하나님의 대로를 평탄케 하라 골짜기마다 돋우어지며 산마다, 작은 산마다 낮아지며 고르지 않은 곳이 평탄케 되며 험한 곳이 평지가 될 것이요',
 N'영광의 하나님,
성탄절이 우리를 위해 예수님이 오셨음을 축하하는 시간이라면, 저의 아이가 예수님의 사랑을 이웃에게 나누어 주도록 이끌어 주옵소서. 그가 주님으로부터 받은 것은 참으로 많습니다. 우선, 그의 마음에 가득 차 있는 하나님의 사랑을 나누게 해 주시기를 소망합니다.
하나님이 우리에게 오심처럼, 저의 아이도 남들에게 다가가기 원합니다. 자기가 갖고 있지만, 남들에게 없는 것을 보게 하옵소서. 사실, 제 것이 제 것이 아니고 거저 받은 것들인데, 없어서 힘들어 하는 이들에게 나누어 주도록 하옵소서.
아주 적은 것이지만 자신의 물건을 이웃과 나눌 수 있게 하시고, 군것질을 할 때, 어려운 애들과 함께 할 수 있도록 해 주옵소서. 그의 마음에 긍휼이 있게 하옵소서. 사랑이 있는 곳에 하나님이 계심을 믿습니다.
예배를 받으시는 하나님,
저의 아이가 주 여호와의 이름을 높여드리며 예배드리는 시간을 사모하기를 소망합니다. 주님의 날이 세상으로부터 구별되고, 하나님께 영광을 드리는 시간이기를 바라는 마음을 주옵소서. 하늘의 하나님께는 영화로움을 드리고 주님께서 세상을 향해서 복을 주시는 예배를 기다리게 하옵소서. 이를 위해서 오늘 밤은 거룩히 보내게 하옵소서.
원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"어려운 애플과","To":"어려운 애들과"},{"From":"길윤","To":"길을"},{"From":"하 나님","To":"하나님"},{"From":"우어지며","To":"돋우어지며"},{"From":"고르 지","To":"고르지"},{"From":"평단케","To":"평탄케"},{"From":"평지가 됩 것이요","To":"평지가 될 것이요"},{"From":"차 있 는","To":"차 있는"},{"From":"소망합니다 하나님이","To":"소망합니다. / 하나님이"},{"From":"원합니 다","To":"원합니다"},{"From":"제 것 이","To":"제 것이"},{"From":"나 누어","To":"나누어"},{"From":"하읍소서","To":"하옵소서"},{"From":"군것 질","To":"군것질"},{"From":"사모 하기를","To":"사모하기를"},{"From":"영광 을","To":"영광을"},{"From":"영화 로움을","To":"영화로움을"},{"From":"에배를","To":"예배를"},{"From":"하 옵소서","To":"하옵소서"},{"From":"364 자녀를 위한 365일 축복 기도문","To":"(쪽 머리글 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1215';
