-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/18 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1218', N'바울의 삶을 따르게 하옵소서', N'눅 1:70-71',
 N'이것은 주께서 예로부터 거룩한 선지자의 입으로 말씀하신 바와 같이 우리 원수에게서와 우리를 미워하는 모든 자의 손에서 구원하시는 구원이라',
 N'왕의 왕이신 하나님,
저의 아이가 주 안에서 서는 은혜로 오늘 하루를 살게 하옵소서. 그에게 이 세상의 국적과 함께 하늘나라의 시민권을 지니게 하셨으니 하늘나라의 백성으로서의 자신을 갖추게 하옵소서. 저의 아이가 예수님을 자기의 모델로 삼되, 그리스도인의 본이 되어준 바울을 본받게 하옵소서. 성경에서 배울 수 있는 바울의 삶을 자기의 것으로 삼는 은혜를 주옵소서.
자기 백성들의 하나님,
예수님의 이 땅에 오심은 잃어버린 천국의 백성을 찾으심인 줄 믿습니다. 성탄절을 맞으면서 저의 아이가 영혼의 구원에 대한 열정을 갖기를 소망합니다. 복음을 전해야 하는 거룩한 부담을 느끼게 하시고, 생명의 선물로 오신 주님을 증거하게 하옵소서. 주님께서 찾으시는 자기 백성들에게로 마음을 열게 하옵소서.
구원의 하나님,
오늘, 저의 아이가 성탄절의 의미를 묵상할 때, 구원하시는 하나님의 열심을 깨닫게 하옵소서. 잃어버린 영혼을 찾으시는 하나님의 마음을 품고 예수님을 전하게 하옵소서. 저의 아이로 하여금 입을 벌려 복음의 비밀을 담대히 알리게 하옵소서. 성탄절의 복을 이웃과 나누게 하시되, 구원에 이르는 복된 소식을 전하려는 사명감을 갖도록 이끌어 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"따르게 하주읍소서","To":"따르게 하옵소서"},{"From":"원수에게서 와","To":"원수에게서와"},{"From":"왕의 왕이신 하나님","To":"왕의 왕이신 하나님,"},{"From":"그에 게","To":"그에게"},{"From":"하늘나 라의","To":"하늘나라의"},{"From":"성 경에서","To":"성경에서"},{"From":"주옵소서. 자기 백성들의 하나님","To":"주옵소서. / 자기 백성들의 하나님,"},{"From":"믿습니 다","To":"믿습니다"},{"From":"중거하게","To":"증거하게"},{"From":"백성들 에게로","To":"백성들에게로"},{"From":"품 고","To":"품고"},{"From":"하읍소서","To":"하옵소서"},{"From":"비 밀을","To":"비밀을"},{"From":"구 원에","To":"구원에"},{"From":"갓도록","To":"갖도록"},{"From":"아빈.","To":"아멘."},{"From":"36","To":"(쪽 번호 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1218';
