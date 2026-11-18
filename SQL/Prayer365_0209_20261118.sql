-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/9 (1편)
-- 2026-11-18 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0209', N'이웃에게 그리스도를 나타내게 하옵소서', N'신 20:1',
 N'네가 나가 대적과 싸우려할 때에 말과 병거와 민중이 너보다 많음을 볼찌라도 그들을 두려워 말라 애굽 땅에서 너를 인도하여 내신 네 하나님 여호와께서 너와 함께 하시느니라',
 N'고마우신 하나님,
믿음의 가정에서 태어난 어린 생명에게 복을 주심에 감사드립니다. 저의 아이들이 살아가는 시간 동안에 하늘에서부터 임하는 복을 누리게 하옵소서. 주 하나님의 보호해주심과 함께 하심으로 기쁨과 감사가 넘치는 생활이 되게 하시고, 그 마음에 원하는 것들을 다 이룸으로써 하나님을 의뢰하는 만족함을 누리게 하옵소서.
인자하신 하나님,
저의 아이가 주님의 인자하심으로 이웃에게 사랑으로 다가가 그리스도를 나타내게 하옵소서. 세상에 대하여 두려워하는 마음을 갖지 않게 해 주시고, 불신자 친구에게도 똑같이 사랑을 함으로써 주님의 제자로 살기를 원합니다. 불신자들은 멍에를 같이 할 수는 없지만, 저희로 말미암아 하나님의 사랑을 받아야 할 대상임을 잊지 않게 하옵소서.
하나님 아버지,
저의 아이에게 주님의 겸손을 자기의 것으로 삼게 하옵소서. 그 겸손으로 하나님을 섬기게 하시고, 말씀에 순종하도록 이끌어 주옵소서. 그 겸손으로 보내어진 마음을 품고 이웃을 섬기게 하옵소서. 주님께서 제자들의 발을 씻기셨던 겸손함을 본받아 친구들을 섬기고, 이웃을 위하여 봉사하는 자세로 사랑하게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"두 려워","To":"두려워"},{"From":"애급 땅","To":"애굽 땅"},{"From":"하시느니 라","To":"하시느니라"},{"From":"저 의아이들이","To":"저의 아이들이"},{"From":"하 옵소서","To":"하옵소서"},{"From":"이룹으로써","To":"이룸으로써"},{"From":"인자하신하나님,","To":"인자하신 하나님,"},{"From":"그리스 도를","To":"그리스도를"},{"From":"말미 암아","To":"말미암아"},{"From":"하나님아버지,","To":"하나님 아버지,"},{"From":"하읍소서","To":"하옵소서"},{"From":"겸손 으로","To":"겸손으로"},{"From":"주읍소서","To":"주옵소서"},{"From":"이웃을 넘기게","To":"이웃을 섬기게"},{"From":"제자 들의","To":"제자들의"},{"From":"셋기셨던","To":"씻기셨던"},{"From":"겹손합을","To":"겸손함을"},{"From":"기 도드립니다.아멘.","To":"기도드립니다. 아멘."}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0209';
