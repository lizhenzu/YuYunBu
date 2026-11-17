-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/30 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1230', N'새해를 바라보게 하옵소서', N'시 126:2',
 N'그 때에 우리 입에는 웃음이 가득하고 우리 혀에는 찬양이 찼었도다 열방 중에서 말하기를 여호와께서 저희를 위하여 대사를 행하셨다 하였도다',
 N'하나님 아버지,
크고 놀라우신 은혜로 살아왔음에 감사를 드립니다. 하나님의 은혜가 지나온 한 해 동안에 저의 아이와 함께 해 주셨음에 찬양을 드립니다. 금년의 마지막 주일을 맞아 저의 아이가 하나님의 행하심을 묵상하고, 찬양을 드리기 원합니다. 하나님은 저의 아이의 편이 되셔서 복된 삶을 살게 하셨습니다.
크신 하나님,
저의 가족을 살피시고, 부족한 저의 간구에 응답해 주셨음을 기뻐합니다. 서툴기 그지없고, 때로는 믿음이 부족한 상태에서 간구했음에도 다 응답해 주셨습니다. 저의 아이가 하나님의 도우심으로 살았음을 고백하게 하옵소서. 아이의 생애에 놀라운 일을 하셨던 그 손길에 감사하면서 새해를 바라보게 하옵소서.
인자하신 하나님,
새해를 주시는 하나님을 찬양합니다. 저의 아이가 새롭게 되어, 새해에는 사랑이 메마른 곳에서 주님의 인자하심을 드러내는 삶을 다짐하게 하옵소서. 온갖 미혹된 말들이 넘쳐나는 세상에서 복음을 외치는 삶을 다짐하게 하옵소서. 비록 외롭고, 친구들로부터 따돌려진다 할지라도 소금과 빛으로 살도록 이끌어 주옵소서. 세상의 타락으로 죄를 짓는 이들을 위해 기도하기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"휴12월 30일","To":"12월 30일"},{"From":"찾었도다","To":"찼었도다"},{"From":"말하기 를","To":"말하기를"},{"From":"하나님아버지","To":"하나님 아버지"},{"From":"금 년의","To":"금년의"},{"From":"찬 양을","To":"찬양을"},{"From":"살 게하셨습니다","To":"살게 하셨습니다"},{"From":"크신하나님","To":"크신 하나님"},{"From":"기뻐합니 다","To":"기뻐합니다"},{"From":"서둘기","To":"서툴기"},{"From":"고백하 게","To":"고백하게"},{"From":"채해 에는","To":"새해에는"},{"From":"소 금과","To":"소금과"},{"From":"이들 을","To":"이들을"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1230';
