-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/5 (1편)
-- 2026-10-11 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1205', N'주님 앞에서 소망을 품게 하옵소서', N'히 11:5',
 N'믿음으로 에녹은 죽음을 보지 않고 옮기웠으니 하나님이 저를 옮기심으로 다시 보이지 아니하였느니라 저는 옮기우기 전에 하나님을 기쁘시게 하는 자라 하는 증거를 받았느니라',
 N'에녹의 하나님,
오늘은 저의 아이가 하나님을 기쁘시게 해드리면서 살았던 에녹의 은혜를 묵상하기 원합니다. 에녹이 자기의 마음을 하나님께 드리고 가까이 하면서 여호와의 손길을 느낀 것을 배우게 하옵소서. 하나님을 의지하고, 도와주심을 믿었기에 동행할 수 있었음을 깨닫게 하옵소서. 그 자신이 여호와 앞에서 떳떳했기에 동행의 은혜를 누렸음을 알게 하옵소서.
기쁨을 주시는 하나님,
에녹이 여호와의 이름으로 살아가는 것을 즐거워한 것을 저의 아이도 경험하게 하옵소서. 하나님의 사람으로 살기를 소망하여 믿음으로 동행 했던 그 걸음이 저의 아이의 것이 되게 하옵소서. 자신의 소원을 하나님께 맡긴 이상, 헛된 꿈이나 까닭 모를 걱정을 하지 않게 하옵소서. 저의 아이의 심령이 주님의 은혜 안에서 굳게 자리를 잡게 하옵소서.
함께 하시는 하나님,
간절히 기도드리오니, 세상의 근심과 어리석은 욕망을 품는 불신앙의 자세에서 건져 주옵소서. 에녹을 만드신 하나님의 손길이 저의 아이에게 나타나기를 간절히 구합니다. 주님을 앙망하는 가운데 소망을 품고, 주님의 도우심이 나타나는 때를 기다리게 하옵소서. 그래서 아무 것도 저를 두렵지 않게 하옵소서. 믿음의 굳건함에서 결코 흔들리지 않도록 이끌어 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"아너하너라","To":"아니하였느니라"},{"From":"옵기우기","To":"옮기우기"},{"From":"(8] 11:5)","To":"(히 11:5)"},{"From":"은 혜를","To":"은혜를"},{"From":"가까0 하면서","To":"가까이 하면서"},{"From":"의지하 고.","To":"의지하고,"},{"From":"그 자신 이","To":"그 자신이"},{"From":"하움소서","To":"하옵소서"},{"From":"하올소서","To":"하옵소서"},{"From":"하나님 께","To":"하나님께"},{"From":"불신앙으 자세","To":"불신앙의 자세"},{"From":"주읍소서","To":"주옵소서"},{"From":"주님음","To":"주님을"},{"From":"주 님의","To":"주님의"},{"From":"하음소서","To":"하옵소서"},{"From":"저 를","To":"저를"},{"From":"하읍소서","To":"하옵소서"},{"From":"군건함","To":"굳건함"},{"From":"0 끌어","To":"이끌어"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1205';
