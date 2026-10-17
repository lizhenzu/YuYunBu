-- ============================================================
--  치유와 회복 기도문 — 단정형을 간구·추측형으로
--
--  기도하는 이는 아픈 사람의 속마음을 알 수 없습니다. "마음이 자주 흔들립니다" 처럼
--  단정해 버리면 남의 속을 사실로 아는 것처럼 되어 버립니다. 그럴 수 있으니 기도하는 것이므로
--  "…하지는 않는지요", "…할 때가 있을 것입니다", "주님께서 아십니다" 로 바꿉니다.
--
--  ※ SoftenAssertions_EntranceExamPray.sql 과 같은 원칙 · 같은 어투입니다.
--
--  · 문제가 되는 문장은 모두 각 기도문의 첫 문단(상황 서술)에 있습니다.
--  · 간구 문단("…하여 주옵소서")은 손대지 않습니다.
--  · 화자 자신의 마음("저의 마음이 주저앉습니다")은 알 수 있는 것이므로 그대로 둡니다.
--  · 곁에서 눈으로 볼 수 있는 것(약을 넘기지 못함 등)도 단정이 아니므로 그대로 둡니다.
--  · REPLACE 로 해당 문장만 바꾸므로, 이미 고쳐진 행은 그대로 지나갑니다 (여러 번 실행해도 안전).
--  · UPDATE 만 씁니다. PrayId 는 바뀌지 않습니다.
-- ============================================================
SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HealingAndRestorationPray_bak_v6' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    SELECT * INTO dbo.HealingAndRestorationPray_bak_v6 FROM dbo.HealingAndRestorationPray;
    PRINT '[HealingAndRestorationPray_bak_v6] 백업 생성';
END
ELSE
    PRINT '[HealingAndRestorationPray_bak_v6] 백업이 이미 있습니다 — 건너뜀';


