-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/6 (1편)
-- 2026-10-11 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1206', N'넉넉한 은총을 부어 주옵소서', N'벧후 3:10',
 N'그러나 주의 날이 도적 같이 오리니 그 날에는 하늘이 큰 소리로 떠나가고 체질이 뜨거운 불에 풀어지고 땅과 그 중에 있는 모든 일이 드러나리로다',
 N'크리스마스의 하나님,
영생을 주시려고 예수님을 세상에 보내주셨음에 감사드립니다. 저의 아이가 주님께서 나신 날을 기다릴 때, 하나님의 구원에 대한 계획에 주의를 기울이기를 소망합니다. 크리스마스를 통해서 인류를 구하시려는 하나님의 자비로우심을 묵상하게 하옵소서. 구원을 원하시는 사랑에 순종하여 예수님의 다스리심을 받게 하옵소서.
하나님 아버지,
때때로 저의 아이가 주님의 다스리심을 거절했던 죄를 위하여 대신 용서를 구합니다. 하나님의 인도하심에 주목하지 못하고 자기의 생각에 따라 살아온 결과, 실패할 수밖에 없었습니다. 그러나 우리에게는 언제나 주님의 섭리에 따라 정해 주신 성장의 분량이 있음을 믿습니다. 하나님의 지혜와 그 능력보다 나 자신을 믿고 앞날을 계획할 때 실패와 헛수고와 무의미가 자신을 기다리게 되는 것을 깨닫게 해 주옵소서.
자비로우신 하나님,
저의 아이가 성탄절을 기다릴 때, 한낱 교회의 축제일로 기다리지 않게 하옵소서. 예수님을 구주로 영접하고, 주님의 다스리심을 받기를 결단하는 중에 이 날을 기다리기를 소망합니다. 실패자의 모습으로 있는 그를 하나님께서 위로하시고 넉넉한 은총을 부어 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":".주의 날이","To":"주의 날이"},{"From":"떠나 가고","To":"떠나가고"},{"From":"뜨게 운 불","To":"뜨거운 불"},{"From":"벤춘 3:10)","To":"벧후 3:10)"},{"From":"크리스마스의하나님","To":"크리스마스의 하나님"},{"From":"주 의를","To":"주의를"},{"From":"순 종하여","To":"순종하여"},{"From":"하나님아버지","To":"하나님 아버지"},{"From":"용 서를","To":"용서를"},{"From":"따 라","To":"따라"},{"From":"결과.","To":"결과,"},{"From":"언제니","To":"언제나"},{"From":"법리에","To":"섭리에"},{"From":"하나님 의 지혜","To":"하나님의 지혜"},{"From":"와무의미가","To":"와 무의미가"},{"From":"깨단게","To":"깨닫게"},{"From":"않 게","To":"않게"},{"From":"예수님음","To":"예수님을"},{"From":"결 단하는","To":"결단하는"},{"From":"예 수님의","To":"예수님의"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1206';
