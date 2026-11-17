-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/27 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0227', N'여호와를 향한 믿음을 굳게 하옵소서', N'민 15:41',
 N'나는 너희의 하나님이 되려하여 너희를 애굽 땅에서 인도하여 낸 여호와 너희 하나님이니라 나는 여호와 너희 하나님이니라',
 N'옥합을 깨뜨린 여인의 하나님,
오늘은 저의 아이가 주님을 사랑하여 자신의 향유를 담은 옥합을 깨뜨려 주님의 발에 부은 여인의 은혜를 묵상하기 원합니다. 주님께서 영광을 받으셔야 하셨기에, 하나님께서는 죄인들 중의 한 여인이 향유가 담긴 옥합을 깨뜨리게 하셨습니다. 여인으로 하여금 향유를 주님께 붓도록 하신 하나님의 열심이 저의 아이를 만져 주시기 원합니다. 여인이 예수님의 발을 자신의 눈물로 씻고, 향유를 부었음을 깨닫게 하옵소서.
하나님 아버지,
다른 이들보다 주님을 더욱 사랑하였기에, 그 사랑을 표현하려던 여인의 가슴을 경험하게 하옵소서. 옥합을 깨뜨린 여인의 사랑이 저의 아이의 것이 되어서 주님께 모든 것을 드리게 하옵소서. 값이 비싼 향유보다도 주님을 향한 사랑을 표현하려고 옥합을 깨뜨려야 했던 그 사랑을 닮겠다는 은혜를 주시옵소서.
인자하신 하나님,
여인의 행동을 주님께서 기뻐하시어 죄를 사해주신다는 선언이 있었음을 사모하게 하옵소서. 그래서 저의 아이가 주님께서 기뻐하실 만큼 사랑을 나타내 보이게 하옵소서. 주님을 향한 그의 믿음이 굳건해지게 하시고, 주님을 사랑함으로 저의 삶이 채워지게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"민 15:11","To":"민 15:41"},{"From":"애급 땅","To":"애굽 땅"},{"From":"하나님이 니라","To":"하나님이니라"},{"From":"너히","To":"너희"},{"From":"옥합을 깨뜨린 여인의 하나님, (본문과 붙음)","To":"옥합을 깨뜨린 여인의 하나님, (호칭 줄로 나눔)"},{"From":"깨뜨 려","To":"깨뜨려"},{"From":"영광 을","To":"영광을"},{"From":"담 긴옥합을","To":"담긴 옥합을"},{"From":"예수 님의","To":"예수님의"},{"From":"여인 의","To":"여인의"},{"From":"아이 의","To":"아이의"},{"From":"향유보다 도","To":"향유보다도"},{"From":"닭겠다는","To":"닮겠다는"},{"From":"옮부 트으","To":"(잡글자 — 뺌)"},{"From":"인자하신 하나님, (본문과 붙음)","To":"인자하신 하나님, (호칭 줄로 나눔)"},{"From":"있었 음을","To":"있었음을"},{"From":"기배하실","To":"기뻐하실"},{"From":"군건해지게","To":"굳건해지게"},{"From":"채위지게","To":"채워지게"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0227';
