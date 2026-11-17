-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/19 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1219', N'하나님의 약속이 이루어짐을 믿게 하옵소서', N'눅 2:25',
 N'예루살렘에 시므온이라 하는 사람이 있으니 이 사람이 의롭고 경건하여 이스라엘의 위로를 기다리는 자라 성령이 그 위에 계시더라',
 N'시므온의 하나님,
오늘은 저의 아이가 시므온의 경건을 묵상하기 원합니다. 하나님 앞에서 의롭게 지내면서 약속된 메시야를 기다리는 그의 삶을 배우게 하옵소서. 메시야만이 이스라엘의 위로가 됨을 믿어 간절하게 기다렸던 것을 깨닫게 하옵소서. 시므온이 주님을 기다리며 살게 하신 하나님의 손길로 저의 아이를 만져 주시기를 소망합니다.
경건한 자의 하나님,
시므온의 거룩한 은혜를 저의 아이도 경험하게 하옵소서. 하나님의 약속이 이루어질 것을 믿으면서 메시야를 기다렸던 믿음이 저의 아이의 것이 되게 하옵소서. 성전에서 하나님을 섬기는 일에 헌신하는 중에, 메시야로 말미암아 이루어질 구원을 기다렸으니, 그 믿음으로 사는 저의 아이이기를 원합니다.
하나님 아버지,
이제 간절히 구하오니, 성탄절을 기다리면서 하나님의 약속이 주는 의미를 깨닫게 하옵소서. 메시야를 보면서 즐거워했던 시므온처럼, 성탄절을 기다리는 저의 아이의 마음에 기쁨이 충만하게 하옵소서. 여호와 앞에서 의롭게 지내기를 소원하게 하시고, 이미 오신 예수님으로 말미암아 찬양을 드리게 하옵소서. 성탄절의 은혜를 누리게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"위 로를","To":"위로를"},{"From":"그. 위에","To":"그 위에"},{"From":"시므온의 하나님.","To":"시므온의 하나님,"},{"From":"앞에 서","To":"앞에서"},{"From":"하옵소 서","To":"하옵소서"},{"From":"경건한자의","To":"경건한 자의"},{"From":"약 속이","To":"약속이"},{"From":"것 이","To":"것이"},{"From":"메시 야로","To":"메시야로"},{"From":"아 이이기를","To":"아이이기를"},{"From":"하나님아버지","To":"하나님 아버지"},{"From":"의 미를","To":"의미를"},{"From":"깨단게 하읍소서","To":"깨닫게 하옵소서"},{"From":"성탄절 을","To":"성탄절을"},{"From":"앞 에서","To":"앞에서"},{"From":"에수님","To":"예수님"},{"From":"안양을","To":"찬양을"},{"From":"은헤를","To":"은혜를"},{"From":"아멘. 수","To":"아멘."},{"From":"368 자녀를 위한 365일 축복 기도문","To":"(쪽 머리글 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1219';
