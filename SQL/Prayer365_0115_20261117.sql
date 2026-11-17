-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 01/15 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0115', N'여호와의 은혜로 지혜롭게 하옵소서', N'롬 12:3',
 N'내게 주신 은혜로 말미암아 너희 중 각 사람에게 말하노니 마땅히 생각할 그 이상의 생각을 품지 말고 오직 하나님께서 각 사람에게 나눠주신 믿음의 분량대로 지혜롭게 생각하라',
 N'자비로우신 하나님,
한 주간의 생활을 시작하면서 주님의 이름으로 저의 아이를 축복합니다. 그를 위하여 예비해 두신 복을 허락하옵소서. 저의 아이가 주님의 숨결을 느끼기 원합니다. 교실에 앉아서 문득 바라본 하늘의 푸름에서 하나님의 오묘하심을 깨닫게 하옵소서. 하늘을 향해 자라는 나무들을 볼 때, 자연에 나타난 하나님의 손길을 알게 하옵소서. 만물에 나타난 하나님을 보게 하옵소서.
만물의 주 하나님,
문득 깨닫는 하나님의 모습 때문에 가슴이 벅차오르는 경험을 누리게 하옵소서. 무엇을 보던지 거기에서 하나님을 알려했던 토마스 아퀴나스의 마음을 갖게 하옵소서. 그리하여 하나님을 아는 지식으로 저의 아이가 지혜롭게 되기를 소망합니다.
이 시간에, 저희 자신의 이기심에 사로잡혀 이웃을 사랑하지 않은 죄를 다 아뢰고 고백하오니 저희의 죄와 허물을 용서하여 주시옵소서. 저희의 이 모든 죄를 기억하지 마옵시고 주님만이 가진 정결로 저희를 정결케 하시며 주님 보시기에 기쁘고 선한 것만을 행하도록 인도해 주옵소서. 주님의 빛으로 저희를 비추사 평안 중에 거하게 하시며 저희가 전능하신 손으로 보호를 받을 수 있도록 은혜를 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"지혜롭게 하읍소서","To":"지혜롭게 하옵소서"},{"From":"너히 중각 사람에 말하노너","To":"너희 중 각 사람에게 말하노니"},{"From":"그. 이상의 생 각을","To":"그 이상의 생각을"},{"From":"각 사람에체 나뉘주신","To":"각 사람에게 나눠주신"},{"From":"지비흡께 생각 하라","To":"지혜롭게 생각하라"},{"From":"자비로우신 하나님, 한 주간의","To":"자비로우신 하나님, / 한 주간의"},{"From":"축복합니 다","To":"축복합니다"},{"From":"허락하옵소서 저의 아이가","To":"허락하옵소서. 저의 아이가"},{"From":"않아서","To":"앉아서"},{"From":"바 라본","To":"바라본"},{"From":"향 해자리는","To":"향해 자라는"},{"From":"때,자연에","To":"때, 자연에"},{"From":"하읍소 서","To":"하옵소서"},{"From":"만물의주 하나님, 문득","To":"만물의 주 하나님, / 문득"},{"From":"깨단는","To":"깨닫는"},{"From":"아퀴나스 의","To":"아퀴나스의"},{"From":"하읍소서","To":"하옵소서"},{"From":"아이 가지혜롭게","To":"아이가 지혜롭게"},{"From":"죄 를","To":"죄를"},{"From":"저 회의","To":"저희의"},{"From":"마읍시고","To":"마옵시고"},{"From":"주옵소 서","To":"주옵소서"},{"From":"전능 하신","To":"전능하신"},{"From":"은헤를","To":"은혜를"},{"From":"30 자녀를 위한 365일 축복 기도문","To":"(쪽 머리글 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0115';
