-- ============================================================
--  수시·수능 준비생을 위한 기도문 — 매일 순환 4편 추가 (SortOrder 41~44)
--
--   41 감사를 세어 보게 하소서       (감사)
--   42 외운 것이 기억나지 않을 때     (암기·기억)
--   43 쉬는 것을 미안해하지 않게      (쉼에 대한 죄책감)
--   44 정직하게 걸어가게 하소서       (요령·부정에 흔들릴 때)
--
--  ※ 네 편 모두 RotateYN = 'Y' — 매일 순환에 들어갑니다.
--  ※ 문체·분량은 기존 편들과 맞췄습니다 (하여 주옵소서 체 · 다섯 문단 · 450~560자).
--  ※ 화자는 모두 "부모가 자녀를 위하여" 입니다. 1인칭('저')을 쓰지 않습니다.
--  ※ 성경 구절은 이미 쓰인 것과 겹치지 않게 골랐습니다.
--  ※ 이미 들어 있으면 다시 넣지 않습니다.
-- ============================================================
SET NOCOUNT ON;

IF EXISTS (SELECT 1 FROM dbo.EntranceExamPray WHERE SortOrder IN (41, 42, 43, 44))
BEGIN
    PRINT N'SortOrder 41~44 가 이미 있습니다 — 삽입 건너뜀';
