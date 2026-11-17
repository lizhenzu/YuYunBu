-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/2 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0202', N'하나님께 자신을 내어드리게 하옵소서', N'행 13:17',
 N'이 이스라엘 백성의 하나님이 우리 조상들을 택하시고 애굽 땅에서 나그네 된 그 백성을 높여 큰 권능으로 인도하여 내사',
 N'인도해 주시는 하나님,
오늘도 주님의 이름으로 저의 아이를 축복합니다. 하나님께서 이제까지 그의 삶을 주장하시고, 간섭해 주셨음에 감사드립니다. 생각과 말 그리고 행동에 은혜를 내려 주셔서 성도로서 살아오게 하셨음을 믿습니다. 이제, 간절히 바라옵기는 저의 아이가 자기를 인도해 주시는 하나님께 대한 믿음을 고백하게 하옵소서.
하나님 아버지,
반항심이라는 가라지가 심겨진 마음에 주님의 은혜를 부어 주옵소서. 성령님의 깨닫게 하시는 역사로 자신의 반항에 대하여 볼 수 있게 하옵소서. 집에서는 부모의 말에, 학교에서는 선생님의 말씀에 반항하는 독초가 자라고 있음을 알게 하옵소서. 그들에게 반항심의 독초가 자라게 한 사탄의 궤계를 예수님의 이름으로 대적합니다. 거역과 고집, 불순종을 물리치려는 마음을 갖게 하시고, 반항을 거절하는 용기를 주옵소서.
미쁘신 하나님,
저의 아이가 여호와 하나님께 자신을 내어드리게 하옵소서. 하나님의 손길에 자신을 내려놓기를 소망합니다. 오직 주님의 간섭하시는 손길을 바라보고 기도하는 아이가 되게 하옵소서. 주님의 은혜를 바라는 기다림의 용기와 하나님이 하신다는 담대함을 놓치지 않게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"조상들율","To":"조상들을"},{"From":"애국 땅","To":"애굽 땅"},{"From":"이름으 로","To":"이름으로"},{"From":"이제까 지","To":"이제까지"},{"From":"그 리고","To":"그리고"},{"From":"하나님아버지,","To":"하나님 아버지,"},{"From":"하읍 소서","To":"하옵소서"},{"From":"독 초가","To":"독초가"},{"From":"사탄의 기계를","To":"사탄의 궤계를"},{"From":"불순종 을","To":"불순종을"},{"From":"미쁘신하나님, (문단 끝에 붙음)","To":"미쁘신 하나님, (호칭 줄로 옮김)"},{"From":"하읍소서","To":"하옵소서"},{"From":"내려농기를","To":"내려놓기를"},{"From":"기다림 의","To":"기다림의"},{"From":"원하면 서","To":"원하면서"},{"From":"아멘.,","To":"아멘."}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0202';
