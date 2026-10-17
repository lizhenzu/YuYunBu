-- ============================================================
--  수시·수능 준비생을 위한 기도문 — 3편 추가 (SortOrder 38~40)
--
--   38 오래 제자리인 것 같을 때   (해도 늘지 않는 시기)
--   39 예민해진 마음을 두고        (가족에게 날 선 말을 하게 될 때)
--   40 늦은 밤 돌아오는 길에       (하루를 마치고 오는 길의 안전과 마무리)
--
--  ※ 문체·분량은 기존 편들과 맞췄습니다 (하여 주옵소서 체 · 다섯 문단 · 450~560자).
--  ※ 화자는 모두 "부모가 자녀를 위하여" 입니다. 1인칭('저')을 쓰지 않습니다.
--  ※ 성경 구절은 이미 쓰인 것과 겹치지 않게 골랐습니다.
--  ※ 이미 들어 있으면 다시 넣지 않습니다.
-- ============================================================
SET NOCOUNT ON;

IF EXISTS (SELECT 1 FROM dbo.EntranceExamPray WHERE SortOrder IN (38, 39, 40))
BEGIN
    PRINT N'SortOrder 38~40 이 이미 있습니다 — 삽입 건너뜀';
END
ELSE
BEGIN
    INSERT INTO dbo.EntranceExamPray (SortOrder, Title, PrayText, BibleRef, BibleText, UseYN, CreatedAt)
    VALUES
    (38, N'오래 제자리인 것 같을 때',
N'때를 기다리게 하시는 하나님,

**○○(이)**가 오래 애썼는데도 나아지는 것이 보이지 않습니다. 어제와 오늘이 다르지 않은 것 같아, 이렇게 계속하는 것이 맞는지 스스로 묻고 있습니다.

보이지 않는 자리에서 자라고 있음을 알게 하여 주옵소서. 씨앗이 땅속에 있는 동안에는 아무 일도 없어 보이나 뿌리가 내리고 있는 줄 아오니, 지금이 그 시간임을 믿게 하여 주옵소서.

눈에 보이는 것으로만 판단하지 않게 하여 주옵소서. 오늘 하루의 성과로 지난 몇 달을 깎아내리지 않게 하시고, 쌓인 것이 어느 날 한꺼번에 드러날 것을 기다리게 하여 주옵소서.

방법을 살필 지혜도 함께 주옵소서. 애쓰는 것만으로 되지 아니하는 자리가 있음을 알게 하시고, 무엇을 바꾸어야 할지 물어볼 용기와 조언을 들을 귀를 주옵소서.

너희에게 인내가 필요함은 너희가 하나님의 뜻을 행한 후에 약속하신 것을 받기 위함이라 하셨습니다. 그 말씀대로 **○○(이)**가 오늘도 자리를 지키게 하옵소서.

예수님의 이름으로 기도드립니다. 아멘.',
     N'히브리서 10:36', N'너희에게 인내가 필요함은 너희가 하나님의 뜻을 행한 후에 약속하신 것을 받기 위함이라',
     'Y', DATEADD(HOUR, 16, GETDATE())),

    (39, N'예민해진 마음을 두고',
N'마음을 아시는 하나님,

지치고 눌린 날이 이어지며 **○○(이)**의 말이 자꾸 날카로워집니다. 가장 가까운 이들에게 먼저 그 말이 향하고, 돌아서서는 스스로도 마음이 불편해집니다.

그 마음의 무게를 주님께서 아십니다. 짜증으로 새어 나오는 것이 사실은 지침이고 두려움인 줄 아시오니, 나무라기 전에 먼저 그 속을 어루만져 주옵소서.

말이 나가기 전에 한 번 멈추게 하여 주옵소서. 사람마다 듣기는 속히 하고 말하기는 더디 하며 성내기도 더디 하라 하셨사오니, 그 말씀이 문득 떠오르게 하여 주옵소서.

미안하다 말할 용기를 주옵소서. 마음에 걸리는 것을 그냥 덮어 두지 않게 하시고, 짧은 한마디로 그날의 어긋남을 풀게 하여 주옵소서.

집이 눈치 보는 자리가 되지 않게 하여 주옵소서. 저희도 예민한 말 한마디에 같이 날을 세우지 않게 하시고, 서로의 지침을 헤아리며 이 시기를 함께 지나가게 하옵소서.

예수님의 이름으로 기도드립니다. 아멘.',
     N'야고보서 1:19', N'사람마다 듣기는 속히 하고 말하기는 더디 하며 성내기도 더디 하라',
     'Y', DATEADD(HOUR, 16, GETDATE())),

    (40, N'늦은 밤 돌아오는 길에',
N'오고 감을 지키시는 하나님,

**○○(이)**가 늦은 밤 집으로 돌아옵니다. 어두운 길과 인적 드문 골목을 지나야 하고, 몸도 마음도 지쳐 있습니다.

그 걸음을 안전하게 지켜 주옵소서. 다니는 길에 위험한 일이 없게 하시고, 타고 오는 길과 걸어오는 길을 눈여겨 보아 주시고, 문을 열고 들어설 때까지 함께하여 주옵소서.

하루를 후회로만 덮지 않게 하여 주옵소서. 돌아오는 길에 못한 것만 세다가 마음이 무거워지지 않게 하시고, 오늘 감당한 것들을 헤아리며 감사하게 하여 주옵소서.

집에 들어설 때 편안하게 하여 주옵소서. 문을 여는 순간 다시 긴장하지 않게 하시고, 따뜻한 한마디와 밥 한 끼가 그 하루를 풀어 주게 하여 주옵소서.

네가 네 길을 평안히 행하겠고 네 발이 거치지 아니하겠으며 네가 누울 때에 두려워하지 아니하겠고 네 잠이 달리로다 하셨습니다. 그 말씀대로 오늘 밤을 지켜 주옵소서.

예수님의 이름으로 기도드립니다. 아멘.',
     N'잠언 3:23-24', N'네가 네 길을 평안히 행하겠고 네 발이 거치지 아니하겠으며 네가 누울 때에 두려워하지 아니하겠고 네 잠이 달리로다',
     'Y', DATEADD(HOUR, 16, GETDATE()));

    PRINT N'기도문 3편 추가 완료';
END
GO

-- 줄바꿈을 CRLF 로 통일 (화면이 \r\n 로 문단을 나눕니다)
UPDATE dbo.EntranceExamPray
   SET PrayText = REPLACE(REPLACE(REPLACE(PrayText,
                    CHAR(13) + CHAR(10), CHAR(10)),
                    CHAR(13),            CHAR(10)),
                    CHAR(10),            CHAR(13) + CHAR(10))
 WHERE SortOrder IN (38, 39, 40);
GO

-- 결과 확인
SELECT PrayId, SortOrder, Title, LEN(PrayText) AS len,
       (LEN(PrayText) - LEN(REPLACE(PrayText, CHAR(13)+CHAR(10)+CHAR(13)+CHAR(10), N''))) / 4 + 1 AS paras,
       BibleRef
  FROM dbo.EntranceExamPray
 WHERE SortOrder IN (38, 39, 40)
 ORDER BY SortOrder;

SELECT COUNT(*) AS rows_total,
       SUM(CASE WHEN ISNULL(UseYN,'Y') = 'Y' THEN 1 ELSE 0 END) AS active_count,
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
