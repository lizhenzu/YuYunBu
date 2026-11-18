-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/16 (1편)
-- 2026-11-18 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0216', N'하나님을 가까이 하게 하옵소서', N'창 50:24',
 N'요셉이 그 형제에게 이르되 나는 죽으나 하나님이 너희를 권고하시고 너희를 이 땅에서 인도하여 내사 아브라함과 이삭과 야곱에게 맹세하신 땅에 이르게 하시리라 하고',
 N'거룩하신 하나님,
저의 아이들이 하나님의 말씀을 가까이 하며 지내도록 이끌어 주옵소서. 하나님을 가까이 하는 것을 복으로 알고 지내게 하옵소서. 그리하여 시냇가에 심겨진 나무는 그 뿌리가 물가로 뻗어있어 시들거나 마르지 않는 것처럼, 그들의 인생이 늘 푸르게 되기를 소망합니다. 하나님을 소망하는 중에 풍성하게 하시는 은혜로 힘찬 생활을 하게 하옵소서.
구별을 바라시는 하나님,
주님을 닮기를 소원하는 저의 아이가 눈으로 보는 것에 주의를 기울이게 하옵소서. 하나님 앞에서 영적인 나실인으로서 보는 것을 분별하는 은혜를 주시기를 소망합니다. 보는 것이 생각을 지배할 수 있고 행동을 충동할 수 있음을 깨달아 조심하게 하옵소서. 이에, 보는 것으로도 주님을 영화롭게 해드리게 하옵소서.
담대하게 하시는 하나님,
저의 아이가 세상을 거절할 수 있는 용기를 지니게 하옵소서. 주님께 영광이 되지 않는 것은 쳐다보려 하지 않기를 원합니다. 그의 마음과 몸이 성도의 삶으로 선택되었으니, 더러운 것은 거절하게 하옵소서. 혹시 세상 친구들의 꼬임에 빠지거나 으스대도록 충동질하는 말에 솔깃해서 주님께로부터 멀어지지 않도록 붙들어 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"*","To":"(날짜 앞 잡글자 — 뺌)"},{"From":"너최를","To":"너희를"},{"From":"하교","To":"하고"},{"From":"주옵소 서","To":"주옵소서"},{"From":"시벗가에","To":"시냇가에"},{"From":"않 는","To":"않는"},{"From":"소망 하는","To":"소망하는"},{"From":"기울이 게","To":"기울이게"},{"From":"하읍소서","To":"하옵소서"},{"From":"주님 을","To":"주님을"},{"From":"0오","To":"(잡글자 — 뺌)"},{"From":"몸 이","To":"몸이"},{"From":"하움소서","To":"하옵소서"},{"From":"이 름으로","To":"이름으로"},{"From":"62 자녀를 위한 365일 축복 기도문","To":"(쪽 머리글 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0216';
