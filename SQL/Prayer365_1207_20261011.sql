-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/7 (1편)
-- 2026-10-11 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1207', N'모든 이들에게 복음이 전해지게 하옵소서', N'대상 16:35',
 N'너희는 이르기를 우리의 구원의 하나님이여 우리를 구원하여 만국 가운데서 건져 내시고 모으시사 우리로 주의 성호를 감사하며 주의 영예를 찬양하게 하소서 할찌어다',
 N'영광의 하나님,
우리를 위해서 구주가 나셨음을 기뻐합니다. 대강절 기간 동안에 저의 아이가 메시야를 기다렸던 이들이 아기 예수님을 영접하는 은혜를 받았음을 기억하게 하옵소서. ''자기들의 죄로부터 구원해 주실 분''으로 예수님을 사모했기에 새로 나신 아기께 경배를 드릴 수 있었음을 깨닫게 하옵소서.
하나님 아버지,
아기 예수님의 나심을 즐거워하는 저의 아이가 특별한 기도의 시간을 갖게 하옵소서. 주님의 오심이 가난한 자들에게 복음이 전해지게 하려 하심이셨듯이, 가난한 이들을 위해서 기도의 시간을 갖기를 소망합니다. 그가 정성을 다하여 돈을 마련해 가난한 이들에게 구제할 수 있게도 하옵소서. 하나님의 사랑이 몸으로 오셨던 것처럼 저의 아이도 주님의 사랑을 몸으로 나타낼 수 있는 영광의 시간을 갖게 하옵소서.
인자하신 하나님,
저의 아이가 성탄절이 주님을 믿는 자들만의 축제가 아님을 알기를 원합니다. 포로된 자에게 자유를, 눈 먼 자에게 다시 보게 함을 위해서 주님이 오셨습니다. 어려움에 있는 이들을 위해 기도하게 하옵소서. 이미 오신 예수님을 아직도 영접하지 못한 사람들을 위해 기도하는 시간을 갖게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"주의 성화를","To":"주의 성호를"},{"From":"찬양하기 하소서","To":"찬양하게 하소서"},{"From":"대 상 16:35)","To":"대상 16:35)"},{"From":"영광의 하나님 우리를","To":"영광의 하나님, 우리를"},{"From":"받았 음을","To":"받았음을"},{"From":"하읍소서","To":"하옵소서"},{"From":"분'' 으로","To":"분''으로"},{"From":"예수 님을","To":"예수님을"},{"From":"하 옵소서","To":"하옵소서"},{"From":"하움소서","To":"하옵소서"},{"From":"졸음이","To":"복음이"},{"From":"소망합니다 그가","To":"소망합니다. 그가"},{"From":"사 랑을","To":"사랑을"},{"From":"하음소서","To":"하옵소서"},{"From":"원 합니다","To":"원합니다"},{"From":"주 님이","To":"주님이"},{"From":"미 오신","To":"이미 오신"},{"From":"갖 게","To":"갖게"},{"From":"기도드립니다.아멘","To":"기도드립니다. 아멘."}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1207';
