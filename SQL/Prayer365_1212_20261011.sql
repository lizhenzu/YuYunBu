-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/12 (1편)
-- 2026-10-11 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1212', N'오신 예수님께 영광을 드리게 하옵소서', N'대상 16:23-24',
 N'온 땅이여 여호와께 노래하며 그 구원을 날마다 선포할찌어다 그 영광을 열방 중에, 그 기이한 행적을 만민 중에 선포할찌어다',
 N'목자들의 하나님,
오늘은 저의 아이가 왕으로 오신 예수님을 찾아 경배를 드렸던 목자들의 은혜를 묵상하기 원합니다. 천사들이 나타나 아기 예수님의 나심을 선포하자, 나신 아기를 찬양했던 것을 깨닫게 하옵소서. 우리를 위하여 구주가 나셨음에 반가워 예수님께 영광을 드렸음을 배우기 원합니다. 멸망당할 죄인을 위하여 독생자를 보내주신 하나님의 계획을 찬양합니다.
경배를 원하시는 하나님,
목자들이 아기 예수님의 탄생을 통해서 누렸던 은혜를 저의 아이도 경험하기를 소망합니다. 목자들이 예수님께 영광을 드리게 하셨듯이 저의 아이도 성탄절을 맞이하면서 이미 오신 예수님께 영광을 드리게 하옵소서. 성탄절의 기쁨을 즐거워하며 하나님께 찬양의 예배를 드리게 하옵소서.
하나님 아버지,
우리를 구원하시므로 새 생명을 지으신 하나님께 감사드립니다. 그런데 마음으로는 날마다 하나님을 찬양하기 원하였으나, 지금은 죄로 얼룩진 자신의 모습 밖에 없으니 용서해 주옵소서. 주님, 감사합니다. 묵은 날은 가버렸습니다. 그들의 생명에 새로운 날을 주셨으니, 지금 맞이한 새날을 주님 앞에서 성실한 일꾼으로 살도록 이끌어 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"오 땅이여","To":"온 땅이여"},{"From":"일방 중에","To":"열방 중에"},{"From":"목자들 의","To":"목자들의"},{"From":"하읍소서","To":"하옵소서"},{"From":"위하~ 구주가","To":"위하여 구주가"},{"From":"말 망당할","To":"멸망당할"},{"From":"찬양합니다 경배를","To":"찬양합니다. / 경배를"},{"From":"드리거 하음소서","To":"드리게 하옵소서"},{"From":"하울소서","To":"하옵소서"},{"From":"그런 데","To":"그런데"},{"From":"얼룩 진","To":"얼룩진"},{"From":"주읍소서","To":"주옵소서"},{"From":"가버렸습니다 그들의","To":"가버렸습니다. 그들의"},{"From":"주섰으니","To":"주셨으니"},{"From":"새날음","To":"새날을"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1212';
