-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 2/8 (1편)
-- 2026-11-18 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('0208', N'하나님의 사람으로 준비하게 하옵소서', N'삼상 10:18',
 N'이스라엘 자손에게 이르되 이스라엘 하나님 여호와께서 이같이 말씀하시기를 내가 이스라엘을 애굽에서 인도하여 내고 너희를 애굽인의 손과 너희를 압제하는 모든 나라의 손에서 건져내었느니라 하셨거늘',
 N'소망의 하나님,
저의 아이에게 복을 주셔서 오늘도 학교에 가게 하셨음에 감사드립니다. 학교생활을 통해서 하나님의 사람으로 자신을 준비하는데 최선을 다하게 하옵소서. 공부를 하는 것에 게으르지 않게 하시고, 새로운 지식을 습득할 때, 그것이 하나님께 어떤 영광을 드리는데 쓰일 것인지를 생각하게 하옵소서.
주 하나님,
저의 아이가 공부를 잘 해서 성적이 올라야겠으나 성적의 오름에 목을 매지 않게 하옵소서. 성적의 점수보다 학업에 임하는 자신의 태도에 주의를 기울이게 하옵소서. 이미 그들이 어미의 태에서 조성될 때, 아버지 하나님의 계획이 있음을 믿습니다. 주님께서 그들의 인생에 대한 계획을 이루어 나가시며 좋은 것으로 만족게 하심을 믿습니다.
사랑의 하나님,
주님의 인도하심을 바라보면서 최선의 노력을 쏟게 하옵소서. 학업의 성적을 바라보지 않고, 주님의 계획을 묵상하면서 주어진 자기 역할에 충실하게 하옵소서. 열매를 맺게 하시는 성령님의 은혜로 성실의 열매를 맺어드리게 하옵소서. 결코 허탄한 것에, 공부 외의 쓸데없는 것에 마음을 빼앗기지 않게 하옵소서. 그 성실함으로 공부에 최선을 기울이게 해 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"휴","To":"(날짜 앞 잡글자 — 뺌)"},{"From":"이스 라벨을","To":"이스라엘을"},{"From":"애급에서","To":"애굽에서"},{"From":"나라의손 에서","To":"나라의 손에서"},{"From":"소망의 하나님, (본문과 붙음)","To":"소망의 하나님, (호칭 줄로 나눔)"},{"From":"감사드럽니 다","To":"감사드립니다"},{"From":"최선을다 하게","To":"최선을 다하게"},{"From":"하읍소서","To":"하옵소서"},{"From":"생각 하게","To":"생각하게"},{"From":"주하나님, (문단 끝에 붙음)","To":"주 하나님, (호칭 줄로 옮김)"},{"From":"주 의를","To":"주의를"},{"From":"하움소서","To":"하옵소서"},{"From":"사랑의 하나님, (문단 끝에 붙음)","To":"사랑의 하나님, (호칭 줄로 옮김)"},{"From":"북상하면서","To":"묵상하면서"},{"From":"하유소서","To":"하옵소서"},{"From":"은해로","To":"은혜로"},{"From":"하옵소서 결코","To":"하옵소서. 결코"},{"From":"마음 을","To":"마음을"},{"From":"54 자녀를 위하 30r","To":"(쪽 머리글 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '0208';
