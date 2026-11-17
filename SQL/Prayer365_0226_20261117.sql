-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/26 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0226', N'주님의 백성으로 세워가도록 하옵소서', N'출 3:12',
 N'하나님이 가라사대 내가 정녕 너와 함께 있으리라 네가 백성을 애굽에서 인도하여 낸 후에 너희가 이 산에서 하나님을 섬기리니 이것이 내가 너를 보낸 증거니라',
 N'복의 근원이신 하나님,
한 주간의 생활을 시작하면서 주님의 이름으로 저의 아이를 축복합니다. 이 주간의 하루하루에 그의 생애를 빛나게 하시려고 예비해 두신 복을 허락하옵소서. 하나님의 사람으로 거룩하게 자신을 세워가도록 하옵소서. 그리고 마귀로 틈을 타지 못하도록 지켜 주시기 원합니다.
고백의 하나님,
네 입의 말로 네가 얽힌다는 말씀에 주의하여 언제나 하늘에 소망을 둔 말을 하게 하옵소서. 자신을 낙심으로 처하게 하는 어리석은 말은 피하게 하시고, 혹시 남들로부터 부정적인 말을 들었을 때는 즉시 거절하게 하옵소서. 땅에서 묶으면 하늘에서도 매어 주신다고 하셨으니, 입술의 선포로 저주를 묶는 담대함을 주시기 원합니다. 자신을 향해서 복을 선포하게 하옵소서.
사랑을 원하시는 하나님,
저의 아이에게 이웃을 주셨음을 감사드립니다. 이웃을 향하여 마음을 열고, 손을 내미는 친절함을 통해서 하나님의 사랑이 전해지기를 원합니다. 그에게 여호와의 사랑을 전하시도록 이웃을 보게 하셨고, 함께 지내게 하셨음을 믿게 하옵소서. 그가 이웃을 존귀히 여기게 하옵소서. 하나님께로부터 사랑을 받고 있는 존재로서의 이웃에 대한 생각을 품게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"하나넘이","To":"하나님이"},{"From":"애급에서","To":"애굽에서"},{"From":"인도하여 번 에","To":"인도하여 낸 후에"},{"From":"복의 근원이신 하나님, (본문과 붙음)","To":"복의 근원이신 하나님, (호칭 줄로 나눔)"},{"From":"축복합니 다","To":"축복합니다"},{"From":"복 을","To":"복을"},{"From":"하옵 소서","To":"하옵소서"},{"From":"마크로","To":"마귀로"},{"From":"고백의 하나님, (본문과 붙음)","To":"고백의 하나님, (호칭 줄로 나눔)"},{"From":"피 하게","To":"피하게"},{"From":"거절하 게","To":"거절하게"},{"From":"하읍소서","To":"하옵소서"},{"From":"입술 의","To":"입술의"},{"From":"선포하게 하옵소서 사랑을 원하시는 하나님,","To":"선포하게 하옵소서. / 사랑을 원하시는 하나님, (호칭 줄로 나눔)"},{"From":"원합니 다","To":"원합니다"},{"From":"합께","To":"함께"},{"From":"지내 게","To":"지내게"},{"From":"하나 님께로부터","To":"하나님께로부터"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0226';
