-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/23 (1편)
-- 2026-11-18 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0223', N'오늘도 말씀으로 시작하게 하옵소서', N'애 2:19',
 N'밤 초경에 일어나 부르짖을찌어다 네 마음을 주의 얼굴 앞에 물 쏟듯 할찌어다 각 길머리에서 주려 혼미한 네 어린 자녀의 생명을 위하여 주를 향하여 손을 들찌어다 하였도다',
 N'하나님 아버지,
저의 아이가 하나님을 따르겠다는 결단으로 아침을 맞이하기를 소망합니다. 여호와의 은혜를 기다리면서 오늘을 시작하게 하옵소서. 하나님의 뜻을 구할 때, 그의 인생을 복되게 하심을 믿습니다. 저의 아이가 평안하고, 기쁘게 살기를 원하오니, 하나님을 찾게 해 주옵소서. 만일, 여호와를 의지하고, 바랄 때, 약속된 복을 누림을 믿습니다.
사랑의 하나님,
오늘 하루의 삶도 하나님의 말씀으로 시작하게 하옵소서. 아직 분별력이 약하고, 지혜가 부족하니, 지금 그에게 필요한 것은 하나님의 말씀입니다. 말씀의 지혜를 따르는 생활이 하나님 보시기에 좋게 된다는 것을 믿게 하옵소서. 이스라엘 백성들이 십계명을 사랑했던 것처럼, 오늘도 그에게 주시는 말씀을 귀중히 여기는 믿음을 주옵소서.
자비로우신 하나님,
저의 아이가 복의 주인공이기 원합니다. 하나님의 말씀에 순종하고, 그 뜻에 따라 살기를 최고의 소원으로 삼게 하옵소서. 영생을 약속해 주시고, 번성케 해 주심을 선언하신 그 은혜를 온전히 누리게 하옵소서. 인생을 위해서 예비하신 복을 경험하게 하옵소서. 그래서 푸른 하늘에서 조각 구름이 흐르는 것을 보기만 해도 하나님의 위대하심을 찬양하기 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"-밤 초경에","To":"밤 초경에"},{"From":"부르짓을찌어다","To":"부르짖을찌어다"},{"From":"길머 리에서","To":"길머리에서"},{"From":"하였도 다","To":"하였도다"},{"From":"(애2.19)","To":"애 2:19"},{"From":"소망 합니다","To":"소망합니다"},{"From":"하나님 의뜻을","To":"하나님의 뜻을"},{"From":"복 되게","To":"복되게"},{"From":"평 안하고","To":"평안하고"},{"From":"여 호와를","To":"여호와를"},{"From":"사랑의하나님,","To":"사랑의 하나님,"},{"From":"분별력 이약하고","To":"분별력이 약하고"},{"From":"말씀입 니다","To":"말씀입니다"},{"From":"하읍소서","To":"하옵소서"},{"From":"그뜻에","To":"그 뜻에"},{"From":"주 시고","To":"주시고"},{"From":"인 생을","To":"인생을"},{"From":"원 하면서","To":"원하면서"},{"From":"아멘,","To":"아멘."}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0223';
