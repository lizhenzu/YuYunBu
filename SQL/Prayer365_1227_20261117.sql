-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/27 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1227', N'생명의 진리를 알게 하옵소서', N'습 3:17',
 N'너의 하나님 여호와가 너의 가운데 계시니 그는 구원을 베푸실 전능자시라 그가 너로 인하여 기쁨을 이기지 못하여 하시며 너를 잠잠히 사랑하시며 너로 인하여 즐거이 부르며 기뻐하시리라 하리라',
 N'시간의 하나님,
한 해의 삶을 마치고 새해를 준비하면서 저의 아이가 성령님의 충만하심으로 옷 입기를 소망합니다. 성령님을 모시고 자신의 생활에 대한 계획을 세우고, 결단을 하는 시간을 갖게 하옵소서. 그를 깨닫게 하시고, 가장 좋은 것으로 인도하시는 성령님께 자신을 내어맡기기 원합니다. 하나님께서 이끌어 가시는 대로 아멘으로 응답하며 따라가게 하옵소서.
삼위일체의 하나님,
성령님께서 저의 아이와 함께 하심을 즐거워합니다. 그가 자신을 뉘우치고 새롭게 결단을 한다 할지라도 성령님의 도우심이 없이는 아무런 열매도 맺지 못한다는 진리를 알게 하옵소서. 성령님께서 그의 마음의 생각을 아시고 친히 도우시며, 하나님 아버지께 간구해주심도 믿습니다. 성령님의 다스림을 환영하도록 하옵소서.
거룩함의 하나님,
신랑으로 이 땅에 오시는 주님이 오실 때, 저의 아이가 신부로서의 모습을 갖추게 하옵소서. 신랑을 기다리며 살아가는 믿음의 순결함을 지니게 하옵소서. 우리 주 예수님께서 강림하실 때 기쁨으로 맞이하게 하옵소서. 온전히 거룩하고 흠이 없어야 신부의 자격으로 신랑을 맞게 하옵소서. 말과 행동이 거룩하고 옳은 행실을 가짐으로 신부의 단장인 세마포를 준비하는 삶을 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"인 하여","To":"인하여"},{"From":"줄거이","To":"즐거이"},{"From":"중만하 심으로","To":"충만하심으로"},{"From":"계 획을","To":"계획을"},{"From":"하 나님께서","To":"하나님께서"},{"From":"뉘우 치고","To":"뉘우치고"},{"From":"아무런열 매도","To":"아무런 열매도"},{"From":"하읍소서","To":"하옵소서"},{"From":"생 각을","To":"생각을"},{"From":"거록함의 하나님, 오 토으동","To":"거룩함의 하나님,"},{"From":"모 습을","To":"모습을"},{"From":"지니 게 하옵소서 우리","To":"지니게 하옵소서. 우리"},{"From":"예수님께 강림하실","To":"예수님께서 강림하실"},{"From":"하옵소 서","To":"하옵소서"},{"From":"세마포 를","To":"세마포를"},{"From":"376 자녀를 위한 365일 축복 기도문","To":"(쪽 머리글 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1227';
