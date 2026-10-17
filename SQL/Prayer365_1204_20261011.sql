-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/4 (1편)
-- 2026-10-11 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1204', N'예수님의 사랑을 구하게 하옵소서', N'눅 1:31-33',
 N'보라 네가 수태하여 아들을 낳으리니 그 이름을 예수라 하라 저가 큰 자가 되고 지극히 높으신 이의 아들이라 일컬을 것이요 주 하나님께서 그 조상 다윗의 위를 저에게 주시리니 영원히 야곱의 집에 왕노릇 하실 것이며 그 나라가 무궁하리라',
 N'하나님 아버지,
예수님께서 이 땅에 나시었을 때, 천군과 천사들은 찬양을 하였습니다. 주님의 오심으로 평화를 주신 하나님께 찬양의 예배를 드렸습니다. 예수님으로 말미암은 평화가 저의 아이의 가슴에 넘치기를 소망합니다. 예수님의 이름만으로 즐거움이 넘치게 하옵소서. 보혜사 성령님의 역사로 세상이 알지 못하는 평안을 간직하게 하옵소서.
용서하시는 하나님,
자신의 즐거움만 생각하고, 남들보다 더 나으려는 생각 때문에 초조해 하고 근심에 갇히게 된 것을 불쌍히 여겨 주시옵소서. 주님을 바라보지 못하고, 자신의 생각에만 집중하다 보니 근심이 끊이지 않고 있습니다. 이제, 더욱 하나님을 바라보며 평안을 구하게 하옵소서. 믿음으로 세상의 근심에서 벗어나게 하옵소서. 마음에 근심하지 말고 하나님을 믿으니 또 나를 믿으라고 하셨습니다.
성탄절을 기다리면서 평화를 주시려고 오신 주님을 생각하기 원합니다. 예수님의 오심으로 이미 평화가 주어졌음을 깨닫게 하옵소서. 저의 아이가 참으로 하나님을 믿고 주 예수 그리스도를 자신의 생명의 주로 믿어 근심과 걱정에서 자유함을 얻도록 이끌어 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"아들은 낳으리너","To":"아들을 낳으리니"},{"From":"그. 이름을","To":"그 이름을"},{"From":"되교","To":"되고"},{"From":"지국8 높으심 이의 1 아들이라","To":"지극히 높으신 이의 아들이라"},{"From":"주시르 니","To":"주시리니"},{"From":"야급의","To":"야곱의"},{"From":"하움소서","To":"하옵소서"},{"From":"하음소서","To":"하옵소서"},{"From":"같히게","To":"갇히게"},{"From":"세싱 의","To":"세상의"},{"From":"또나를","To":"또 나를"},{"From":"주 예수. 그리스도","To":"주 예수 그리스도"},{"From":"아벤,","To":"아멘."}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1204';
