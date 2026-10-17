-- ============================================================
--  수시·수능 준비생을 위한 기도문 — 단정형을 간구·추측형으로
--
--  기도하는 부모는 자녀의 속마음을 알 수 없습니다. "답답해합니다" 처럼
--  단정해 버리면 사실을 아는 것처럼 되어 버립니다. 그럴 수 있으니 기도하는 것이므로
--  "답답해하지는 않는지요", "…할 때가 있습니다", "주님께서 아십니다" 로 바꿉니다.
--
--  · 문제가 되는 문장은 거의 모두 각 기도문의 첫 문단(상황 서술)에 있습니다.
--  · 간구 문단("…하여 주옵소서")은 손대지 않습니다.
--  · REPLACE 로 해당 문장만 바꾸므로, 이미 고쳐진 행은 그대로 지나갑니다.
--  · UPDATE 만 씁니다. PrayId 는 바뀌지 않습니다.
-- ============================================================
SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EntranceExamPray_bak_v2' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    SELECT * INTO dbo.EntranceExamPray_bak_v2 FROM dbo.EntranceExamPray;
    PRINT '[EntranceExamPray_bak_v2] 백업 생성';
END
ELSE
    PRINT '[EntranceExamPray_bak_v2] 백업이 이미 있습니다 — 건너뜀';

DECLARE @n INT = 0;

-- 3 집중력을 구하며
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'**○○(이)**의 마음이 자꾸 흩어집니다. 책상 앞에 앉아 있어도 생각은 다른 곳에 가 있고, 시간은 흘렀는데 남은 것이 없어 스스로 답답해합니다.',
N'**○○(이)**의 마음이 자꾸 흩어지지는 않는지요. 책상 앞에 앉아 있어도 생각은 다른 곳에 가 있고, 시간은 흘렀는데 남은 것이 없어 스스로 답답해할 때가 있을 것입니다. 저희가 다 알지 못하는 그 속을 주님께서 아십니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 3;

-- 4 마음이 불안할 때
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'이유 없이 가슴이 답답하고 밤에 잠이 오지 않습니다. 아직 이르지 아니한 일들이 **○○(이)**의 마음을 미리 무겁게 합니다.',
N'이유 없이 가슴이 답답하고 밤에 잠이 오지 아니하는 날이 있을 것입니다. 아직 이르지 아니한 일들이 **○○(이)**의 마음을 미리 무겁게 하지는 않는지요.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 4;

-- 5 성적이 생각만큼 나오지 않았을 때
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'열심히 하였는데 결과가 따라 주지 않아 마음이 무너집니다. 애쓴 시간이 헛되었나 싶어 **○○(이)**의 어깨가 처집니다.',
N'열심히 하였는데 결과가 따라 주지 아니할 때가 있습니다. 애쓴 시간이 헛되었나 싶어 **○○(이)**의 어깨가 처지지는 않는지, 겉으로 드러내지 아니하는 그 마음을 주님께서 아십니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 5;

-- 6 비교하는 마음을 두고
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'남보다 앞섰다 싶으면 잠시 안심하고 뒤처졌다 싶으면 곧 무너지니, 비교는 **○○(이)**의 마음을 지치게만 합니다.',
N'남보다 앞섰다 싶으면 잠시 안심하고 뒤처졌다 싶으면 곧 무너지기 쉬우니, 비교가 **○○(이)**의 마음을 지치게 하지는 않는지 살펴 주옵소서.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 6;

-- 7 지치고 잠이 부족할 때
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'몸이 무겁고 자꾸 눈이 감깁니다.',
N'몸이 무겁고 자꾸 눈이 감기는 날이 있을 것입니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 7;

-- 9 미루는 마음을 두고
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'해야 할 것을 알면서도 **○○(이)**가 자꾸 미룹니다. 내일 하면 된다는 생각으로 오늘을 흘려보내고, 그렇게 쌓인 것들이 다시 마음을 무겁게 합니다.',
N'해야 할 것을 알면서도 자꾸 미루게 되는 날이 있습니다. 내일 하면 된다는 생각으로 오늘을 흘려보내고, 그렇게 쌓인 것들이 다시 **○○(이)**의 마음을 무겁게 하지는 않는지요.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 9;

