-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/18 (1편)
-- 2026-11-18 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0218', N'여호와의 이름을 부르게 하옵소서', N'수 24:15',
 N'만일 여호와를 섬기는 것이 너희에게 좋지 않게 보이거든 너희 열조가 강 저편에서 섬기던 신이든지 혹 너희의 거하는 땅 아모리 사람의 신이든지 너희 섬길 자를 오늘날 택하라 오직 나와 내 집은 여호와를 섬기겠노라',
 N'살아계신 하나님,
저희 가족에게 여호와를 하나님으로 삼는 은혜를 주셨음에 감사드립니다. 지난 시간 동안에도 저의 아이를 지켜 주시고 돕는 손길이 되어 주셨음을 찬양합니다. 이 복스러운 날 아침에 그가 기쁨으로 여호와를 섬기고, 찬송을 부르며 예배하러 나아가기 원합니다. 주님께서 차려 놓으신 은혜의 식탁을 생각하며 감격하는 예배가 되기 원합니다. 여호와의 이름을 부르는 것만으로 은혜의 시간이 되게 하옵소서.
좋으신 하나님,
예배를 드리는 시간에 신령과 진정을 경험하게 하옵소서. 마음을 다하고, 성품을 다하여 머리를 조아리게 하옵소서. 예배의 한 순서, 한 순서에 정성껏 따르게 하시고, 오직 영광을 하나님께 드리려는 마음으로 뜨겁게 하옵소서. 함께 예배하는 친구들에게 마음을 팔지 않게 하옵소서.
하나님 아버지,
오늘은 우리 민족의 설날이온데, 저의 아이에게 조상들이 지켜온 새해를 맞이하는 이 날에 어른을 공경함과 자기의 생활에 대한 다짐을 하는 은혜로운 시간을 주옵소서. 우리 조상들은 우상을 숭배하면서 이 명절을 지켰으나 하나님 앞에서 가족과 부모의 의미를 묵상하도록 이끌어 주옵소서. 하나님을 기쁘시게 해드리는 삶을 계획하게 하기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"여혼와를","To":"여호와를"},{"From":"너회","To":"너희"},{"From":"섬기 던","To":"섬기던"},{"From":"신이는지 홈","To":"신이든지 혹"},{"From":"랙하 라","To":"택하라"},{"From":"Iloio-lo,","To":"(잡글자 — 뺌)"},{"From":"감사드립 니다","To":"감사드립니다"},{"From":"주 셨음을","To":"주셨음을"},{"From":"섬 기고","To":"섬기고"},{"From":"놓으 신","To":"놓으신"},{"From":"어어이의s","To":"(잡글자 — 뺌)"},{"From":"좋으신 하나님","To":"좋으신 하나님,"},{"From":"다하 고","To":"다하고"},{"From":"순서 에","To":"순서에"},{"From":"뜨 접게","To":"뜨겁게"},{"From":"하나님아버지,","To":"하나님 아버지,"},{"From":"새해 를","To":"새해를"},{"From":"주입 소서","To":"주옵소서"},{"From":"예수 님의","To":"예수님의"},{"From":"~^ 자녀를 위한 365일 축복 기도무","To":"(쪽 머리글 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0218';
