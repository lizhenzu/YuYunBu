-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/15 (1편)
-- 2026-11-18 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0215', N'마음의 귀를 주님께로 두게 하옵소서', N'신 7:19',
 N'네 하나님 여호와께서 너를 인도하여 내실 때에 네가 목도한 큰 시험과 이적과 기사와 강한 손과 편 팔을 기억하라 그와 같이 네 하나님 여호와께서 네가 두려워하는 모든 민족에게 행하실 것이요',
 N'삶을 인도해 주시는 하나님,
저의 아이가 마음의 귀를 주님께로 기울이게 하옵소서. 그를 이끄시려고 주신 생명의 말씀을 가까이 하게 하옵소서. 규칙적으로 성경을 읽는 가운데 하나님만을 섬기는 사람으로 자랐던 요한 깔뱅의 마음을 저의 아이도 지니기를 소망합니다. 깔뱅을 닮아 성경에서 믿음의 원리를 발견하고, 회개의 은혜를 자신의 것으로 하게 하옵소서.
"너희 안에서 행하시는 이는 하나님이시니 자기의 기쁘신 뜻을 위하여 너희로 소원을 두고 행하게 하시나니"라고 약속하셨음을 감사드립니다. 이 약속이 저의 아이에게도 이루어져서, 인생의 소원을 품기 원합니다. 살아도 주를 위하여 살고, 죽어도 주를 위하여 죽는 인생의 비전을 허락해 주실 것을 믿습니다. 그가 무엇을 하면서 주님께서 세상에 다시 오시는 날까지 살아야 하는지 밝히 보여 주옵소서.
사랑을 원하시는 하나님,
하나님을 사랑하는 저의 아이가 이웃을 사랑하기를 소망합니다. 교실 안에서 우정을 두텁게 하게 하시며, 섬겨야 할 친구들에게 사랑의 손을 내어밀게 하옵소서. 친구에게 가서 돕기를 원합니다. 힘겨워하는 친구를 도와줌으로써 함께 지내도록 하신 하나님의 사랑을 향유하게 하옵소서. 만일, 공부를 어려워하는 친구가 있다면 자신이 아는 것을 가르쳐 주게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"목도한 근 시험","To":"목도한 큰 시험"},{"From":"여훈와께서","To":"여호와께서"},{"From":"민 적에게","To":"민족에게"},{"From":"이끄시려 고주신","To":"이끄시려고 주신"},{"From":"저의 아 이도","To":"저의 아이도"},{"From":"발견하 고","To":"발견하고"},{"From":"위하 여너희로","To":"위하여 너희로"},{"From":"감사드립니 다","To":"감사드립니다"},{"From":"원합니 다","To":"원합니다"},{"From":"하나님 ,","To":"하나님,"},{"From":"하읍소서","To":"하옵소서"},{"From":"힘겨위하는","To":"힘겨워하는"},{"From":"61","To":"(쪽 번호 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0215';
