-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/11 (1편)
-- 2026-10-11 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1211', N'친구들에게 기쁨이 되게 하옵소서', N'눅 1:34-35',
 N'마리아가 천사에게 말하되 나는 사내를 알지 못하니 어찌 이 일이 있으리이까 천사가 대답하여 가로되 성령이 네게 임하시고 지극히 높으신 이의 능력이 너를 덮으시리니 이러므로 나실 바 거룩한 자는 하나님의 아들이라 일컬으리라',
 N'신실하신 하나님,
금년에도 저의 아이의 인생을 복되게 하셨음에 감사드립니다. 그의 기도를 들으시고, 마음에 원하는 바를 이루어 주셨으며, 하나님 앞에서 소망을 품게 하시니 영광을 받아주옵소서. 주님의 은혜로 형통케 하셔서 하나님 앞에서나 세상의 일들 앞에서 주님의 모습을 닮아 증인된 삶을 살게 하심을 기뻐합니다.
사랑의 하나님,
이제, 저의 아이가 하나님 앞에서 소원을 품기 원합니다. 주님께서 그에게 짊어지게 하신 십자가를 지고 세상 안에서 살아가게 하옵소서. 하나님의 심부름꾼으로 살아가기를 소망합니다. 매일의 생활 현장에서 사귀는 친구들에게 기쁨이 되게 하옵소서. 그들이 저의 아이를 만난 사실만으로도 기쁨을 누림이 되기 원합니다. 우울하거나 낙심이 될 때, 저의 아이로 말미암아 어두움의 그림자가 사라지기를 소망합니다.
교회공동체의 하나님,
저의 아이가 교회 안에서 섬기는 종이 되게 하옵소서. 교회의 지체들을 섬기면서 주님의 사랑을 나누도록 이끌어 주옵소서. 주님의 품에서 형제의 사랑을 나누게 하시고, 기도를 통하여 돕는 자리에 서게 하옵소서. 교회의 교역자들을 존경하고, 선생님들의 가르침을 잘 따르게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"이일이","To":"이 일이"},{"From":"대 답하여","To":"대답하여"},{"From":"이러 므로","To":"이러므로"},{"From":"나실바","To":"나실 바"},{"From":"기 도를 들으시고.","To":"기도를 들으시고,"},{"From":"받아주읍소서","To":"받아주옵소서"},{"From":"중인된","To":"증인된"},{"From":"그 에게","To":"그에게"},{"From":"히 나님의","To":"하나님의"},{"From":"실아가기를","To":"살아가기를"},{"From":"사 귀는","To":"사귀는"},{"From":"사실 만으로도","To":"사실만으로도"},{"From":"우울하거니","To":"우울하거나"},{"From":"때.","To":"때,"},{"From":"저으 아이로","To":"저의 아이로"},{"From":"소망합니다 교회공동체의","To":"소망합니다. / 교회공동체의"},{"From":"지체들 을 넘기면서","To":"지체들을 섬기면서"},{"From":"하울소 서","To":"하옵소서"},{"From":"해 주 시기를","To":"해 주시기를"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1211';
