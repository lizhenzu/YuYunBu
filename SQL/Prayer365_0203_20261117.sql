-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/3 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0203', N'죄에 대하여 슬퍼하게 하옵소서', N'사 38:19',
 N'오직 산 자 곧 산 자는 오늘날 내가 하는 것과 같이 주께 감사하며 주의 신실을 아비가 그 자녀에게 알게 하리이다',
 N'사랑의 하나님,
지금, 저의 아이의 죄를 고백합니다. 하나님께서는 그를 사랑하셔서, 지키고 따라야 하는 말씀을 주셨으나, 말씀에 따르지 못했던 지난 생활을 회개합니다. 유혹에 이끌리고, 욕심으로 말미암아 죄를 지으며 살았습니다. 참으로 뉘우치오니, 하나님의 인자하심으로 용서하여 주옵소서. 죄인을 의인으로 만드는 힘 있는 주님의 피로, 새로워지기 원합니다. 몸도, 마음도, 생각도, 영도 새롭게 해 주시는 주님의 은총을 받게 하소서.
용서하시는 하나님,
저의 아이가 때때로 주님을 잊고 지내는 시간들에 대하여 회개하기를 소망합니다. 하나님의 도우심이 없이 제 마음대로 했던 죄를 뉘우치게 하옵소서. 슬퍼하는 자가 복이 있다고 하셨으니 죄에 대하여 슬퍼하고, 하나님을 영화롭게 해드리지 못했음을 슬퍼하게 하옵소서.
예배를 받으시는 하나님,
저의 아이가 주 여호와의 이름을 높여드리며 예배드리는 시간을 사모하기를 소망합니다. 주님의 날이 세상으로부터 구별되고, 하나님께 영광을 드리는 시간이기를 바라는 마음을 주옵소서. 하늘의 하나님께는 영화로움을 드리고 주님께서 세상을 향해서 복을 주시는 예배를 기다리게 하옵소서. 이를 위해서 오늘 밤은 거룩히 보내게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"그자녀에게","To":"그 자녀에게"},{"From":"생활 을","To":"생활을"},{"From":"살았 습니다","To":"살았습니다"},{"From":"몸 도,마음도","To":"몸도, 마음도"},{"From":"하읍소서","To":"하옵소서"},{"From":"에배드리는","To":"예배드리는"},{"From":"사모 하기를","To":"사모하기를"},{"From":"영광 울","To":"영광을"},{"From":"바리는","To":"바라는"},{"From":"영화 로움을","To":"영화로움을"},{"From":"에배를","To":"예배를"},{"From":"하 읍소서","To":"하옵소서"},{"From":"거륙히","To":"거룩히"},{"From":"헤 주시기를","To":"해 주시기를"},{"From":"에 수님의","To":"예수님의"},{"From":"49","To":"(쪽 번호 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0203';
