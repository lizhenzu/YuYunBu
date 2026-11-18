-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/21 (1편)
-- 2026-11-18 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0221', N'하나님의 만져주심을 기대하게 하옵소서', N'느 1:11',
 N'주여 구하오니 귀를 기울이사 종의 기도와 주의 이름을 경외하기를 기뻐하는 종들의 기도를 들으시고 오늘날 종으로 형통하여 이 사람 앞에서 은혜를 입게 하옵소서 하였나니 그 때에 내가 왕의 술 관원이 되었었느니라',
 N'입다의 하나님,
오늘은 저의 아이가 입다가 하나님만 의지하고 바라보면서 성숙한 사람으로 세워졌던 은혜를 묵상하기 원합니다. 입다를 만드신 하나님의 열심이 저의 아이를 만져 주시기 원합니다. 비록 기생의 아들로 태어났으나 하나님께서 함께 해 주셨고, 형제들로부터는 외면당하고 쫓겨나는 신분이었으나 하나님은 그를 버리지 않으셨던 은혜를 받은 사실을 깨닫게 하옵소서. 저의 아이에게도 입다가 누렸던 은혜를 경험하게 하옵소서.
하나님 아버지,
입다의 믿음이 저의 아이의 것이 되어서 사람은 자신을 버려도 하나님께서는 자신과 함께 하신다는 믿음으로 살게 하옵소서. 아울러 입다가 암몬 족속과의 전쟁에서 수고를 다하여 승리를 거두었을 때, 그 영광을 여호와께 돌렸던 것을 본받게 하옵소서. 주님께서 받으셔야 하는 영광을 가로채는 교만함이 없게 하시기를 소망합니다.
전능하신 하나님,
바라기는, 이 고요한 순간으로부터 흐르는 사랑의 빛과 성령께서 주시는 기쁨이 충만하기 원합니다. 공부하는 학생으로서 수업에 충실하고 근면을 나타내게 하소서. 저의 기도와 사랑으로 말미암아 교실에 평화가 넘치기를 원합니다. 오늘도 하루의 삶에서 예수님의 향기를 드러내도록 은혜를 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"기 도를","To":"기도를"},{"From":"하읍소서","To":"하옵소서"},{"From":"되였었느니라","To":"되었었느니라"},{"From":"사 람으로","To":"사람으로"},{"From":"열 심이","To":"열심이"},{"From":"태어났으 나","To":"태어났으나"},{"From":"신 분이었으나","To":"신분이었으나"},{"From":"하나님아버지, (문단 끝에 붙음)","To":"하나님 아버지, (호칭 줄로 옮김)"},{"From":"하나님 께서는","To":"하나님께서는"},{"From":"주시 는","To":"주시는"},{"From":"근 면을","To":"근면을"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0221';
