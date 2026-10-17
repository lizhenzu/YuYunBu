-- ============================================================
--  수시·수능 준비생을 위한 기도문 — 매일 순환 3편 추가 (SortOrder 45~47)
--
--   45 혼자인 것 같을 때            (수험 생활의 외로움)
--   46 무엇을 위해 하는지 잊을 때    (공부의 의미·동기)
--   47 기도할 힘조차 없을 때         (기도가 나오지 않는 날)
--
--  ※ 세 편 모두 RotateYN = 'Y' — 매일 순환에 들어갑니다.
--  ※ 문체·분량은 기존 편들과 맞췄습니다 (하여 주옵소서 체 · 다섯 문단 · 440~560자).
--  ※ 화자는 모두 "부모가 자녀를 위하여" 입니다. 1인칭('저')을 쓰지 않습니다.
--  ※ 성경 구절은 이미 쓰인 것과 겹치지 않게 골랐습니다.
--  ※ 이미 들어 있으면 다시 넣지 않습니다.
-- ============================================================
SET NOCOUNT ON;

IF EXISTS (SELECT 1 FROM dbo.EntranceExamPray WHERE SortOrder IN (45, 46, 47))
BEGIN
    PRINT N'SortOrder 45~47 이 이미 있습니다 — 삽입 건너뜀';
END
ELSE
BEGIN
    INSERT INTO dbo.EntranceExamPray (SortOrder, Title, PrayText, BibleRef, BibleText, UseYN, RotateYN, CreatedAt)
    VALUES
    (45, N'혼자인 것 같을 때',
N'떠나지 아니하시는 하나님,

사람들 사이에 있어도 **○○(이)**가 혼자인 것 같습니다. 같은 교실에 앉아 있지만 저마다 제 짐을 지고 있어, 속엣말을 꺼내 놓을 자리가 마땅치 않습니다.

그 외로움을 가볍게 여기지 않으시는 줄 압니다. 아무 말 없이 견디는 시간에도 주님께서 곁에 계심을 느끼게 하시고, 혼자 걷는 길이 아님을 알게 하여 주옵소서.

마음을 나눌 한 사람을 붙여 주옵소서. 여럿이 아니어도 좋으니, 힘든 날 문자 한 통 보낼 수 있는 이를 두어 주시고, 그 앞에서 솔직할 수 있게 하여 주옵소서.

스스로 문을 닫지 않게 하여 주옵소서. 지쳤다는 이유로 곁에 오는 손길을 밀어내지 않게 하시고, 도움이 필요할 때 도와 달라 말할 수 있게 하여 주옵소서.

여호와 그가 네 앞서 행하시며 너와 함께 하사 너를 떠나지 아니하시며 버리지 아니하시리니 하였습니다. 그 말씀대로 **○○(이)**의 오늘을 지켜 주옵소서.

예수님의 이름으로 기도드립니다. 아멘.',
     N'신명기 31:8', N'그리하면 여호와 그가 네 앞서 행하시며 너와 함께 하사 너를 떠나지 아니하시며 버리지 아니하시리니 너는 두려워하지 말라 놀라지 말라',
     'Y', 'Y', DATEADD(HOUR, 16, GETDATE())),

    (46, N'무엇을 위해 하는지 잊을 때',
N'뜻을 두시는 하나님,

**○○(이)**가 왜 이것을 하고 있는지 모르겠다고 합니다. 하루하루 문제를 풀고 있으나 무엇을 향해 가는지 흐려져, 발걸음에 힘이 실리지 않습니다.

이 시간이 점수를 얻기 위한 것만이 아님을 알게 하여 주옵소서. 견디며 배우는 성실과 정직과 인내가 그 안에 쌓이고 있음을 보게 하시고, 그것이 오래 남을 것임을 믿게 하여 주옵소서.

주님께서 무엇을 맡기시려는지 궁금해하게 하여 주옵소서. 지금 배우는 것이 훗날 어디에 쓰일지 다 알지 못하여도, 준비시키시는 손길이 있음을 신뢰하게 하여 주옵소서.

남이 정해 준 이유로만 걷지 않게 하여 주옵소서. 좋은 대학이 목적의 전부가 되지 않게 하시고, 그 너머에 무엇을 하며 살고 싶은지 스스로 물어보게 하여 주옵소서.

무엇을 하든지 다 하나님의 영광을 위하여 하라 하셨습니다. 오늘 푸는 한 문제도 그 뜻 안에 있음을 알고 다시 붙들게 하옵소서.

예수님의 이름으로 기도드립니다. 아멘.',
     N'고린도전서 10:31', N'그런즉 너희가 먹든지 마시든지 무엇을 하든지 다 하나님의 영광을 위하여 하라',
     'Y', 'Y', DATEADD(HOUR, 16, GETDATE())),

    (47, N'기도할 힘조차 없을 때',
N'탄식을 들으시는 하나님,

**○○(이)**가 너무 지쳐 기도할 말조차 찾지 못합니다. 무엇을 구해야 할지도 모르겠고, 입을 떼는 것마저 무겁습니다.

말이 되지 않는 마음을 그대로 받아 주옵소서. 정돈된 문장이라야 들으시는 분이 아닌 줄 아오니, 한숨과 침묵도 기도로 받아 주옵소서.

기도하지 못한 날을 죄책감으로 덮지 않게 하여 주옵소서. 잘 기도하지 못하였다 하여 스스로를 몰아세우지 않게 하시고, 주님께서 먼저 찾아오시는 분인 줄 알게 하여 주옵소서.

곁에서 대신 기도하는 이들을 붙여 주옵소서. 저희가 그 자리에서 잠잠히 기도하게 하시고, 지친 아이 대신 저희 입을 열어 주님께 아뢰게 하여 주옵소서.

성령께서 말할 수 없는 탄식으로 우리를 위하여 친히 간구하신다 하였습니다. 오늘 **○○(이)**의 침묵도 그렇게 주님께 닿게 하옵소서.

예수님의 이름으로 기도드립니다. 아멘.',
     N'로마서 8:26', N'성령도 우리의 연약함을 도우시나니 오직 성령이 말할 수 없는 탄식으로 우리를 위하여 친히 간구하시느니라',
     'Y', 'Y', DATEADD(HOUR, 16, GETDATE()));

    PRINT N'기도문 3편 추가 완료';
