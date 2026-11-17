-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/23 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1223', N'아기 예수님의 오심을 묵상하게 하옵소서', N'미 5:2',
 N'베들레헴 에브라다야 너는 유다 족속 중에 작을찌라도 이스라엘을 다스릴 자가 네게서 내게로 나올 것이라 그의 근본은 상고에, 태초에니라',
 N'하나님 아버지,
대강절 넷째 주일에 하나님께 영광을 드립니다. 저의 아이가 성탄절을 기다리면서 아기로 오신 하나님을 기뻐하게 하옵소서. 주님이 받으셔야 하는 영광을 바치는 예배를 드리게 하옵소서. 성탄절의 주인이신 예수님께 마음과 생각, 몸을 다 드리는 경배의 시간을 갖게 하옵소서.
거룩하신 하나님,
날이 갈수록 세상은 비성경적인 문화로 가득 차는데 진리 안에서 살도록 이끌어 주옵소서. 성탄절을 맞이하면서 아기 예수님의 오심의 의미에 대하여 묵상하도록 해 주옵소서. 세상의 문화를 거절하고, 하나님의 뜻을 살피면서 살겠다는 각오가 다시 맞이하는 성탄절로 인하여 새로워지기를 소망합니다. 사망의 문화에서 벗어나 예수님께서 주시는 생명의 문화를 누리게 하옵소서.
은혜로우신 하나님,
예수님께서 주시는 생명을 받음으로 평강이 넘치게 하시고, 하나님께는 영광을 드리는 저의 아이가 되기를 소망합니다. 주님을 기뻐하므로 하늘의 은혜로 충만하게 하옵소서. 간절히 비오니, 즐거움을 찾기 위해서 사망의 문화에 기웃거리지 않게 하시고, 영원한 즐거움이 되시는 예수님의 품에 머물게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"베틀레침","To":"베들레헴"},{"From":"직속 중에","To":"족속 중에"},{"From":"다스민 자가","To":"다스릴 자가"},{"From":"네세서 내기로","To":"네게서 내게로"},{"From":"상꼬에","To":"상고에"},{"From":"하나님아버지","To":"하나님 아버지"},{"From":"예수님 께","To":"예수님께"},{"From":"살도 록이끌어","To":"살도록 이끌어"},{"From":"해 주읍소서","To":"해 주옵소서"},{"From":"뜻 을 살펴면서","To":"뜻을 살피면서"},{"From":"새로워지 기를","To":"새로워지기를"},{"From":"문 화를","To":"문화를"},{"From":"하움소서","To":"하옵소서"},{"From":"은혜로우신 하나님, 의 의 로스으스","To":"은혜로우신 하나님,"},{"From":"하나님께 는","To":"하나님께는"},{"From":"하읍소서","To":"하옵소서"},{"From":"위해 서","To":"위해서"},{"From":"에 수님","To":"예수님"},{"From":"에수님의 이름으로","To":"예수님의 이름으로"},{"From":"기도드립 니다. 아멘,","To":"기도드립니다. 아멘."},{"From":"372 자녀를 위한 365일 축복 기도문","To":"(쪽 머리글 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1223';
