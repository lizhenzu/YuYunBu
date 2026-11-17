-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/28 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1228', N'의롭지 못했던 삶을 회개하게 하옵소서', N'눅 13:8-9',
 N'대답하여 가로되 주인이여 금년에도 그대로 두소서 내가 두루 파고 거름을 주리니 이 후에 만일 실과가 열면이어니와 그렇지 않으면 찍어버리소서 하였다 하시니라',
 N'영원하신 하나님,
여호와의 권능이 저의 아이를 여기까지 이끌어 주셨습니다. 여호와의 자비하심이 저의 아이를 이만큼 자라게 하셨습니다. 눈동자 같이 지키시며 각양 좋은 것으로 만족하게 해 주신 하나님께 감사하면서 지난 시간들을 돌아보게 하옵소서. 집에서 보냈던 시간들, 학교에 다니는 동안에 공부를 하며 친구들과 지냈던 시간들, 그리고 또 다른 시간들을 뒤돌아 보게 하옵소서.
죄를 용서해 주시는 하나님,
하나님 앞에서 의롭지 못했던 삶을 회개하게 하옵소서. 이 해를 시작하면서 거룩한 백성으로 살겠다고 다짐을 했으나 다 지키지 못하였던 게으름을 용서해 주옵소서. 열심을 내어 계획한대로 살고자 노력하지 못한 게으름을 용서하옵소서. 주님의 말씀에 순종하여 이웃에게 베풀어야 하는 사랑과 용서에 게을렀던 죄도 용서해 주시기 원합니다.
하나님의 자녀로서 마땅히 여호와의 이름을 영화롭게 해드려야 했건만 도리어 영광을 가리었음을 뉘우치게 하옵소서. 아울러 육체의 욕심을 따랐던 죄를 뉘우치게 하옵소서. 주님의 사랑을 의지하며 회개할 때, 그리스도의 보혈로 씻어주셔서 깨끗하게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"13:8- 9.","To":"13:8-9"},{"From":"이 추 에","To":"이 후에"},{"From":"영원하신 하나님 여호와의","To":"영원하신 하나님, / 여호와의"},{"From":"지키시 며","To":"지키시며"},{"From":"시간 들을","To":"시간들을"},{"From":"시작 하면서","To":"시작하면서"},{"From":"게 으름을","To":"게으름을"},{"From":"주옵소서, 열심을","To":"주옵소서. 열심을"},{"From":"용서하읍소서","To":"용서하옵소서"},{"From":"배풀어야 하 는","To":"베풀어야 하는"},{"From":"게울렸던","To":"게을렀던"},{"From":"해드러야","To":"해드려야"},{"From":"했건 만","To":"했건만"},{"From":"뉘우치고 하유소서","To":"뉘우치게 하옵소서"},{"From":"회개한 때","To":"회개할 때"},{"From":"그 리스도","To":"그리스도"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1228';