-- ── 2 무너진 이 몸의 회복을 구합니다 ──────────────────
--    "그 몸은 여전히 무겁고" → 남의 몸이 어떻게 느껴지는지는 알 수 없습니다.
UPDATE dbo.HealingAndRestorationPray SET PrayText = REPLACE(PrayText,
N'그러나 그 몸은 여전히 무겁고, 지켜보는 저의 마음도 자꾸 주저앉습니다.',
N'그러나 눈에 보이는 것은 어제와 크게 다르지 아니하니, 그 몸이 여전히 무겁지는 않은지요. 지켜보는 저의 마음도 자꾸 주저앉습니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE PrayId = 2;


-- ── 14 두려움 대신 평안을 구합니다 ────────────────────
UPDATE dbo.HealingAndRestorationPray SET PrayText = REPLACE(PrayText,
N'**○○(이)**의 마음이 자주 흔들립니다. 별다른 일이 없을 때에도 문득 가슴이 내려앉고, 아직 이르지 아니한 일을 미리 근심하며 앓습니다.',
N'**○○(이)**의 마음이 자주 흔들리지는 않는지요. 별다른 일이 없을 때에도 문득 가슴이 내려앉고, 아직 이르지 아니한 일을 미리 근심하며 앓는 때가 있을 것입니다. 저희가 다 알지 못하는 그 속을 주님께서 아십니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE PrayId = 14;


-- ── 15 낙심하지 않게 붙들어 주소서 ────────────────────
UPDATE dbo.HealingAndRestorationPray SET PrayText = REPLACE(PrayText,
N'나아지는 기색이 좀처럼 보이지 아니하여 **○○(이)**의 마음이 주저앉습니다.',
N'나아지는 기색이 좀처럼 보이지 아니하니 **○○(이)**의 마음도 주저앉지는 않는지요.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE PrayId = 15;


-- ── 16 몸과 마음을 함께 고쳐 주소서 ───────────────────
UPDATE dbo.HealingAndRestorationPray SET PrayText = REPLACE(PrayText,
N'몸이 아프니 **○○(이)**의 마음도 함께 무너집니다. 아픈 곳은 한 군데이나 무너지는 것은 온 삶인 것을 주님께서 아십니다.',
N'몸이 아프면 마음도 함께 무너지기 쉬우니, **○○(이)**의 속이 지금 어떠한지요. 아픈 곳은 한 군데라 하여도 흔들리는 것은 온 삶인 것을 주님께서 아십니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE PrayId = 16;


-- ── 19 약한 자리에서 주님을 구합니다 ──────────────────
--    "감추고 버티던 힘도 이제 다 떨어졌습니다" → 속을 단정하는 자리입니다.
UPDATE dbo.HealingAndRestorationPray SET PrayText = REPLACE(PrayText,
N'**○○(이)**가 약합니다. 스스로 할 수 있는 것이 하나도 없어 그대로 엎드립니다. 감추고 버티던 힘도 이제 다 떨어졌습니다.',
N'**○○(이)**가 약하여진 자리에 있습니다. 스스로 할 수 있는 것이 없어 그대로 엎드립니다. 감추고 버티어 온 힘마저 다하지는 않았는지, 저희가 다 알지 못하는 그 속을 주님께서 아십니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE PrayId = 19;


-- ── 24 나을 때까지 참을 힘을 구합니다 ─────────────────
UPDATE dbo.HealingAndRestorationPray SET PrayText = REPLACE(PrayText,
N'**○○(이)**가 아픈 것도 힘겨우나, 언제 끝날지 알지 못한 채 곁에서 지켜보는 일이 저희를 더욱 지치게 합니다.',
N'**○○(이)**가 견디고 있는 무게를 저희는 다 알지 못하오며, 언제 끝날지 알지 못한 채 곁에서 지켜보는 일이 저희를 지치게 합니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE PrayId = 24;


-- ── 25 무너진 마음의 위로를 구합니다 ──────────────────
UPDATE dbo.HealingAndRestorationPray SET PrayText = REPLACE(PrayText,
N'오늘은 **○○(이)**의 마음이 많이 무너졌습니다. 대수롭지 않은 말에도 눈물이 나고, 아무렇지 않은 척하는 일조차 이제 버겁습니다.',
N'오늘 **○○(이)**의 마음이 많이 무너지지는 않았는지요. 대수롭지 않은 말에도 눈물이 나고, 아무렇지 않은 척하는 일조차 버거운 날이 있을 것입니다. 겉으로 드러내지 아니하는 그 마음을 주님께서 아십니다.'
), UpdatedAt = DATEADD(HOUR,16,GETDATE()) WHERE PrayId = 25;
GO


-- 줄바꿈을 CRLF 로 통일 (화면이 \r\n 로 문단을 나눕니다)
UPDATE dbo.HealingAndRestorationPray
   SET PrayText = REPLACE(REPLACE(REPLACE(PrayText,
                    CHAR(13) + CHAR(10), CHAR(10)),
                    CHAR(13),            CHAR(10)),
                    CHAR(10),            CHAR(13) + CHAR(10))
 WHERE PrayId IN (2, 14, 15, 16, 19, 24, 25);
GO


-- ────────────────────────────────────────────────────────────
--  확인 — 고쳐야 할 단정형이 남아 있는지
--  (모두 0 이라야 정상)
-- ────────────────────────────────────────────────────────────
SELECT
    SUM(CASE WHEN PrayText LIKE N'%의 마음이 자주 흔들립니다%'      THEN 1 ELSE 0 END) AS a_14,
    SUM(CASE WHEN PrayText LIKE N'%의 마음이 주저앉습니다%'          THEN 1 ELSE 0 END) AS a_15,
    SUM(CASE WHEN PrayText LIKE N'%마음도 함께 무너집니다%'          THEN 1 ELSE 0 END) AS a_16,
    SUM(CASE WHEN PrayText LIKE N'%마음이 많이 무너졌습니다%'        THEN 1 ELSE 0 END) AS a_25,
    SUM(CASE WHEN PrayText LIKE N'%힘도 이제 다 떨어졌습니다%'       THEN 1 ELSE 0 END) AS a_19,
    SUM(CASE WHEN PrayText LIKE N'%그 몸은 여전히 무겁고%'           THEN 1 ELSE 0 END) AS a_02,
    SUM(CASE WHEN PrayText LIKE N'%가 아픈 것도 힘겨우나%'           THEN 1 ELSE 0 END) AS a_24
  FROM dbo.HealingAndRestorationPray;

-- 바뀐 일곱 편의 첫 문단 확인
SELECT PrayId, SortOrder, Title, LEN(PrayText) AS len,
       LEFT(SUBSTRING(PrayText,
            CHARINDEX(CHAR(13)+CHAR(10)+CHAR(13)+CHAR(10), PrayText) + 4, 400), 220) AS first_para
  FROM dbo.HealingAndRestorationPray
 WHERE PrayId IN (2, 14, 15, 16, 19, 24, 25)
 ORDER BY SortOrder;
GO
