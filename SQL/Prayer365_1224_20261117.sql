-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/24 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1224', N'하나님께 소망의 근거를 두게 하옵소서', N'마 2:9-10',
 N'박사들이 왕의 말을 듣고 갈새 동방에서 보던 그 별이 문득 앞서 인도하여 가다가 아기 있는 곳 위에 머물러 섰는지라 저희가 별을 보고 가장 크게 기뻐하고 기뻐하더라',
 N'복의 근원이신 하나님,
성탄의 아침을 맞이하는 주간에 저의 아이를 축복합니다. 주님의 이름으로 저의 아이를 축복할 때, 그를 위하여 예비해 두신 복을 허락하옵소서. 아기 예수님을 찾아 경배를 드렸던 동방의 박사들과 같은 영예를 누리게 하옵소서. 성탄절의 영광을 하나님께 드리게 하옵소서.
기다림의 하나님,
성탄절 전날, 기다림의 훈련을 받기를 원합니다. 오랫동안 메시야를 기다렸던 유대 백성들에게 구주가 오신 것처럼, 저의 아이들도 기다림의 은혜를 누리는 가운데 응답의 복을 받게 하옵소서. 아브라함이 오래 기다려 아들을 보았던 것처럼, 한나가 오래 기다려 아들을 낳은 것처럼, 오래 기다릴 줄 아는 사람이 되게 하옵소서. 그 기다림을 통하여 하나님을 더욱 의지하고, 하나님께 소망의 근거를 두도록 이끌어 주옵소서.
충성의 하나님,
저의 아이가 주님 앞에서 자신의 삶을 계산하는 날을 기다리게 하옵소서. 주님께서 회계하자 하실 때, 내어놓을 것이 많이 있기를 소망합니다. 이제껏 사는 동안에 가족을 사랑하고, 가정을 위해서 살았던 삶을 보여드리게 하옵소서. 나아가 학생의 신분으로 사는 시간에 열심히 공부한 삶을 보여드리게 하옵소서. 언제라도 다시 오실 주님을 기다리는 날들이 되게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"갈색","To":"갈새"},{"From":"그. 별이","To":"그 별이"},{"From":"기머하더라","To":"기뻐하더라"},{"From":"복의 근원이신 하나님.","To":"복의 근원이신 하나님,"},{"From":"이름 으로","To":"이름으로"},{"From":"허락하옵소 서","To":"허락하옵소서"},{"From":"누 리게","To":"누리게"},{"From":"웅답의","To":"응답의"},{"From":"기 다려","To":"기다려"},{"From":"오 래기다릴","To":"오래 기다릴"},{"From":"충성의 하나님","To":"충성의 하나님,"},{"From":"하음소 서","To":"하옵소서"},{"From":"보여 드리게","To":"보여드리게"},{"From":"하읍소서","To":"하옵소서"},{"From":"시는 시간에","To":"사는 시간에"},{"From":"아민. 기","To":"아멘."},{"From":"373","To":"(쪽 번호 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1224';