END
ELSE
BEGIN
    INSERT INTO dbo.EntranceExamPray (SortOrder, Title, PrayText, BibleRef, BibleText, UseYN, RotateYN, CreatedAt)
    VALUES
    (41, N'감사를 세어 보게 하소서',
N'날마다 베푸시는 하나님,

**○○(이)**의 하루가 부족한 것으로만 채워지는 것 같습니다. 못한 것과 모자란 것은 금세 눈에 들어오는데, 이미 받은 것은 잘 보이지 않습니다.

오늘 받은 것을 헤아리게 하여 주옵소서. 아침에 눈을 뜬 것과 앉을 자리가 있는 것과 배울 기회가 주어진 것이, 당연한 것이 아니라 은혜인 줄 알게 하여 주옵소서.

곁에 있는 이들을 보게 하여 주옵소서. 차려 주신 밥과 건네받은 말 한마디와 조용히 기도해 주는 손길이 있었음을 기억하게 하시고, 그 마음을 알아보게 하여 주옵소서.

감사가 성적에 매이지 않게 하여 주옵소서. 잘된 날에만 감사하고 그렇지 못한 날에는 원망으로 돌아서지 않게 하시고, 어떤 하루에도 세어 볼 것이 있음을 알게 하여 주옵소서.

내 영혼아 여호와를 송축하며 그의 모든 은택을 잊지 말지어다 하였습니다. 그 말씀대로 오늘 하루를 감사로 닫게 하옵소서.

예수님의 이름으로 기도드립니다. 아멘.',
     N'시편 103:2', N'내 영혼아 여호와를 송축하며 그의 모든 은택을 잊지 말지어다',
     'Y', 'Y', DATEADD(HOUR, 16, GETDATE())),

    (42, N'외운 것이 기억나지 않을 때',
N'기억하게 하시는 하나님,

분명히 외웠는데 떠오르지 않습니다. 어제 알던 것이 오늘 사라진 것 같아 **○○(이)**가 답답해합니다.

배운 것이 마음에 오래 머물게 하여 주옵소서. 읽은 것을 이해하게 하시고, 이해한 것을 새기게 하시고, 새긴 것을 필요한 때에 꺼내어 쓰게 하여 주옵소서.

잊어버린 것에 낙심하지 않게 하여 주옵소서. 잊는 것이 사람의 일인 줄 알게 하시고, 다시 들여다보는 수고를 아까워하지 않게 하여 주옵소서. 되풀이하는 날들이 헛되지 않음을 믿게 하옵소서.

머리에만 담지 않게 하여 주옵소서. 외우는 것에 그치지 말고 왜 그러한지를 알게 하시고, 그렇게 익힌 것이 오래 남게 하여 주옵소서.

성령께서 모든 것을 가르치시고 생각나게 하시리라 하셨습니다. 시험의 자리에서도 그 도우심을 힘입어 담대하게 하옵소서.

예수님의 이름으로 기도드립니다. 아멘.',
     N'요한복음 14:26', N'보혜사 곧 아버지께서 내 이름으로 보내실 성령 그가 너희에게 모든 것을 가르치시고 내가 너희에게 말한 모든 것을 생각나게 하리라',
     'Y', 'Y', DATEADD(HOUR, 16, GETDATE())),

    (43, N'쉬는 것을 미안해하지 않게 하소서',
N'쉼을 명하신 하나님,

**○○(이)**가 잠시 쉬면서도 마음이 편하지 않습니다. 남들은 지금도 하고 있을 것 같아, 쉬는 시간마저 불안으로 채웁니다.

쉬는 것이 게으름이 아님을 알게 하여 주옵소서. 주님께서도 제자들을 한적한 곳으로 데려가 쉬게 하셨사오니, 멈추는 것도 걸어가는 일의 한 부분인 줄 믿게 하여 주옵소서.

쉴 때에는 온전히 쉬게 하여 주옵소서. 몸은 앉아 있고 마음은 책상에 매여 있지 않게 하시고, 짧은 시간이라도 숨을 돌리고 다시 힘을 얻게 하여 주옵소서.

죄책감으로 하루를 갉아먹지 않게 하여 주옵소서. 쉬고 나서 스스로를 탓하느라 그 쉼마저 잃지 않게 하시고, 다시 앉을 때 가벼운 마음으로 앉게 하여 주옵소서.

오래 달려야 할 길인 줄 아오니, 지치기 전에 쉬는 지혜를 주옵소서. 그 쉼 가운데 주님께서 **○○(이)**를 새롭게 하여 주옵소서.

예수님의 이름으로 기도드립니다. 아멘.',
     N'마가복음 6:31', N'너희는 따로 한적한 곳에 가서 잠깐 쉬어라',
     'Y', 'Y', DATEADD(HOUR, 16, GETDATE())),

    (44, N'정직하게 걸어가게 하소서',
N'속을 보시는 하나님,

앞서가고 싶은 마음에 지름길이 눈에 들어옵니다. 남들도 다 그렇게 한다는 말 앞에서 **○○(이)**의 마음이 흔들립니다.

정직하게 걸어가게 하여 주옵소서. 남의 것을 가져다 자기 것처럼 내놓지 않게 하시고, 아직 하지 아니한 것을 하였다 말하지 않게 하여 주옵소서.

작은 것에서 지켜지게 하여 주옵소서. 아무도 보지 않는 자리에서의 한 번이 결국 습관이 되는 줄 아오니, 그 한 번을 이기게 하여 주옵소서.

느려 보여도 바른 길을 택하게 하여 주옵소서. 정직이 손해처럼 느껴지는 순간에도, 주님께서 그 걸음을 보고 계심을 믿게 하여 주옵소서.

바른 길로 행하는 자는 걸음이 평안하려니와 굽은 길로 행하는 자는 드러나리라 하였습니다. 그 말씀대로 **○○(이)**가 떳떳한 걸음으로 이 길을 지나가게 하옵소서.

예수님의 이름으로 기도드립니다. 아멘.',
     N'잠언 10:9', N'바른 길로 행하는 자는 걸음이 평안하려니와 굽은 길로 행하는 자는 드러나리라',
     'Y', 'Y', DATEADD(HOUR, 16, GETDATE()));

    PRINT N'기도문 4편 추가 완료';
END
GO

-- 줄바꿈을 CRLF 로 통일 (화면이 \r\n 로 문단을 나눕니다)
UPDATE dbo.EntranceExamPray
   SET PrayText = REPLACE(REPLACE(REPLACE(PrayText,
                    CHAR(13) + CHAR(10), CHAR(10)),
                    CHAR(13),            CHAR(10)),
                    CHAR(10),            CHAR(13) + CHAR(10))
 WHERE SortOrder IN (41, 42, 43, 44);
GO

-- 결과 확인
SELECT PrayId, SortOrder, Title, LEN(PrayText) AS len,
       (LEN(PrayText) - LEN(REPLACE(PrayText, CHAR(13)+CHAR(10)+CHAR(13)+CHAR(10), N''))) / 4 + 1 AS paras,
       RotateYN, BibleRef
  FROM dbo.EntranceExamPray
 WHERE SortOrder IN (41, 42, 43, 44)
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
