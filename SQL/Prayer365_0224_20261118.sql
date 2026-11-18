-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/24 (1편)
-- 2026-11-18 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0224', N'베풀어 주신 사랑에 감사드리게 하옵소서', N'사 55:11',
 N'내 입에서 나가는 말도 헛되이 내게로 돌아오지 아니하고 나의 뜻을 이루며 나의 명하여 보낸 일에 형통하리라',
 N'전능하신 하나님,
저의 아이를 축복합니다. 추운 겨울을 보내는 동안, 건강하게 지내게 하시고, 주님을 더욱 깊게 알아가게 하셨음을 즐거워합니다. 길게 여겨졌던 방학을 보낸 저의 아이가 새 학기를 맞이합니다. 방학 동안에도 그의 공부와 삶을 간섭해 주신 은혜를 기억합니다. 주님께서 베풀어 주신 사랑에 찬양으로 감사드리며 살기 원합니다.
하나님 아버지,
제가 보건대, 방학 기간을 통하여 새로운 시작을 하지 못하였음을 뉘우치게 하옵소서. 세월을 아끼는 지혜가 부족하여 시간을 낭비했음을 깨닫기 원합니다. 시간을 아꼈어야 하는데, 쓸데없이 보냈던 날들에 대하여 회개합니다. 게으르게 지내다가 새 학기를 맞이함을 용서해 주옵소서. 새 학기에는 열심히 살아가게 하시고, 규모 있게 지내게 하옵소서.
예배를 받으시는 하나님,
저의 아이가 주 여호와의 이름을 높여드리며 예배드리는 시간을 사모하기를 소망합니다. 주님의 날이 세상으로부터 구별되고, 하나님께 영광을 드리는 시간이기를 바라는 마음을 주옵소서. 하늘의 하나님께는 영화로움을 드리고 주님께서 세상을 향해서 복을 주시는 예배를 기다리게 하옵소서. 이를 위해서 오늘 밤은 거룩히 보내게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"찾되이","To":"헛되이"},{"From":"내끼로","To":"내게로"},{"From":"사 55.:11","To":"사 55:11"},{"From":"전능하신 하나님, (본문과 붙음)","To":"전능하신 하나님, (호칭 줄로 나눔)"},{"From":"여겨 졌던","To":"여겨졌던"},{"From":"그 의","To":"그의"},{"From":"하나님아버지,","To":"하나님 아버지,"},{"From":"뉘 우치게","To":"뉘우치게"},{"From":"깨 닫기","To":"깨닫기"},{"From":"대하 여","To":"대하여"},{"From":"주옵소 서","To":"주옵소서"},{"From":"예배를 받으시는 하나님, (본문과 붙음)","To":"예배를 받으시는 하나님, (호칭 줄로 나눔)"},{"From":"사모 하기를","To":"사모하기를"},{"From":"영광 을","To":"영광을"},{"From":"주읍소서","To":"주옵소서"},{"From":"영화 로움을","To":"영화로움을"},{"From":"에배를","To":"예배를"},{"From":"기다리게하 음소서","To":"기다리게 하옵소서"},{"From":"거록히","To":"거룩히"},{"From":"헤 주시기를","To":"해 주시기를"},{"From":"에 수님의","To":"예수님의"},{"From":"70 자녀를 의히","To":"(쪽 머리글 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0224';
