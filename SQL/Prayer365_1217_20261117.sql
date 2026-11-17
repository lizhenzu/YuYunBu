-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/17 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1217', N'남을 불쌍히 여기는 마음을 주옵소서', N'사 61:10',
 N'내가 여호와로 인하여 크게 기뻐하며 내 영혼이 나의 하나님으로 인하여 즐거워하리니 이는 그가 구원의 옷으로 내게 입히시며 의의 겉옷으로 내게 더하심이 신랑이 사모를 쓰며 신부가 자기 보물로 단장함 같게 하셨음이라',
 N'은혜로우신 하나님,
저의 아이가 하나님을 더 알기를 소망합니다. 하나님을 더 깊이 알고, 지식과 총명이 있게 하옵소서. 여호와 하나님에 대한 지식이 있어 선과 악을 분별하게 하옵소서. 지극히 선한 것을 분별함으로써 하늘의 하나님께 영광을 드리고, 선한 일에 힘쓰게 하옵소서. 성령님께서 주시는 하나님에 대한 지식으로 선한 일에 더욱 힘쓰게 하옵소서.
사랑의 하나님,
오늘, 저의 아이가 이웃을 대할 때, 남을 불쌍히 여기는 마음을 주시옵소서. 주님의 긍휼히 여기심을 은혜로 주시어 이웃을 대할 때, 주님의 손길을 나타내 보이게 하옵소서. 하나님은 영광을 받으시고, 세상에 읽혀지는 주님의 편지가 되게 하옵소서. 하나님께서 저의 아이의 마음과 손과 발을 통하여 이웃에게로 나아가심을 깨달아 행동하게 하옵소서.
좋으신 하나님,
저의 아이에게 이웃을 향해 친절을 베풀어서 사랑의 열매를 맺게 하옵소서. 어려움에 빠진 이웃을 도움으로써 선행의 열매를 맺게 하옵소서. 예수님을 영접하였거나 그렇지 않았나를 떠나 하나님의 사랑으로 이웃을 대하게 하옵소서. 그래서 베풀 만한 힘이 있을 때, 남을 도와주게 하시고, 마땅히 도움을 받을 자에게 친절하게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"줄거워하리니","To":"즐거워하리니"},{"From":"내기 입히시며","To":"내게 입히시며"},{"From":"걸옷","To":"겉옷"},{"From":"쓰 며","To":"쓰며"},{"From":"같기 하셨음이라","To":"같게 하셨음이라"},{"From":"0저의 아이가","To":"저의 아이가"},{"From":"하나님 께","To":"하나님께"},{"From":"하나 님에","To":"하나님에"},{"From":"주시옵 소서","To":"주시옵소서"},{"From":"긍율히","To":"긍휼히"},{"From":"손 길을","To":"손길을"},{"From":"읽혀 지는","To":"읽혀지는"},{"From":"손 과발을","To":"손과 발을"},{"From":"하옵 소서","To":"하옵소서"},{"From":"이웃 을","To":"이웃을"},{"From":"하 시고","To":"하시고"},{"From":"예수 님의","To":"예수님의"},{"From":"366 자녀를 위한 365일 축복 기도문","To":"(쪽 머리글 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1217';
