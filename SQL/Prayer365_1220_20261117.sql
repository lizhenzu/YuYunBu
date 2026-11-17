-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/20 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1220', N'말씀을 듣기를 기뻐하게 하옵소서', N'사 61:10',
 N'내가 여호와로 인하여 크게 기뻐하며 내 영혼이 나의 하나님으로 인하여 즐거워하리니 이는 그가 구원의 옷으로 내게 입히시며 의의 겉옷으로 내게 더하심이 신랑이 사모를 쓰며 신부가 자기 보물로 단장함 같게 하셨음이라',
 N'말씀의 하나님,
저의 아이가 성탄절을 기다리면서 말씀으로 오신 예수님에 대한 새로움을 경험하기 원합니다. 말씀에 대한 자세를 통하여 아기 예수님을 경배하는 성탄절을 맞이하기를 소망합니다. 먼저, 말씀을 듣기를 기뻐하도록 이끌어 주옵소서. 믿음은 들음에서 나며 이 말씀은 주님으로 말미암은 것임을 고백하게 하옵소서.
순종의 예물을 받으시는 하나님,
저의 아이가 말씀에 순종하기를 즐거워하게 하옵소서. 말씀을 듣고 순종한 마리아에게서 하나님의 뜻이 이루어졌음을 깨닫습니다. 또한, 말씀이 저의 아이의 삶에서 이루어질 것을 기다리게 하옵소서. 하나님은 미쁘시기에 틀림없이 이루어질 것을 기대하는 마음을 갖게 하옵소서.
성결함의 하나님,
저의 아이에게 친구들과 더불어 지내는 은혜를 주심을 감사드립니다. 그가 친구들과 어울릴 때, 성령님의 인도하심에 주목하게 하옵소서. 성령님께 거스르는 생각이나 행동을 하지 않게 하시고, 산 제사를 드리는 모습이게 하옵소서. 친구들과 대화를 나눌 때, 하나님께 드려도 좋을 거룩한 이야기를 하게 하시고, 놀이를 즐기는 순간에도 주님께 드려지는 것이 되게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"하나넘으로","To":"하나님으로"},{"From":"줄거워하리니","To":"즐거워하리니"},{"From":"걸옷","To":"겉옷"},{"From":"쓰 며","To":"쓰며"},{"From":"새로 움을","To":"새로움을"},{"From":"경 배하는","To":"경배하는"},{"From":"기뻐하도 록","To":"기뻐하도록"},{"From":"말미암 은것임을","To":"말미암은 것임을"},{"From":"순종의 예물을 받으시는 하나님, 저의","To":"순종의 예물을 받으시는 하나님, / 저의"},{"From":"순 종한","To":"순종한"},{"From":"말씀 이","To":"말씀이"},{"From":"미 쁘시기에","To":"미쁘시기에"},{"From":"하옵소서. >","To":"하옵소서."},{"From":"성결함의하나님","To":"성결함의 하나님"},{"From":"성렁 님께","To":"성령님께"},{"From":"모 습이게 하음소서","To":"모습이게 하옵소서"},{"From":"거룩 한","To":"거룩한"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1220';
