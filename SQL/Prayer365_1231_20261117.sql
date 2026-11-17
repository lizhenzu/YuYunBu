-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/31 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1231', N'예비된 상급을 바라보게 하옵소서', N'담후 4:7-8',
 N'내가 선한 싸움을 싸우고 나의 달려갈 길을 마치고 믿음을 지켰으니 이제 후로는 나를 위하여 의의 면류관이 예비되었으므로 주 곧 의로우신 재판장이 그 날에 내게 주실 것이니 내게만 아니라 주의 나타나심을 사모하는 모든 자에게니라',
 N'전능하신 하나님,
벌써 한 해의 삶을 끝내는 마지막 날에 서 있습니다. 돌이켜 볼 때, 사랑하는 저의 가족이 지내온 365일은 주님의 은혜였습니다. 이 해를 시작하는 첫날 아침부터 주님께서는 은혜의 손을 펴 주셨습니다. 저와 저의 가족에게 부족함을 느끼지 못하고 살아오게 하신 목자 하나님의 이름을 찬양합니다.
고마우신 하나님,
지난 한 해 하나님은 참으로 좋으신 아버지가 되어 주셨습니다. 주님의 넘치는 자비로우심으로 저희들은 살아왔습니다. 저희에게 베풀어주신 그 모든 은혜를 생각할 때, 끝이 없는 감사를 드립니다. 때를 따라 돕는 은혜로 도우시며, 저희 가족과 저의 아이의 삶이 물댄 동산과 같이 모자람이 조금도 없게 하셨으니 감사드립니다.
자비로우신 하나님,
지금까지 지내왔던 1년 12달 동안 하나님은 저의 아이의 편이 되어 주셨습니다. 첫 주일에 영광과 찬미로 예배하게 하신 은혜가 1년 동안 주일을 지키게 해주셨습니다. 오늘로서 금년의 마지막 주일을 지킵니다. 하나님 아버지, 새해를 맞이하면서 회개의 영을 내려주시기를 간구합니다. 시작과 나중이 되시는 하나님께만 영광을 드리는 예배이기를 소망하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"달려갈 기운","To":"달려갈 길을"},{"From":"민음울","To":"믿음을"},{"From":"지켰으너","To":"지켰으니"},{"From":"것이 니","To":"것이니"},{"From":"전능하신 하나님, 벌써","To":"전능하신 하나님, / 벌써"},{"From":"사 랑하는","To":"사랑하는"},{"From":"시작 하는","To":"시작하는"},{"From":"한 해하나님은","To":"한 해 하나님은"},{"From":"주님 의","To":"주님의"},{"From":"념치는","To":"넘치는"},{"From":"베풀어주 신","To":"베풀어주신"},{"From":"돕 는","To":"돕는"},{"From":"모 자람이","To":"모자람이"},{"From":"주 셨습니다","To":"주셨습니다"},{"From":"에배하게","To":"예배하게"},{"From":"주일 을","To":"주일을"},{"From":"지컵니다","To":"지킵니다"},{"From":"하 나님","To":"하나님"},{"From":"소망하면 서","To":"소망하면서"},{"From":"아멘","To":"아멘."},{"From":"380 자녀를 위한 365일 축복 기도문","To":"(쪽 머리글 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1231';