END
GO

-- 줄바꿈을 CRLF 로 통일 (화면이 \r\n 로 문단을 나눕니다)
UPDATE dbo.EntranceExamPray
   SET PrayText = REPLACE(REPLACE(REPLACE(PrayText,
                    CHAR(13) + CHAR(10), CHAR(10)),
                    CHAR(13),            CHAR(10)),
                    CHAR(10),            CHAR(13) + CHAR(10))
 WHERE SortOrder IN (45, 46, 47);
GO

-- 결과 확인
SELECT PrayId, SortOrder, Title, LEN(PrayText) AS len,
       (LEN(PrayText) - LEN(REPLACE(PrayText, CHAR(13)+CHAR(10)+CHAR(13)+CHAR(10), N''))) / 4 + 1 AS paras,
       RotateYN, BibleRef
  FROM dbo.EntranceExamPray
 WHERE SortOrder IN (45, 46, 47)
 ORDER BY SortOrder;

SELECT COUNT(*) AS rows_total,
       SUM(CASE WHEN ISNULL(UseYN,'Y') = 'Y' THEN 1 ELSE 0 END) AS active_count,
       SUM(CASE WHEN ISNULL(UseYN,'Y') = 'Y' AND ISNULL(RotateYN,'Y') = 'Y' THEN 1 ELSE 0 END) AS rotation_count,
       SUM(CASE WHEN PrayText LIKE N'%저를%' OR PrayText LIKE N'%저만%' OR PrayText LIKE N'%제가%'
                 OR PrayText LIKE N'%저는%' OR PrayText LIKE N'%저의%' OR PrayText LIKE N'%저에게%'
                THEN 1 ELSE 0 END) AS first_person_left,
       SUM(CASE WHEN PrayText LIKE N'%해 주세요%'  THEN 1 ELSE 0 END) AS casual_left,
       SUM(CASE WHEN PrayText LIKE N'%기도 제목:%' THEN 1 ELSE 0 END) AS label_left
  FROM dbo.EntranceExamPray;

-- 성경 구절이 겹치는지 확인 (없어야 정상)
SELECT BibleRef, COUNT(*) AS cnt FROM dbo.EntranceExamPray
 GROUP BY BibleRef HAVING COUNT(*) > 1;
GO
