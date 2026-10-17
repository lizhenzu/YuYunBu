-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/8 (1편)
-- 2026-10-11 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1208', N'심령적으로 주님께 깨어있게 하옵소서', N'살전 1:3',
 N'너희의 믿음의 역사와 사랑의 수고와 우리 주 예수 그리스도에 대한 소망의 인내를 우리 하나님 아버지 앞에서 쉬지 않고 기억함이니',
 N'고마우신 하나님,
성탄절의 축제를 준비하느라 자칫 들뜨기 쉽지만, 지난 11개월의 생활을 돌이켜 보면서 감사의 기도를 하게 하옵소서. 금년 첫 날부터 주 하나님의 간섭을 통하여 소망의 시간들을 보낼 수 있었음을 기뻐하게 하옵소서. 이번 성탄절에는 자신을 만져 주시고, 여기에까지 이끌어 주신 하나님의 은혜를 찬미하는 예배를 드리도록 이끌어 주옵소서.
결단하게 하시는 하나님,
저의 아이에게 깨어있는 믿음의 생활을 하게 하옵소서. 별을 보던 박사들이었기에 큰 별이 나타난 것을 보았듯이, 저의 아이가 심령적으로 주님께 깨어있기를 소망합니다. 믿음으로 깨어서 주님의 영광을 구하게 하시고, 사랑으로 깨어서 주님의 손길을 나타내게 하시며, 소망으로 깨어서 하나님의 나라를 바라보게 하옵소서.
예배를 받으시는 하나님,
저의 아이가 주 여호와의 이름을 높여드리며 예배드리는 시간을 사모하기를 소망합니다. 주님의 날이 세상으로부터 구별되고, 하나님께 영광을 드리는 시간이기를 바라는 마음을 주옵소서. 하늘의 하나님께는 영화로움을 드리고 주님께서 세상을 향해서 복을 주시는 예배를 기다리게 하옵소서. 이를 위해서 오늘 밤은 거룩히 보내게 하옵소서. 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"침령적으로","To":"심령적으로"},{"From":"너히의","To":"너희의"},{"From":"우르 하나님","To":"우리 하나님"},{"From":"자칭 들뜨기","To":"자칫 들뜨기"},{"From":"11 개월의 생 활을","To":"11개월의 생활을"},{"From":"하움소서","To":"하옵소서"},{"From":"주하 나님의","To":"주 하나님의"},{"From":"하음 소서","To":"하옵소서"},{"From":"하 나님의","To":"하나님의"},{"From":"주읍소서","To":"주옵소서"},{"From":"하읍소서","To":"하옵소서"},{"From":"박 사들","To":"박사들"},{"From":"깨 어서","To":"깨어서"},{"From":"사모 하기를","To":"사모하기를"},{"From":"영광 을","To":"영광을"},{"From":"영화 로움","To":"영화로움"},{"From":"에배를","To":"예배를"},{"From":"히 옵소서","To":"하옵소서"},{"From":".이를","To":"이를"},{"From":"거록히","To":"거룩히"},{"From":"하옵소시","To":"하옵소서"},{"From":"이름 으로","To":"이름으로"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1208';
