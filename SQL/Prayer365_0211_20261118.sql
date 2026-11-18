-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/11 (1편)
-- 2026-11-18 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0211', N'예배만이 있는 주일이 되게 하옵소서', N'느 2:20',
 N'내가 대답하여 가로되 하늘의 하나님이 우리로 형통케 하시리니 그의 종 우리가 일어나 건축하려니와 오직 너희는 예루살렘에서 아무 기업도 없고 권리도 없고 명록도 없다 하였느니라',
 N'예배를 받으시는 하나님,
주님의 날을 구별하여 지키는 저의 아이를 축복합니다. 이 날에 예비하신 은혜를 풍성하게 누리게 하옵소서. 그를 위하여 준비해 놓으신 신령한 복을 자기의 것으로 하게 하옵소서. 하나님의 이름을 부르는 것을 즐거워하는 가운데, 주일을 주일답게 지내기를 소망합니다.
거룩하신 하나님,
오늘 하루 주 하나님의 이름에 합당한 영광을 드리는 것에 집중할 수 있도록 이끌어 주옵소서. 공부라든가, 취미활동 등의 한 주간의 생활 속에서 있었던 일들은 잠시 접어두게 하옵소서. 소제를 받으시고, 번제를 거두시는 하나님께 자신을 바치는 예배만이 있는 주일이 되게 하옵소서. 오늘의 예배로 말미암은 은혜를 받고 하나님께서 주시는 전신갑주를 입음이 체험되게 하옵소서.
세상으로 보내시는 하나님,
예배를 드림으로써 시작된 하루를 온전히 거룩하게 보내게 하옵소서. 저의 아이가 예배를 마치고 예배당을 나설 때, 세상으로 보내지는 은혜를 경험하게 하옵소서. 하나님께서 사랑하시는 세상으로 보내져 복음을 전하고, 예수님이 하나님의 아들이심을 증거하려는 결단을 하게 하옵소서. 주님의 제자로서 세상을 향하여 축복하는 입술을 갖게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"#2월 11일 #","To":"(날짜 앞뒤 잡글자 — 뺌)"},{"From":"하 였느니라","To":"하였느니라"},{"From":"예배를 받으시는 하나님, 300","To":"예배를 받으시는 하나님, (잡글자 뺌)"},{"From":"예비 하신","To":"예비하신"},{"From":"신 령한","To":"신령한"},{"From":"거룩하신하나님,","To":"거룩하신 하나님,"},{"From":"하루주 하나님의","To":"하루 주 하나님의"},{"From":"집중할수","To":"집중할 수"},{"From":"속 에서","To":"속에서"},{"From":"입 음이","To":"입음이"},{"From":"은혜 를","To":"은혜를"},{"From":"하읍소서","To":"하옵소서"},{"From":"중거하려는","To":"증거하려는"},{"From":"하옵소 서","To":"하옵소서"},{"From":"아멘","To":"아멘."}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0211';
