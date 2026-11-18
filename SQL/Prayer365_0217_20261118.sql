-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/17 (1편)
-- 2026-11-18 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0217', N'여호와의 생각에 예민하게 하옵소서', N'시 118:25',
 N'여호와여 구하옵나니 이제 구원하소서 여호와여 우리가 구하옵나니 이제 형통케 하소서',
 N'하나님 아버지,
저의 아이가 오늘도 거룩한 삶으로 이끄시는 하나님을 바라기를 소망합니다. 모세에게 떨기 나무의 불꽃 가운데 나타나셔서 이스라엘 백성들을 인도하라 하셨던 것처럼 그에게도 하나님 앞에서 살아가야 할 삶을 보여 주옵소서. 여호수아에게 말씀으로 찾아오셔서 담대하라 하셨던 것처럼 그에게도 어떠한 삶을 살아야 하는지 말씀해 주옵소서.
비전의 하나님,
저의 아이가 아직은 어리고, 깊은 영의 세계를 몰라도, 하나님의 인도하심을 바라보기 원합니다. 하나님께서 자신에게 계획을 갖고 계심에 주목하게 하옵소서. 여호와의 생각에 예민함을 보이게 하옵소서. 바울에게는 마게도냐 사람을 환상 가운데서 보게 하셨던 것처럼 삶의 거룩한 소명에 대한 환상을 보여 주시어 소망 가운데 주님의 사람으로 살아가게 하옵소서.
예배를 받으시는 하나님,
저의 아이가 여호와의 이름을 높여드리며 예배드리는 시간을 사모하기를 소망합니다. 주님의 날이 세상으로부터 구별되고, 하나님께 영광을 드리는 시간이기를 바라게 하옵소서. 하늘의 하나님께 산제사를 드리는 복된 시간의 예배를 기다리게 하옵소서. 이를 위해서 오늘 밤은 거룩히 보내게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"여혼와여","To":"여호와여"},{"From":"구하움나너","To":"구하옵나니"},{"From":"하소 A","To":"하소서"},{"From":"( 118:25)","To":"시 118:25"},{"From":"하나님 아버지.","To":"하나님 아버지,"},{"From":"소망 합니다","To":"소망합니다"},{"From":"멸기 나무","To":"떨기 나무"},{"From":"나타나서서","To":"나타나셔서"},{"From":"백성들 을","To":"백성들을"},{"From":"것 처럼","To":"것처럼"},{"From":"그에계도","To":"그에게도"},{"From":"인도 하심을","To":"인도하심을"},{"From":"주 목하게","To":"주목하게"},{"From":"하읍소서","To":"하옵소서"},{"From":"바울에게 는","To":"바울에게는"},{"From":"소 명에","To":"소명에"},{"From":"하움소서","To":"하옵소서"},{"From":"사모하 기를","To":"사모하기를"},{"From":"하읍소서,","To":"하옵소서."},{"From":"하음소서","To":"하옵소서"},{"From":"63","To":"(쪽 번호 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0217';
