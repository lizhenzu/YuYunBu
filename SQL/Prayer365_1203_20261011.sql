-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/3 (1편)
-- 2026-10-11 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1203', N'주님의 기쁜 날을 기다리게 하옵소서', N'창 3:15',
 N'내가 너로 여자와 원수가 되게 하고 너의 후손도 여자의 후손과 원수가 되게 하리니 여자의 후손은 네 머리를 상하게 할 것이요 너는 그의 발꿈치를 상하게 할 것이니라 하시고',
 N'복의 근원이신 하나님,
한 주간의 생활을 시작하면서 주님의 이름으로 저의 아이를 축복합니다. 그를 위하여 예비해 두신 복을 허락하옵소서. 성탄절을 맞이하면서 하나님께 복을 받을 사람이 되게 하옵소서. 세례 요한이 주님의 오시는 길을 예비했듯이, 예수님을 생각하며 기쁜 날을 기다리기 원합니다.
살아계신 하나님,
저의 아이가 성탄절을 기다릴 때, 주님께서 기뻐하시는 모습을 갖추게 하옵소서. 예수님께서 이 땅에 오셔야만 하셨던 하나님의 뜻을 바르게 알게 하시고, 거룩하신 이름을 찬양하게 하옵소서. 자기 자신의 유익과 공부하는 것으로 분주하게 지내왔던 지난 시간을 돌아보고, 혹시 성탄절에 어울리지 않는 죄의 모습이 있다면 회개하도록 하옵소서. 자기 자신을 깨끗하게 하는 은혜를 경험하기 원합니다.
메시야의 하나님,
옛날, 믿음의 조상들은 처음 성탄절을 기다렸지만, 저희들은 다시 오시마 약속하시며, 하늘로 가셨던 예수님의 재림을 기다리기 원합니다. 이미 오신 예수님의 생일을 축하하면서 또 다시 오실 예수님을 기다리도록 이끌어 주심을 믿습니다. 메시야의 약속이 이루어지던 날, 하나님의 아들은 초라하게 오셨지만 다시 오시는 예수님께서는 하나님의 영광 가운데 오시리라 믿으면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"온수가 되게","To":"원수가 되게"},{"From":"여 자의 훼손은","To":"여자의 후손은"},{"From":"상하비 할","To":"상하게 할"},{"From":"하올소서","To":"하옵소서"},{"From":"하움소서","To":"하옵소서"},{"From":"하읍소서","To":"하옵소서"},{"From":"재립을","To":"재림을"},{"From":"에수님을","To":"예수님을"},{"From":"하나님으 아들","To":"하나님의 아들"},{"From":"영광 기 운데","To":"영광 가운데"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1203';
