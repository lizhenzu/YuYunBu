-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/22 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1222', N'예수님을 참 맘으로 예배하게 하옵소서', N'눅 2:15',
 N'천사들이 떠나 하늘로 올라가니 목자가 서로 말하되 이제 베들레헴까지 가서 주께서 우리에게 알리신바 이 이루어진 일을 보자 하고',
 N'찬양의 하나님,
흰눈으로 덮인 세상처럼 저의 아이의 영혼이 깨끗하기를 소망합니다. 그가 자신의 심령 깊은 데서 하나님을 찬양하려는 마음이 샘솟게 하시고, 구원의 주님께 찬송을 드리게 하옵소서. 주님의 사랑을 받는 저의 아이가 찬양과 시를 통해서 하나님을 경배하는 삶을 살아가게 하옵소서. 하나님의 거룩하심을 사모하면서 흠이 없이 지내도록 이끌어 주옵소서.
신실하신 하나님,
저의 아이에게 늘 의지의 대상이 되어 주신 하나님께 영광을 드립니다. 찬양으로 말미암아 감사와 영광을 여호와께만 돌리게 하옵소서. 이러한 찬양을 통하여, 그가 자신을 발견할 수 있게 도와주시고, 자신의 신앙표현을 하나님께 고백하는 기회를 갖도록 이끌어 주옵소서. 주님께 찬양을 드리면서 신앙의 균형적인 성장을 도모하게 하옵소서. 올해 1년 동안에 지식적으로, 신체적으로 성장한 것만큼 신앙적으로 자라게 하옵소서.
예배를 받으시는 주여,
주일을 맞이하는 저의 아이의 심령이 주님의 음성을 들을 수 있고, 주님의 손길을 느끼기 위해서 조용한 시간을 갖도록 이끌어 주옵소서. 흥미의 만족을 채우려고 즐기는 것들을 절제할 수 있는 은혜를 내려 주옵소서. 그러한 절제 속에 조용한 시간을 갖고 주님께 마음을 두도록 이끌어 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"눈 2:15","To":"눅 2:15"},{"From":"베들레함","To":"베들레헴"},{"From":"우 리에게","To":"우리에게"},{"From":"찬양의 하나님, 흰눈으로","To":"찬양의 하나님, / 흰눈으로"},{"From":"하시 고","To":"하시고"},{"From":"구원이 주님께","To":"구원의 주님께"},{"From":"하읍소서","To":"하옵소서"},{"From":"아 이가","To":"아이가"},{"From":"도외주시고","To":"도와주시고"},{"From":"신앙표 현을","To":"신앙표현을"},{"From":"하움소서","To":"하옵소서"},{"From":"주 님의","To":"주님의"},{"From":"주움소서","To":"주옵소서"},{"From":"홍 미의","To":"흥미의"},{"From":"은해를 내러 주읍 소서","To":"은혜를 내려 주옵소서"},{"From":"이끝 어","To":"이끌어"},{"From":"371","To":"(쪽 번호 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1222';
