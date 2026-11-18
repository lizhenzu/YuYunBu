-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/13 (1편)
-- 2026-11-18 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0213', N'주님을 향한 열정을 체험하게 하옵소서', N'시 147:13',
 N'저가 네 문빗장을 견고히 하시고 너의 가운데 자녀에게 복을 주셨으며',
 N'자비로우신 하나님,
저의 아이가 하나님의 사랑을 확인하기를 소망합니다. 주님의 보호하심과 지켜주심에 대하여 확실히 믿고 담대하게 하옵소서. 요즈음 막연한 불안감으로 두려워하지 않기를 원합니다. 이유도 알 수 없는 불안으로 잠을 제대로 이루지 못하는 아이를 도와주옵소서. 불확실함에서 오는 두려움으로 말미암아 흔들리고 있으니 성령님의 은혜를 바라봅니다.
지켜 주시는 하나님,
저의 아이에게 주님을 향한 열렬함을 체험하게 하옵소서. 뜨겁게 하나님을 사랑함으로써 두려움을 극복하게 하옵소서. 학교에서도 수업에 집중하지 못한 채로 멍하니 있다가 돌아오는 경우도 있으니, 주님의 평안을 허락해 주시기를 소망합니다. 아이의 영혼을 불쌍히 여겨주시고, 평안의 은혜를 내려 주옵소서.
사랑의 하나님,
그의 심령의 빗장을 단단히 닫아 주옵소서. 하나님의 말씀을 믿지 못하게 하고, 여호와의 도우심을 믿지 못하게 하는 불신의 영을 물리쳐 주옵소서. 저의 아이가 하나님께 집중하지 않음으로써 일어나는 분열일 뿐이오니, 주님을 향한 마음을 뜨겁게 하옵소서. 그의 생각과 말이 주님을 사랑하는 것으로 시작되게 하옵소서. 기도에 더욱 힘쓰므로 확신을 누리게 해주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"BON","To":"(날짜 앞 잡글자 — 뺌)"},{"From":"0 (구절 앞)","To":"(잡글자 — 뺌)"},{"From":"보호하 심과","To":"보호하심과"},{"From":"불학실함","To":"불확실함"},{"From":"두 려움","To":"두려움"},{"From":"지켜 주시는 하나님","To":"지켜 주시는 하나님,"},{"From":"하나 님을","To":"하나님을"},{"From":"집 중하지","To":"집중하지"},{"From":"평안 을","To":"평안을"},{"From":"평 안의은혜를","To":"평안의 은혜를"},{"From":"주움소서","To":"주옵소서"},{"From":"사랑의 하나님","To":"사랑의 하나님,"},{"From":"못 하게","To":"못하게"},{"From":"물리처 주 옵소서","To":"물리쳐 주옵소서"},{"From":"뿐 이노니","To":"뿐이오니"},{"From":"하읍소서","To":"하옵소서"},{"From":"하움소서","To":"하옵소서"},{"From":"누리 게","To":"누리게"},{"From":"에수님의","To":"예수님의"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0213';
