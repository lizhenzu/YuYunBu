-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/21 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1221', N'성탄의 복을 이웃과 나누게 하옵소서', N'벧후 1:10-11',
 N'그러므로 형제들아 더욱 힘써 너희 부르심과 택하심을 굳게 하라 너희가 이것을 행한즉 언제든지 실족지 아니하리라 이같이 하면 우리 주 곧 구주 예수 그리스도의 영원한 나라에 들어감을 넉넉히 너희에게 주시리라',
 N'경배의 하나님,
아기 예수님께서 세상에 나셨을 때, 동방에서 찾아온 박사들이 예물을 드렸던 것처럼, 저의 아이도 자신을 예물로 드리게 하옵소서. 이번 성탄절에는 자신의 몸을 예수님께 선물하는 은혜를 내려 주옵소서. 성탄절의 감사로 헌금을 준비함도 은혜이지만, 자신을 하나님께 드리는 체험이 있기를 소망합니다.
열납하시는 하나님,
저의 아이가 자신을 여호와께 드림을 소망하게 하옵소서. 하나님 아버지께 의의 병기로 드리게 하옵소서. 오직 그 자신을 죽은 자 가운데서 다시 산 자같이 하나님께 드리도록 이끌어 주옵소서. 하나님 아버지께 종으로 드려 거룩함에 이르도록 이끌어 주옵소서.
오메가의 하나님,
저의 아이가 성탄절을 맞이하면서 다시 오시는 주님을 기다리게 하옵소서. 주님의 다시 오심은 덫과 같이 갑자기 닥쳐온다고 하셨으니 저의 아이가 주님의 재림을 기다리게 하옵소서. 그러므로 세속적인 즐거움이나 방탕한 생활을 좇지 않게 해 주시기 원합니다. 항상 기도하고 깨어서 지내게 하옵소서. 기도로 깨어있는 생활을 하는 가운데 주님을 맞이하게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"벤후","To":"벧후"},{"From":"너히","To":"너희"},{"From":"택하심율 굴비 하라","To":"택하심을 굳게 하라"},{"From":"나라 에","To":"나라에"},{"From":"성탄 절에는","To":"성탄절에는"},{"From":"있 기를","To":"있기를"},{"From":"아버 지께","To":"아버지께"},{"From":"다 시산자같이","To":"다시 산 자같이"},{"From":"종 으로","To":"종으로"},{"From":"주옵소서. 0이0","To":"주옵소서."},{"From":"저의 아기가","To":"저의 아이가"},{"From":"하을 소서,","To":"하옵소서."},{"From":"덧과 같이","To":"덫과 같이"},{"From":"닥처온다고","To":"닥쳐온다고"},{"From":"하읍소서","To":"하옵소서"},{"From":"즐거움이 나","To":"즐거움이나"},{"From":"좋지 않게","To":"좇지 않게"},{"From":"하음소서,","To":"하옵소서."},{"From":"생환을","To":"생활을"},{"From":"370 자녀를 위한 865일 축복 기도문","To":"(쪽 머리글 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1221';
