-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/14 (1편)
-- 2026-11-18 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0214', N'하나님의 위로를 바라보게 하옵소서', N'롬 15:9',
 N'이방인으로 그 긍휼하심을 인하여 하나님께 영광을 돌리게 하려 하심이라 기록된바 이러므로 내가 열방 중에서 주께 감사하고 주의 이름을 찬송하리로다 함과 같으니라',
 N'하나님 아버지,
저의 아이에게 인간의 고통에 대하여 안타깝게 여기는 마음을 주옵소서. 그리하여 마취제를 발견하여 아픔의 고통을 덜어주려 했던 제임스 심슨의 마음을 닮게 하옵소서. 심슨이 하나님의 사랑을 품고 마취제를 발견하기까지 고생을 다했던 것처럼, 저의 아이도 주님의 뜻을 나타내기 위해서 소원을 품고 노력하기를 소망합니다. 심슨의 하나님께서 저의 아이에게도 똑같은 은혜로 역사하시기를 간절히 바랍니다.
위로자가 되시는 하나님,
주님의 평안이 약속되어 있음을 믿고 두려워하지 말게 하옵소서. 혹시 잠깐의 근심이 있겠으나 초조해 하지 않고, 하나님의 위로를 바라보게 하옵소서. 머리털까지 세신 바가 되신 주님의 은혜가 그들의 심령에 덮여지기 원합니다. 회복의 은혜를 주시는 여호와의 손길을 보게 하옵소서.
감사의 하나님,
저의 아이가 작년보다도 오른 학업의 성적으로 인하여 감사드리게 하옵소서. 올해에는 성령님의 도우심으로 공부를 잘 하였고, 지식에 있어서 많이 성장하였음을 은혜로 여기게 하옵소서. 성령님의 깨닫게 하시며, 기억하게 하시는 은혜가 공부를 잘 하도록 하게 하셨음을 믿습니다. 이에, 올해의 감사 예배에는 이 모든 은혜가 하나님으로 말미암았음을 고백하게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"금출하십울","To":"긍휼하심을"},{"From":"영광은 돌리끼","To":"영광을 돌리게"},{"From":"하려 하십이라","To":"하려 하심이라"},{"From":"기육된바 이 러므로","To":"기록된바 이러므로"},{"From":"일방 중에서","To":"열방 중에서"},{"From":"갑사하고","To":"감사하고"},{"From":"이름은 찬송하리로다","To":"이름을 찬송하리로다"},{"From":"(몸 15:9)","To":"롬 15:9"},{"From":"하나님아버지,","To":"하나님 아버지,"},{"From":"주옵소 서","To":"주옵소서"},{"From":"답게","To":"닮게"},{"From":"저의아 이에게도","To":"저의 아이에게도"},{"From":"똑같은은혜로","To":"똑같은 은혜로"},{"From":"하 옵소서","To":"하옵소서"},{"From":"덮여지 기","To":"덮여지기"},{"From":"성렁님의","To":"성령님의"},{"From":"있어 서","To":"있어서"},{"From":"은해로","To":"은혜로"},{"From":"하육소서","To":"하옵소서"},{"From":"깨단게","To":"깨닫게"},{"From":"은폐가","To":"은혜가"},{"From":"하도목","To":"하도록"},{"From":"기도드립니다.아멘,","To":"기도드립니다. 아멘."}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0214';
