-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/4 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0204', N'주일에 여호와를 높이게 하옵소서', N'사 48:17',
 N'너희의 구속자시요 이스라엘의 거룩하신 자이신 여호와께서 가라사대 나는 네게 유익하도록 가르치고 너를 마땅히 행할 길로 인도하는 너희 하나님 여호와라',
 N'예배로 불러주시는 하나님,
거룩한 아침에 하나님의 이름을 높여드립니다. 저의 아이가 하늘에 마음을 두고 예배를 드리기를 즐거워하며, 교회 안에 머무르게 하심을 기뻐합니다. 주님께서 하나님의 날로 구별해 주신 시간에 여호와의 이름을 높이는 영광을 갖게 하옵소서. 예배하는 시간에 하나님께는 영광이 되고, 신령과 진정으로 예배를 드리게 하옵소서.
거룩하신 하나님,
예배하는 한 시간, 기쁜 마음으로 작은 입술을 벌려 찬송하게 하옵소서. 그 입술로 하나님을 영화롭게 해드리게 하옵소서. 예수님을 주님으로 믿는 신앙을 고백하게 하옵소서. 저의 아이가 자신을 온전히 주님께 드리고, 하나님의 다스리심에 자신을 맡기도록 이끌어 주옵소서. 예배하는 시간에 주 하나님의 거룩하심이 예배당 안에 가득하게 하옵소서.
하나님 아버지,
예배 시간에 목사님의 입술을 빌려 선포되는 말씀이 재미있고도 참된 말씀이기를 소망합니다. 그 말씀 한 절, 한 절을 들으면서 소망을 갖게 해 주시며, 저의 아이의 심령을 뜨겁게 하옵소서. 생명의 말씀으로 힘을 얻어 새 삶을 다짐하게 하옵소서. 그리고 주님의 이름이 널리 퍼지도록 기도하게 하기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"휴","To":"(날짜 앞 잡글자 — 뺌)"},{"From":"너회의","To":"너희의"},{"From":"여효와께서","To":"여호와께서"},{"From":"유익하 도록","To":"유익하도록"},{"From":"(사 4817.","To":"사 48:17"},{"From":"하늘에마 음을","To":"하늘에 마음을"},{"From":"하심을기 빼합니다","To":"하심을 기뻐합니다"},{"From":"되 고","To":"되고"},{"From":"하읍소 서","To":"하옵소서"},{"From":"헤드리게","To":"해드리게"},{"From":"하읍소서","To":"하옵소서"},{"From":"주님으 로","To":"주님으로"},{"From":"예배하 는","To":"예배하는"},{"From":"뜨집게","To":"뜨겁게"},{"From":"하음소서","To":"하옵소서"},{"From":"에수님의","To":"예수님의"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0204';