-- 10 절제를 구하며
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'**○○(이)**의 손이 자꾸 휴대전화로 갑니다. 잠깐이라 생각한 것이 한 시간이 되고, 그러고 나면 마음만 더 허전해집니다.',
N'**○○(이)**의 손이 자꾸 휴대전화로 향하지는 않는지요. 잠깐이라 여긴 것이 한 시간이 되고, 그러고 나면 마음만 더 허전해지기 쉬운 줄 압니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 10;

-- 11 시험을 앞두고
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'준비한 것이 있어도 마음은 자꾸 조급해지고, 남은 시간이 짧게만 느껴집니다.',
N'준비한 것이 있어도 마음은 조급해지기 쉽고, 남은 시간이 짧게만 느껴질 것입니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 11;

-- 13 학교와 학과를 정하며
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'**○○(이)**가 어느 학교와 어느 학과에 마음을 두어야 할지 고민이 깊습니다. 앞에 놓인 길은 여럿이나 어느 길이 옳은지 알기 어렵고, 주변에서 들려오는 말들이 저마다 달라 마음이 더욱 어지럽습니다.',
N'**○○(이)** 앞에 놓인 길은 여럿이나 어느 길이 옳은지 알기 어렵습니다. 어느 학교와 어느 학과에 마음을 두어야 할지 고민이 깊지는 않은지요. 주변에서 들려오는 말들이 저마다 달라 마음이 더욱 어지러울 것입니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 13;

-- 14 자기소개서를 쓰며
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'무엇을 써야 할지 막막하고, 지나온 시간이 초라해 보여 손이 잘 나가지 않습니다.',
N'무엇을 써야 할지 막막하고, 지나온 시간이 초라해 보여 손이 잘 나가지 아니할 것입니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 14;

-- 16 면접을 앞두고
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'준비한 것이 있어도 그 자리에 서면 마음이 굳어지고 손이 떨립니다.',
N'준비한 것이 있어도 그 자리에 서면 마음이 굳어지고 손이 떨리기 쉬운 줄 압니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 16;

-- 17 논술과 서술형을 준비하며
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'머릿속에는 있는 듯한데 막상 쓰려 하면 문장이 되지 않아 **○○(이)**가 답답해합니다.',
N'머릿속에는 있는 듯한데 막상 쓰려 하면 문장이 되지 아니하여 **○○(이)**가 답답해하지는 않는지요.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 17;

-- 18 발표를 기다리며
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'**○○(이)**의 마음이 하루에도 몇 번씩 오르내립니다. 좋은 쪽을 그려 보다가도 이내 나쁜 쪽을 먼저 헤아리게 됩니다.',
N'**○○(이)**의 마음이 하루에도 몇 번씩 오르내리지는 않는지요. 좋은 쪽을 그려 보다가도 이내 나쁜 쪽을 먼저 헤아리기 쉬운 자리입니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 18;

-- 21 수능일을 앞두고
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'숫자가 줄어드는 것을 보며 **○○(이)**의 마음이 조급해집니다.',
N'숫자가 줄어드는 것을 보며 **○○(이)**의 마음이 조급해지지는 않는지 살펴 주옵소서.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 21;

-- 22 수능 전날 밤에
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'마음이 자꾸 뛰어 잠이 오지 않습니다. 준비한 것을 헤아려 보다가도 부족한 것만 눈에 들어옵니다.',
N'마음이 자꾸 뛰어 잠이 오지 아니할 것입니다. 준비한 것을 헤아려 보다가도 부족한 것만 눈에 들어오기 쉬운 밤입니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 22;

-- 24 시험장에 들어서며
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'익숙하지 않은 책상과 처음 보는 사람들 사이에서 마음이 굳어집니다.',
N'익숙하지 아니한 책상과 처음 보는 사람들 사이에서 마음이 굳어지기 쉬운 자리입니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 24;

-- 25 시험 도중 막힐 때
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'갑자기 머리가 하얘집니다. 분명 알던 것인데 떠오르지 않고, 마음만 급해집니다.',
N'갑자기 머리가 하얘지는 순간이 있습니다. 분명 알던 것인데 떠오르지 아니하고, 마음만 급해지기 쉽습니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 25;

-- 27 기대가 무겁게 느껴질 때
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'**○○(이)**를 향한 기대가 그 어깨를 무겁게 합니다. 사랑에서 나온 마음인 줄 알면서도, 그 마음에 미치지 못할까 두려워 눌려 있습니다.',
N'**○○(이)**를 향한 기대가 그 어깨를 무겁게 하지는 않는지요. 사랑에서 나온 마음인 줄 알면서도, 그 마음에 미치지 못할까 두려워 눌려 있을지 모릅니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 27;

