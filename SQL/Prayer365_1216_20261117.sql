-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/16 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1216', N'대강절에 죄를 고백하게 하옵소서', N'사 9:6',
 N'이는 한 아기가 우리에게 났고 한 아들을 우리에게 주신바 되었는데 그 어깨에는 정사를 메었고 그 이름은 기묘자라, 모사라, 전능하신 하나님이라, 영존하시는 아버지라, 평강의 왕이라 할 것임이라',
 N'영광중에 계신 하나님,
대강절 셋째 주일을 맞이해서 하나님을 영화롭게 해드리는 저의 아이를 축복합니다. 하늘에서 계획하신 구원의 일을 이 땅에서 이루신 하나님의 섭리를 즐거워하게 하시니 감사를 드립니다. 그가 성탄절을 기다리면서 이 세상을 향하신 하나님의 사랑을 좀 더 깊게 배우게 하심을 좋아합니다.
성탄의 하나님,
자기 백성을 저희의 죄로부터 구원해 주시려고 메시야를 약속하셨습니다. 그 약속을 이루셔서 예수님이 아기로 오셨으니, 구원을 이루신 하나님의 자비하심을 깨닫게 하옵소서. 소망이 없이 지내던 유대 사람들에게 빛이 비취게 하신 은혜를 깨닫도록 하옵소서. 그 거룩하신 은혜가 저의 아이로 하여금 주님을 그리스도로 믿게 하셨습니다.
하나님 아버지,
처음 성탄절에 동방의 박사들이 황금, 몰약, 유향을 아기 예수님께 드렸던 것처럼, 저희들은 대강절에 지은 죄를 고백하고 용서를 구하도록 이끌어 주옵소서. 심판의 주님께서 오시는 그날에, 저희들은 영원한 생명으로 다시 살아나게 하옵소서. 저희들의 심령에 성령의 충만함이 역사하여, 빛으로 오신 주님을 기억하는 대강절의 믿음을 나타내 보이게 하시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"사 9:C","To":"사 9:6"},{"From":"우리에비","To":"우리에게"},{"From":"평강 의","To":"평강의"},{"From":"할것임이라","To":"할 것임이라"},{"From":"아이 를","To":"아이를"},{"From":"하나 님의","To":"하나님의"},{"From":"기다리 면서","To":"기다리면서"},{"From":"좋아 합니다","To":"좋아합니다"},{"From":"약속하셨습 니다","To":"약속하셨습니다"},{"From":"하 나님의","To":"하나님의"},{"From":"사람들에 게빛이","To":"사람들에게 빛이"},{"From":"깨단도록","To":"깨닫도록"},{"From":"저 의아이로","To":"저의 아이로"},{"From":"하나님아버지","To":"하나님 아버지,"},{"From":"박시들이","To":"박사들이"},{"From":"드 졌던","To":"드렸던"},{"From":"저회들은","To":"저희들은"},{"From":"주유소서,","To":"주옵소서."},{"From":"심관의","To":"심판의"},{"From":"생 명으로","To":"생명으로"},{"From":"실아나게","To":"살아나게"},{"From":"하옵소서, 저희들의","To":"하옵소서. 저희들의"},{"From":"충만합이","To":"충만함이"},{"From":"역사 하여,빛으로","To":"역사하여, 빛으로"},{"From":"민음율","To":"믿음을"},{"From":"하 시기를","To":"하시기를"},{"From":"아벤. 으","To":"아멘."},{"From":"365","To":"(쪽 번호 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1216';
