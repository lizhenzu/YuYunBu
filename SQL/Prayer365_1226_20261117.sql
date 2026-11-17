-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/26 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1226', N'아버지의 사랑을 즐거워하게 하옵소서', N'시 66:8-9',
 N'만민들아 우리 하나님을 송축하며 그 송축 소리로 들리게 할찌어다 그는 우리 영혼을 살려 두시고 우리의 실족함을 허락지 아니하시는 주시로다',
 N'동방박사들의 하나님,
오늘은 저의 아이가 밤하늘의 별을 보고 구주를 찾아 나선 동방박사들의 은혜를 묵상하기 원합니다. 큰 별의 나타남으로 새 왕이 나셨음을 깨닫고 산을 넘고 물을 건너서 별을 따라 길을 나선 그들의 행동을 배우게 하옵소서. 새 왕께 드릴 예물을 준비하고 경배하러 나선 그들의 믿음을 깨닫기를 소망합니다.
영광을 받으실 하나님,
새로 나신 왕을 찾아 예배한 동방박사들의 믿음을 저의 아이가 경험하게 하옵소서. 그들이 예수님께 예물을 드리게 하신 하나님의 손길이 저의 아이에게도 은혜로 나타나게 하옵소서. 예수님을 구주로 영접하여 예배하는 기쁨을 누리게 해 주시기를 구합니다. 하나님이신 예수님의 오심으로 아버지의 사랑이 저희에게 나타난 것을 즐거워하게 하옵소서.
거룩함의 하나님,
신랑으로 이 땅에 주님이 오실 때, 저의 아이가 신부로서의 모습을 갖추게 하옵소서. 신랑을 기다리며 살아가는 믿음의 순결함을 지니게 하옵소서. 우리 주 예수님께서 강림하실 때 기쁨으로 맞이하게 하옵소서. 온전히 거룩한 신부의 자격으로 신랑을 맞게 하옵소서. 말과 행동이 거룩하고 옳은 행실을 가짐으로 신부의 단장인 세마포를 준비하는 삶을 살게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"송출 소리로","To":"송축 소리로"},{"From":"들리고 할찌어다","To":"들리게 할찌어다"},{"From":"영손윤 살 려","To":"영혼을 살려"},{"From":"실죽함을","To":"실족함을"},{"From":"동방박사들 의","To":"동방박사들의"},{"From":"깨 닫고","To":"깨닫고"},{"From":"하읍소서","To":"하옵소서"},{"From":"받으실하나님","To":"받으실 하나님"},{"From":"경험하 게","To":"경험하게"},{"From":"저 의아이에게도","To":"저의 아이에게도"},{"From":"예 배하는","To":"예배하는"},{"From":"오심 으로","To":"오심으로"},{"From":"갖 추게 하읍소서,","To":"갖추게 하옵소서."},{"From":"실아가는","To":"살아가는"},{"From":"지니게하읍 소서","To":"지니게 하옵소서"},{"From":"예수님께 강립하실","To":"예수님께서 강림하실"},{"From":"하움소서","To":"하옵소서"},{"From":"온전 허","To":"온전히"},{"From":"거룩하 고","To":"거룩하고"},{"From":"아민.","To":"아멘."},{"From":"375","To":"(쪽 번호 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1226';