-- 31 진로를 정하지 못했을 때
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'**○○(이)**가 무엇을 하고 싶은지 아직 알지 못합니다. 주위는 저마다 길을 정한 것 같은데 자기만 남겨진 듯하여 마음이 조급해집니다.',
N'**○○(이)**가 무엇을 하고 싶은지 아직 찾지 못하였습니다. 주위는 저마다 길을 정한 것 같은데 자기만 남겨진 듯하여 마음이 조급해지지는 않는지요.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 31;

-- 34 학교와 학과를 정하며 (두 번째)
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'**○○(이)**가 어느 학교와 어느 학과에 마음을 두어야 할지 고민이 깊습니다. 주변에서 들려오는 말들이 저마다 달라 마음이 어지럽습니다.',
N'**○○(이)**가 어느 학교와 어느 학과에 마음을 두어야 할지 고민이 깊지는 않은지요. 주변에서 들려오는 말들이 저마다 달라 마음이 어지러울 것입니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 34;

-- 37 예배의 자리를 지키게 하소서
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'바쁜 날들이 이어지며 **○○(이)**가 주님 앞에 앉는 시간이 자꾸 밀려납니다. 한 시간이 아까워 예배도 기도도 뒤로 미루게 됩니다.',
N'바쁜 날들이 이어지며 **○○(이)**가 주님 앞에 앉는 시간이 밀려나기 쉽습니다. 한 시간이 아까워 예배도 기도도 뒤로 미루게 되지는 않는지 살펴 주옵소서.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 37;

-- 38 오래 제자리인 것 같을 때
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'**○○(이)**가 오래 애썼는데도 나아지는 것이 보이지 않습니다. 어제와 오늘이 다르지 않은 것 같아, 이렇게 계속하는 것이 맞는지 스스로 묻고 있습니다.',
N'**○○(이)**가 오래 애썼는데도 나아지는 것이 잘 보이지 아니할 때가 있습니다. 어제와 오늘이 다르지 않은 것 같아, 이렇게 계속하는 것이 맞는지 스스로 묻고 있지는 않은지요.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 38;

-- 39 예민해진 마음을 두고
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'지치고 눌린 날이 이어지며 **○○(이)**의 말이 자꾸 날카로워집니다. 가장 가까운 이들에게 먼저 그 말이 향하고, 돌아서서는 스스로도 마음이 불편해집니다.',
N'지치고 눌린 날이 이어지며 **○○(이)**의 말이 날카로워질 때가 있습니다. 가장 가까운 이들에게 먼저 그 말이 향하고, 돌아서서는 스스로도 마음이 불편하지는 않은지요.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 39;

-- 40 늦은 밤 돌아오는 길에
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'어두운 길과 인적 드문 골목을 지나야 하고, 몸도 마음도 지쳐 있습니다.',
N'어두운 길과 인적 드문 골목을 지나야 하고, 몸도 마음도 지쳐 있을 것입니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 40;

-- 41 감사를 세어 보게 하소서
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'**○○(이)**의 하루가 부족한 것으로만 채워지는 것 같습니다. 못한 것과 모자란 것은 금세 눈에 들어오는데, 이미 받은 것은 잘 보이지 않습니다.',
N'**○○(이)**의 하루가 부족한 것으로만 채워지지는 않는지요. 못한 것과 모자란 것은 금세 눈에 들어오는데, 이미 받은 것은 잘 보이지 아니하기 쉽습니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 41;

-- 42 외운 것이 기억나지 않을 때
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'분명히 외웠는데 떠오르지 않습니다. 어제 알던 것이 오늘 사라진 것 같아 **○○(이)**가 답답해합니다.',
N'분명히 외웠는데 떠오르지 아니할 때가 있습니다. 어제 알던 것이 오늘 사라진 것 같아 **○○(이)**가 답답해하지는 않는지요.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 42;

-- 43 쉬는 것을 미안해하지 않게 하소서
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'**○○(이)**가 잠시 쉬면서도 마음이 편하지 않습니다. 남들은 지금도 하고 있을 것 같아, 쉬는 시간마저 불안으로 채웁니다.',
N'**○○(이)**가 잠시 쉬면서도 마음이 편하지 아니할지 모릅니다. 남들은 지금도 하고 있을 것 같아, 쉬는 시간마저 불안으로 채우지는 않는지요.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 43;

