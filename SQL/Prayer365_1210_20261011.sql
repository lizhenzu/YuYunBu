-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/10 (1편)
-- 2026-10-11 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1210', N'바른 영에 대한 분별력을 주옵소서', N'시 112:4-5',
 N'정직한 자에게는 흑암 중에 빛이 일어나나니 그는 어질고 자비하고 의로운 자로다 은혜를 베풀며 꾸이는 자는 잘 되나니 그 일을 공의로 하리로다',
 N'참 신이신 하나님,
저의 아이에게 진리 안에서 참됨을 깨닫게 하옵소서. 분별의 영을 주셔서 참과 거짓을 가려내게 하옵소서. 거짓이 참인 양 날뛰고 있으며, 사탄이 성령을 위장하고 날뛰고 있습니다. 분별을 통하여 하나님께 영광을 드리기 원합니다. 이에, 다른 복음이 아닌 바른 복음, 다른 예수가 아닌 바른 예수, 다른 영이 아닌 바른 영에 대한 분별력을 갖게 하옵소서.
열매의 하나님,
저의 아이에게 하늘나라를 소망하면서 열매를 맺기를 소망합니다. 예수님으로 말미암아 의의 열매가 가득하게 하옵소서. 예수 그리스도를 믿음으로 의롭다 하심을 얻었으니 회개에 합당한 열매를 맺게 하옵소서. 그리고 거룩함에 이르는 열매, 성령의 열매를 믿게 하옵소서. 하늘로부터 임하는 성령님의 능력으로 의의 열매를 맺게 하옵소서.
온전함의 하나님,
저의 아이가 주님께서 인정하시는 온전함에 이르기를 바라게 하옵소서. 하나님의 사람으로 흠이 없고, 바로 서는 것에 마음을 두게 하옵소서. 하나님께로부터 사랑을 받고 있은즉, 그 사랑에 응답하여 온전히 서기를 소망하게 하옵소서. 나아가 그리스도의 분량에까지 자라기 위한 믿음의 훈련을 즐겨 감당하게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"후암 중에","To":"흑암 중에"},{"From":"은하 를","To":"은혜를"},{"From":"그. 일을","To":"그 일을"},{"From":"주 셔서","To":"주셔서"},{"From":"하읍소서","To":"하옵소서"},{"From":"있으며. 시 탄이","To":"있으며, 사탄이"},{"From":"이에.","To":"이에,"},{"From":"졸음이","To":"복음이"},{"From":"볶음","To":"복음"},{"From":"예 수님으로","To":"예수님으로"},{"From":"의률다","To":"의롭다"},{"From":"하옵소서 그리고","To":"하옵소서. 그리고"},{"From":"하늘로부 터","To":"하늘로부터"},{"From":"온전합의","To":"온전함의"},{"From":"홈이","To":"흠이"},{"From":"하옵소 서,","To":"하옵소서."},{"From":"하읍소 서.하나님께로부터","To":"하옵소서. 하나님께로부터"},{"From":"서 기를","To":"서기를"},{"From":"하움소서","To":"하옵소서"},{"From":"믿 음의","To":"믿음의"},{"From":"감당하게 주시기를","To":"감당하게 해 주시기를"},{"From":"기도 드립니다","To":"기도드립니다"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1210';
