-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/28 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0228', N'새 학기에 주님의 인도를 바라게 하옵소서', N'레 26:45',
 N'내가 그들의 하나님이 되기 위하여 열방의 목전에 애굽에서 인도하여 낸 그들의 열조와 맺은 언약을 그들을 위하여 기억하리라 나는 여호와니라',
 N'살아계신 하나님,
자기 백성을 만족하게 하시는 그 크신 이름으로 저의 아이를 축복합니다. 추운 겨울 동안에 건강을 지켜주셨고, 긴 겨울방학을 마치고 학교에 다니도록 함께 해주셨음에 감사드립니다. 저의 아이에게 복을 주사 마음껏 공부를 하고, 자신의 소질도 계발하는 기회를 주셨음을 감사드립니다. 이제, 새 학년에 오르고 새 학기를 맞으면서 더욱 하나님과 사람들에게 사랑을 받도록 이끌어 주옵소서.
아버지 하나님,
이제 곧 새 학년의 새 학기가 시작되고, 새로운 선생님을 만나게 될 것입니다. 새 학년에는 좀 더 공부에 흥미를 느끼게 하시고, 하나님 앞에서 자신을 준비하는 근면함을 허락해 주시옵소서. 선생님의 가르침을 통해서 배워야 할 과목을 잘 학습하도록 이끌어 주옵소서. 나아가 경건한 교사를 담임으로 만나 좋은 영향을 받는 복을 주옵소서.
은혜의 하나님,
하나님의 자녀로서의 복을 누리는 가운데 2월의 마지막 날을 맞이합니다. 저의 아이를 지켜주셔서 은혜 가운데 한 달을 믿음으로 지내온 것에 감사드립니다. 이 달에 행하고자 마음먹은 것들을 잘 해낼 수 있게 도와주신 여호와의 손을 자랑합니다. 내일, 새롭게 시작되는 달에 소망을 품게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"휴","To":"(날짜 앞 잡글자 — 뺌)"},{"From":"레 26:15","To":"레 26:45"},{"From":"얼방의","To":"열방의"},{"From":"애급에서","To":"애굽에서"},{"From":"일자와","To":"열조와"},{"From":"축복합니 다","To":"축복합니다"},{"From":"마음 것","To":"마음껏"},{"From":"감사드립니 다","To":"감사드립니다"},{"From":"사람들에 게","To":"사람들에게"},{"From":"아버지 하나님, 자신 (문단 끝에 붙음)","To":"아버지 하나님, (호칭 줄로 옮김 · 잡글자 뺌)"},{"From":"될 것 입니다","To":"될 것입니다"},{"From":"통해 서","To":"통해서"},{"From":"교 사를","To":"교사를"},{"From":"맞이합 니다","To":"맞이합니다"},{"From":"은해 기운네","To":"은혜 가운데"},{"From":"것 에","To":"것에"},{"From":"해벌","To":"해낼"},{"From":"도 와주신","To":"도와주신"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0228';
