-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/20 (1편)
-- 2026-11-18 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0220', N'공부할 때, 성령님의 도우심을 구하게 하옵소서', N'전 7:14',
 N'형통한 날에는 기뻐하고 곤고한 날에는 생각하라 하나님이 이 두 가지를 병행하게 하사 사람으로 그 장래 일을 능히 헤아려 알지 못하게 하셨느니라',
 N'은혜로우신 하나님,
저의 아이가 하나님의 사랑을 입고 그 은혜로 모자람을 채울 수 있게 하옵소서. 주님은 그의 어리석음을 아시니, 가르쳐 기도하게 하옵소서. 위로부터 은혜가 임해야 함을 깨닫기를 소망합니다. 주님은 그의 연약함을 아시니, 굳세게 되는 힘을 구하게 하옵소서. 주님은 그의 교만을 아시니, 겸손을 배우게 하옵소서. 그리하여 어느 곳에 있든지, 마음과 성품 그리고 힘을 다하여 주님께만 이끌려지게 하옵소서.
도우시는 성령님,
오늘도 저의 아이가 학교에서 공부할 때, 성령님의 도와주심을 구합니다. 교실에서 공부할 때, 선생님의 가르침에 열심히 배우게 하옵소서. 다른 생각을 하거나 허탄한 것에 마음을 두지 않고, 집중해서 전심전력할 수 있도록 이끌어 주옵소서. 책을 읽을 때 문장의 내용을 바르게 이해하도록 하시고, 선생님의 설명을 귀담아 듣게 하옵소서.
지혜와 지식의 하나님,
저의 아이가 수업을 할 때, 지혜의 하나님께서 역사해 주시기를 소망합니다. 새로운 지식을 이해하도록 도와주시는 성령님의 은총으로 잘 배우게 하옵소서. 지식의 하나님께서 간섭해 주셔서 바른 지식을 갖게 하옵소서. 교실에서의 생활에도 하나님의 함께 하심이 있으시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"형동한","To":"형통한"},{"From":"끈고한","To":"곤고한"},{"From":"병행하제","To":"병행하게"},{"From":"못하비","To":"못하게"},{"From":"은혜로우신 하나님, (본문과 붙음)","To":"은혜로우신 하나님, (호칭 줄로 나눔)"},{"From":"연약함 을","To":"연약함을"},{"From":"아시 니","To":"아시니"},{"From":"도우시는 성령님","To":"도우시는 성령님,"},{"From":"구합니 다","To":"구합니다"},{"From":"다 른","To":"다른"},{"From":"이해하 도록","To":"이해하도록"},{"From":"커담아 든게","To":"귀담아 듣게"},{"From":"하읍소서","To":"하옵소서"},{"From":"지혜와 지식의 하나님","To":"지혜와 지식의 하나님,"},{"From":"소망 합니다","To":"소망합니다"},{"From":"이해하도특","To":"이해하도록"},{"From":"배 우게","To":"배우게"},{"From":"간십해","To":"간섭해"},{"From":"하 옵소서","To":"하옵소서"},{"From":"원하면 서","To":"원하면서"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0220';
