-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/22 (1편)
-- 2026-11-18 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0222', N'주님의 사랑으로 이웃을 축복하게 하옵소서', N'시 1:3',
 N'저는 시냇가에 심은 나무가 시절을 좇아 과실을 맺으며 그 잎사귀가 마르지 아니함 같으니 그 행사가 다 형통하리로다',
 N'복을 주시는 하나님,
축복의 언어를 주신 하나님의 인자하심을 찬양합니다. 저의 아이들이 하늘의 언어를 사용하기를 축복합니다. 아침에 일어났을 때, 먼저 자신들을 향해서 축복하도록 이끌어 주옵소서. 축복의 사람으로 보내졌으니, 만나는 모든 이들에게 복을 빌게 하옵소서. 미움의 말을 버리고 축복의 말을 하기 원합니다. 비난의 말을 버리고 축복의 말을 하기 원합니다. 허물을 덮어주며, 주님의 사랑으로 용서하면서 축복하게 하옵소서.
의로우신 하나님,
저의 아이가 자신의 삶을 통해서 주님께 의의 제물이 되는 모습을 보이게 하옵소서. 주님께서 그에게 주신 것 가운데 일부를 돌려드리는 것에 언제나 민감하게 하옵소서. 그가 부모나 이웃의 어른들로부터 용돈을 받았을 때, 우선적으로 십분의 일을 떼어서 드리게 하옵소서. 재물에 대한 십일조의 신앙이 모든 삶의 영역에서 더해지기를 소망합니다.
하나님 아버지,
저의 아이가 돈을 사용하는 일에 하나님께 정직하게 하옵소서. 하고 싶은 것들이 많지만, 그것들이 주님께 영광을 드리는 일인지를 살펴서 돈을 쓰게 하옵소서. 자신의 돈이라 하여 마음이 내키는 대로 쓰지 않도록 하옵소서. 아주 적은 돈이지만 거기에서부터 청지기의 지혜를 훈련받게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"시냈가에","To":"시냇가에"},{"From":"좋아","To":"좇아"},{"From":"과실율","To":"과실을"},{"From":"아너함","To":"아니함"},{"From":"같으 니그","To":"같으니 그"},{"From":"자신 들을","To":"자신들을"},{"From":"허 물을","To":"허물을"},{"From":"보 이게","To":"보이게"},{"From":"것 에","To":"것에"},{"From":"하읍소서","To":"하옵소서"},{"From":"대 한","To":"대한"},{"From":"살퍼서","To":"살펴서"},{"From":"않도 록하옵소서","To":"않도록 하옵소서"},{"From":"훈련받 게","To":"훈련받게"},{"From":"에수님의","To":"예수님의"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0222';
