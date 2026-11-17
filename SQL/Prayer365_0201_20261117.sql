-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/1 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0201', N'주님의 뜻을 새기게 하옵소서', N'계 7:17',
 N'이는 보좌 가운데 계신 어린 양이 저희의 목자가 되사 생명수 샘으로 인도하시고 하나님께서 저희 눈에서 모든 눈물을 씻어 주실 것임이러라',
 N'믿음의 하나님,
저의 아이에게 하나님을 섬기는 믿음을 주셨음에 감사드립니다. 하나님을 경외하는 것을 제일로, 최고의 일로 여기는 마음을 주옵소서. 예수님의 보혈의 공로에 의해서 하나님을 아버지로 부르게 되었음을 찬양하기 원합니다. 주님의 이름만 불러도 그의 마음이 즐거워지게 하옵소서.
살아계신 하나님,
오직 의인은 믿음으로 말미암아 살 것을 바라시는 주님의 뜻을 마음에 새기게 하옵소서. 아주 사소한 일, 친구들과 잡담을 즐길지라도 믿는 자의 도리에서 떠나지 말게 하옵소서. 사람들로부터는 뒤떨어져 보이고, 별난 사람처럼 여겨질지라도 믿음의 표준이 행동의 근거가 되게 하옵소서. 하나님의 말씀이 선택의 기준이 되기 원합니다. 세상에서는 인정받지 못하더라도 하나님께서 인정하시는 삶을 선택하게 하옵소서.
이기게 하시는 하나님,
저의 아이가 새롭게 한 달의 삶을 시작하면서 하나님께 마음을 두게 하옵소서. 주님의 말씀이 그의 심령에 풍성히 거하기를 소망합니다. 그 말씀으로 사탄의 공격을 물리치게 하옵소서. 자신의 욕심도 거절하는 힘이 있게 하옵소서. 죄의 유혹이 강하게 몰려와도 하나님의 말씀의 권세로 이기게 하옵소서. 주님의 이름으로 악한 일들을 물리치는 역사를 보게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"(예계 7:17)","To":"계 7:17"},{"From":"하나님 께서","To":"하나님께서"},{"From":"씨어","To":"씻어"},{"From":"것입미러라","To":"것임이러라"},{"From":"하나 님을","To":"하나님을"},{"From":"예수 님의","To":"예수님의"},{"From":"찬양하 기원합니다","To":"찬양하기 원합니다"},{"From":"살아계신 하나님 (문단 끝에 붙음)","To":"살아계신 하나님, (호칭 줄로 옮김)"},{"From":"믿는 자 의","To":"믿는 자의"},{"From":"하옵소 서","To":"하옵소서"},{"From":"인정받 지못하더라도","To":"인정받지 못하더라도"},{"From":"하읍소서","To":"하옵소서"},{"From":"이기게 하시는 하나님, 을 소문은","To":"이기게 하시는 하나님, (잡글자 뺌)"},{"From":"새롭계","To":"새롭게"},{"From":"하음소서","To":"하옵소서"},{"From":"힘 이","To":"힘이"},{"From":"하음소서,","To":"하옵소서."},{"From":"권세 로","To":"권세로"},{"From":"보 게","To":"보게"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0201';
