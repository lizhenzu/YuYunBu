-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 01/14 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0114', N'믿음이 자라고 사랑에 풍성하게 하옵소서', N'살후 1:3',
 N'형제들아 우리가 너희를 위하여 항상 하나님께 감사할찌니 이것이 당연함은 너희 믿음이 더욱 자라고 너희가 다 각기 서로 사랑함이 풍성함이며',
 N'영광의 하나님,
저의 아이에게 주님의 날을 지키도록 해 주셨음에 감사드립니다. 하나님의 시간을 하나님께 드리는 거룩함을 보여드리게 하시니 참으로 즐겁습니다. 어려서부터 여호와 하나님께 속한 것을 구별할 수 있는 용기를 주시니, 영광을 받아 주옵소서.
간구를 들으시는 주여,
오늘을 주님께 거룩하게 함으로써 주일을 중심으로 해서 한 주간의 생활을 하게 하옵소서. 저의 아이가 자신의 생활시간표를 작성할 때, 주일이 첫째 자리에 놓게 하시기를 간절히 원합니다. 하나님을 예배하는 것을 최고의 기쁨으로 여기게 하시고, 주 하나님의 합당한 영광을 드리는 것을 자신의 소명으로 생각하게 하옵소서.
아버지 하나님,
이제, 교회공동체 안에서 자신의 주변에 성령님의 은혜로 충만한 친구들의 울타리를 치게 해 주시기를 소망합니다. 지혜로운 자와 동행을 하면 지혜로워진다고 하신 말씀을 기억하여 친구를 사귀는데 주의하게 하옵소서. 미련한 자와 사귀면 해를 받는다고 주의를 주셨으니, 미련한 친구는 멀리하게 하옵소서. 어려서부터 교회 안에서 친구들과 한마음으로 주님의 몸을 이루는 은혜를 누리기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"영제들아","To":"형제들아"},{"From":"믿음 이","To":"믿음이"},{"From":"하나 님의","To":"하나님의"},{"From":"거륙함을","To":"거룩함을"},{"From":"즐겁 습니다","To":"즐겁습니다"},{"From":"생 활을","To":"생활을"},{"From":"하읍소서","To":"하옵소서"},{"From":"주일 이","To":"주일이"},{"From":"것 을","To":"것을"},{"From":"아버지 하나님.","To":"아버지 하나님,"},{"From":"이제,교회공동체","To":"이제, 교회공동체"},{"From":"주번에","To":"주변에"},{"From":"은헤로","To":"은혜로"},{"From":"친구 들의","To":"친구들의"},{"From":"하 면","To":"하면"},{"From":"주의하계 하 옵소서","To":"주의하게 하옵소서"},{"From":"친 구는","To":"친구는"},{"From":"에수님","To":"예수님"},{"From":"기도 드립니다. 아멘","To":"기도드립니다. 아멘."}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0114';
