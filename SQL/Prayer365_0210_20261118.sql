-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/10 (1편)
-- 2026-11-18 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0210', N'주님의 품에서 자라가게 하옵소서', N'시 35:27',
 N'나의 의를 즐거워하는 자로 기꺼이 부르고 즐겁게 하시며 그 종의 형통을 기뻐하시는 여호와는 광대하시다 하는 말을 저희로 항상 하게 하소서',
 N'전능하신 하나님,
저의 아이를 사랑하시고, 저의 생활에 일일이 간섭하시며, 바른 길로 이끌어 주신 은혜에 감사드립니다. 아무 것도 모르던 어린 시절부터 하나님이 그의 아버지가 되심을 알게 하시고, 믿음 안에서 지내왔음에 감사드립니다. 영광을 받으실 전능하신 하나님을 가까이 해드리게 하옵소서. 여호와의 이름을 마음에서부터 높여드리게 하옵소서.
찬양 중에 계신 하나님,
아직도 주변에는 하나님을 아버지로 알지 못한 채로 살아가는 교만한 이들이 많이 있는데, 주님의 은혜로 하나님을 섬겨왔음을 감사드립니다. 더욱 더 주님의 품에서 자라가게 하옵소서. 하나님의 은혜를 찬양하는 샘이 풍성하게 하옵소서. 하나님의 도우심을 찬양하는 샘이 넘쳐나게 하옵소서. 하나님의 인도하심으로 즐겁게 지냄을 찬양하게 하옵소서.
예배를 받으시는 하나님,
저의 아이가 주 여호와의 이름을 높여드리며 예배드리는 시간을 사모하기를 소망합니다. 주님의 날이 세상으로부터 구별되고, 하나님께 영광을 드리는 시간이기를 바라는 마음을 주옵소서. 하늘의 하나님께는 영화로움을 드리고 주님께서 세상을 향해서 복을 주시는 예배를 기다리게 하옵소서. 이를 위해서 오늘 밤은 거룩히 보내게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"줄겹게","To":"즐겁게"},{"From":"그.종의","To":"그 종의"},{"From":"기뻐하시는여 호와는","To":"기뻐하시는 여호와는"},{"From":"전능하신 하나님, (본문과 붙음)","To":"전능하신 하나님, (호칭 줄로 나눔)"},{"From":"하 나님이","To":"하나님이"},{"From":"감 사드립니다","To":"감사드립니다"},{"From":"하옵소 서","To":"하옵소서"},{"From":"하 옵소서","To":"하옵소서"},{"From":"사모 하기를","To":"사모하기를"},{"From":"영광 을","To":"영광을"},{"From":"주읍소서","To":"주옵소서"},{"From":"영화 로움을","To":"영화로움을"},{"From":"보내게 하옵소서. 원하면서","To":"보내게 해 주시기를 원하면서 (1/20·1/27 같은 문단 기준)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0210';
