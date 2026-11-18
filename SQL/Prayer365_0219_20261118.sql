-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/19 (1편)
-- 2026-11-18 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0219', N'소망의 말을 하는 입술을 갖게 하옵소서', N'레 11:45',
 N'나는 너희의 하나님이 되려고 너희를 애굽 땅에서 인도하여 낸 여호와라 내가 거룩하니 너희도 거룩할찌어다',
 N'복의 근원이신 하나님,
한 주간의 생활을 시작하면서 주님의 이름으로 저의 아이를 축복합니다. 하나님을 마음에 두고 지내게 하옵소서. 주님을 가까이 함이 그에게 복스러운 시간이 되기를 소망합니다. 여호와의 얼굴을 바라보는 심령에 생명의 빛이 들어옴을 느끼게 하시고, 그 빛줄기가 생기를 갖게 하는 경험을 하게 하옵소서.
이루시는 하나님,
주님의 은혜는 저의 아이로 하여금 하늘의 소망을 두게 하심을 믿습니다. 소망의 말을 하는 입술을 갖기 원합니다. 자기 자신을 향해서 소망을 주는 말씀을 증거하게 하옵소서. 친구들에게도 주님의 이름으로 소망의 말로 나누게 하옵소서. 그리하여 상심한 친구에게는 용기를 북돋아주고, 연약한 친구에게는 힘을 갖도록 해주며, 낙심 중에 있는 친구에게는 희망을 선물하게 하옵소서.
위로의 하나님,
저의 아이의 오늘까지 하나님은 진실로 인자하셨습니다. 이스라엘 백성들을 애굽에서 빼어내셨던 것처럼, 저의 아이가 주님의 손길로 인도되고 있음을 느끼게 하시니 참으로 감사드립니다. 주님께서 생명의 말씀으로 위로해 주셨으니, 이 말씀을 마음에 새기면서 소망을 품게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"너최의","To":"너희의"},{"From":"애급 땅","To":"애굽 땅"},{"From":"여초와라","To":"여호와라"},{"From":"레 11: 45","To":"레 11:45"},{"From":"축복합니 다","To":"축복합니다"},{"From":"경 험을","To":"경험을"},{"From":"AL","To":"(잡글자 — 뺌)"},{"From":"이루시는 하나님","To":"이루시는 하나님,"},{"From":"믿습니 다","To":"믿습니다"},{"From":"중거하게","To":"증거하게"},{"From":"북돈아주고","To":"북돋아주고"},{"From":"나심 중에","To":"낙심 중에"},{"From":"회 망을","To":"희망을"},{"From":"백 성들을","To":"백성들을"},{"From":"인도되 고","To":"인도되고"},{"From":"말씀으 로","To":"말씀으로"},{"From":"주시기 를","To":"주시기를"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0219';
