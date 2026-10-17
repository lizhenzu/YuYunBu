-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/9 (1편)
-- 2026-10-11 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1209', N'메시야의 은혜를 맛보게 하옵소서', N'시 119:12-14',
 N'찬송을 받으실 여호와여 주의 율례를 내게 가르치소서 주의 입의 모든 규례를 나의 입술로 선포하였으며 내가 모든 재물을 즐거워함 같이 주의 증거의 도를 즐거워하였나이다',
 N'사랑의 하나님,
성탄절을 기다리는 저희들에게 복된 시간이 되게 하옵소서. 저의 아이가 예수님의 오심이 주는 기쁨을 누리기를 소망합니다. 그 옛날, 억눌려 있던 사람들에게 아기 예수님이 기쁨이 되어주셨던 것처럼, 성탄절을 기다리면서 저의 아이에게 메시야의 은혜를 맛보게 하옵소서.
여호와 하나님,
대강절 둘째 주일을 맞이했으니, 생명의 길이 되어 주신 주님을 찬미하는 은혜를 허락하옵소서. 저희의 길을 인도하시는 등불로 오신 예수님을 생각하면서 주님을 기대하게 하옵소서. 성령님의 깨닫게 하시는 감화로 예수님을 더욱 깊이 알아가는 시간을 갖도록 이끌어 주옵소서. 이미 그 옛날에 오신 예수님의 성탄절을 기다리면서 우리의 가는 빛이 되어주신 예수님을 바라보게 하옵소서.
위로의 하나님,
저의 아이가 하나님을 사랑하면서 살아가기에는 어려움도 많습니다. 그의 마음을 약하게도 하고, 두렵게도 합니다. 그런 어려움이 있지만 주님의 빛이 그에게 생명의 길을 열어주시옵소서. 낙심하게 하는 세상을 바라보지 말고, 소망을 주시는 하나님을 기대하게 하옵소서. 성탄의 약속을 이루신 하나님께서 재림의 약속을 이루심을 믿게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"12원 9일","To":"12월 9일"},{"From":"여혼와여","To":"여호와여"},{"From":"율레를","To":"율례를"},{"From":"내끼","To":"내게"},{"From":"규레문","To":"규례를"},{"From":"입술 로","To":"입술로"},{"From":"줄거워함","To":"즐거워함"},{"From":"중거의","To":"증거의"},{"From":"줄거워하었나이다","To":"즐거워하였나이다"},{"From":"() 112:12-19)","To":"시 119:12-14"},{"From":"하읍소서","To":"하옵소서"},{"From":"아이 가","To":"아이가"},{"From":"기 다리면서","To":"기다리면서"},{"From":"찬미 하는","To":"찬미하는"},{"From":"예수님 을","To":"예수님을"},{"From":"감회로","To":"감화로"},{"From":"하음소서","To":"하옵소서"},{"From":"많습니다 그의","To":"많습니다. 그의"},{"From":"두럽게도","To":"두렵게도"},{"From":"주 님의","To":"주님의"},{"From":"열어주시유소서","To":"열어주시옵소서"},{"From":"익 속을 이부신","To":"약속을 이루신"},{"From":"재립의","To":"재림의"},{"From":"헤주시기를 원해 면서","To":"해 주시기를 원하면서"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1209';