-- 44 정직하게 걸어가게 하소서
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'앞서가고 싶은 마음에 지름길이 눈에 들어옵니다. 남들도 다 그렇게 한다는 말 앞에서 **○○(이)**의 마음이 흔들립니다.',
N'앞서가고 싶은 마음에 지름길이 눈에 들어오는 자리입니다. 남들도 다 그렇게 한다는 말 앞에서 **○○(이)**의 마음이 흔들리지는 않는지 지켜 주옵소서.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 44;

-- 45 혼자인 것 같을 때
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'사람들 사이에 있어도 **○○(이)**가 혼자인 것 같습니다. 같은 교실에 앉아 있지만 저마다 제 짐을 지고 있어, 속엣말을 꺼내 놓을 자리가 마땅치 않습니다.',
N'사람들 사이에 있어도 **○○(이)**가 혼자인 것처럼 느끼지는 않는지요. 같은 교실에 앉아 있지만 저마다 제 짐을 지고 있어, 속엣말을 꺼내 놓을 자리가 마땅치 아니할 것입니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 45;

-- 46 무엇을 위해 하는지 잊을 때
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'**○○(이)**가 왜 이것을 하고 있는지 모르겠다고 합니다. 하루하루 문제를 풀고 있으나 무엇을 향해 가는지 흐려져, 발걸음에 힘이 실리지 않습니다.',
N'**○○(이)**가 왜 이것을 하고 있는지 모르겠다고 할 때가 있습니다. 하루하루 문제를 풀고 있으나 무엇을 향해 가는지 흐려져, 발걸음에 힘이 실리지 아니하는 날도 있을 것입니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 46;

-- 47 기도할 힘조차 없을 때
UPDATE dbo.EntranceExamPray SET PrayText = REPLACE(PrayText,
N'**○○(이)**가 너무 지쳐 기도할 말조차 찾지 못합니다. 무엇을 구해야 할지도 모르겠고, 입을 떼는 것마저 무겁습니다.',
N'**○○(이)**가 너무 지쳐 기도할 말조차 찾지 못할 때가 있을 것입니다. 무엇을 구해야 할지 모르겠고, 입을 떼는 것마저 무거운 날이 있는 줄 압니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE SortOrder = 47;

PRINT N'단정형 → 간구형 교정 완료';
GO

-- 줄바꿈을 CRLF 로 통일 (화면이 \r\n 로 문단을 나눕니다)
UPDATE dbo.EntranceExamPray
   SET PrayText = REPLACE(REPLACE(REPLACE(PrayText,
                    CHAR(13) + CHAR(10), CHAR(10)),
                    CHAR(13),            CHAR(10)),
                    CHAR(10),            CHAR(13) + CHAR(10));
GO

-- ────────────────────────────────────────────────────────────
--  확인 : 아직 남아 있는 단정형 표현이 있는지
--  (완전히 0 이 되기는 어렵습니다 — 눈으로 볼 수 있는 사실은 단정해도 됩니다)
-- ────────────────────────────────────────────────────────────
SELECT SortOrder, Title,
       CASE WHEN PrayText LIKE N'%해합니다%'   THEN N'해합니다 '   ELSE N'' END
     + CASE WHEN PrayText LIKE N'%무너집니다%' THEN N'무너집니다 ' ELSE N'' END
     + CASE WHEN PrayText LIKE N'%깊습니다%'   THEN N'깊습니다 '   ELSE N'' END
     + CASE WHEN PrayText LIKE N'%집니다.%'    THEN N'~집니다 '    ELSE N'' END AS 남은표현
  FROM dbo.EntranceExamPray
 WHERE PrayText LIKE N'%해합니다%' OR PrayText LIKE N'%무너집니다%'
    OR PrayText LIKE N'%깊습니다%' OR PrayText LIKE N'%집니다.%'
 ORDER BY SortOrder;

SELECT COUNT(*) AS rows_total,
       SUM(CASE WHEN PrayText LIKE N'%하지는 않는지%' OR PrayText LIKE N'%때가 있%'
                 OR PrayText LIKE N'%것입니다%'       OR PrayText LIKE N'%줄 압니다%'
                THEN 1 ELSE 0 END) AS softened_rows
  FROM dbo.EntranceExamPray;
GO
