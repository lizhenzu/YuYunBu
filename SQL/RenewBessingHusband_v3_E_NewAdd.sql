-- ============================================================
--  남편을 위한 아내의 기도 — 현실 개편 v3 (E: 신규 4편)   ※ 2026-10-11 전 연령 공용으로 재작성
--  일의 자리가 흔들릴 때 / 쉬는 날 / 주님 안에서의 기쁨 / 남편에게 드리는 감사
--
--  ⚠️ 운영 DB 입니다.
--     이미 들어가 있으면(옛 제목 또는 새 제목) 그 행을 덮어쓰고, 없으면 새로 넣습니다.
--     → 몇 번을 실행해도 4편이 중복되지 않습니다.
--     새로 넣을 때 PageNo 는 현재 최대값 뒤로 이어 붙이고, visited 는 0 으로 시작합니다.
-- ============================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BessingHusband_bak_v3' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    SELECT * INTO dbo.BessingHusband_bak_v3 FROM dbo.BessingHusband;
    PRINT '[BessingHusband_bak_v3] 백업 생성';
END
ELSE
    PRINT '[BessingHusband_bak_v3] 백업이 이미 있습니다 - 건너뜀';

BEGIN TRAN;
DECLARE @T DATETIME = DATEADD(HOUR,16,GETDATE());
DECLARE @Title NVARCHAR(200), @OldTitle NVARCHAR(200), @Text NVARCHAR(MAX), @Word NVARCHAR(MAX), @N INT;

-- ── 1 ─────────────────────────────────────────────
SET @N = 43;
SET @OldTitle = N'이직과 구조조정 앞에 선 남편을 위한 기도';
SET @Title    = N'일의 자리가 흔들리는 남편을 위한 기도';
SET @Text = N'길을 여시는 주님,

남편의 일의 자리가 흔들리고 있습니다. 계속 이 자리에 있어야 할지, 새로운 길을 찾아야 할지, 아니면 잠시 멈추어야 할지 어느 쪽도 쉽게 고를 수 없는 마음이 얼마나 무거운지 주님이 아십니다. 괜찮다고 말하면서도 밤마다 생각이 깊어지는 남편을 봅니다.

주님, 남편의 자리를 붙들어 주세요. 지금의 자리에 있게 하실 수도, 새로운 문을 여실 수도 있음을 믿습니다. 어느 쪽이든 남편이 자존심을 다치지 않게 하시고, 일이 흔들린다는 두려움이 남편의 존재 전체를 흔들지 않게 하옵소서. 남편의 가치는 하는 일이 아니라 주님의 자녀라는 데 있음을 잊지 않게 하옵소서.

기다림이 길어져도 낙심하지 않게 하옵소서. 남편의 성실과 경험을 알아봐 주는 사람을 만나게 하시고, 맞는 때에 맞는 문이 열리게 하옵소서. 그 시간 동안에도 주님이 우리 가정을 먹이신다는 믿음을 붙들게 해 주세요.

저도 남편 곁에서 흔들리지 않겠습니다. 불안을 숨기지 않으면서도 키우지 않고, 함께 살림을 점검하며, 어떤 결정을 하든 나는 당신 편이라고 말해 주는 아내가 되겠습니다.

예수님의 이름으로 기도드립니다.
아멘.';
SET @Word = N'이사야 43:2
"네가 물 가운데로 지날 때에 내가 너와 함께 할 것이라 강을 건널 때에 물이 너를 침몰하지 못할 것이며 네가 불 가운데로 지날 때에 타지도 아니할 것이요 불꽃이 너를 사르지도 못하리니"

시편 37:5
"너의 길을 여호와께 맡기라 그를 의지하면 그가 이루시고"

로마서 8:28
"우리가 알거니와 하나님을 사랑하는 자 곧 그의 뜻대로 부르심을 입은 자들에게는 모든 것이 합력하여 선을 이루느니라"';
IF EXISTS (SELECT 1 FROM dbo.BessingHusband WHERE PrayTitle IN (@OldTitle, @Title))
    UPDATE dbo.BessingHusband SET PrayTitle = @Title, PrayText = @Text, PrayHolyWord = @Word, UpdateDT = DATEADD(MINUTE,@N,@T)
     WHERE PrayTitle IN (@OldTitle, @Title);
ELSE
    INSERT INTO dbo.BessingHusband (PageNo, PrayTitle, PrayText, PrayHolyWord, InsertDT, UpdateDT)
    VALUES (ISNULL((SELECT MAX(PageNo) FROM dbo.BessingHusband),0) + 1, @Title, @Text, @Word, DATEADD(MINUTE,@N,@T), DATEADD(MINUTE,@N,@T));

-- ── 2 ─────────────────────────────────────────────
SET @N = 44;
SET @OldTitle = N'쉬는 날 남편에게 진짜 쉼을 주소서';
SET @Title    = N'쉬는 날 남편에게 진짜 쉼을 주소서';
SET @Text = N'안식을 주시는 주님,

쉬는 날이 되면 남편은 늦게까지 자고, 일어나서는 누워서 화면을 봅니다. 그러다 다시 바쁜 날이 다가온다는 생각에 표정이 어두워집니다. 쉬는 날인데도 마음은 쉬지 못하는 남편을 보며 마음이 아픕니다. 쉬었는데도 피곤한 날이 이어지는 것을 압니다.

주님, 남편의 쉬는 날이 진짜 쉼이 되게 하옵소서. 몸만 눕는 것이 아니라 마음도 일에서 완전히 내려놓게 하시고, 다가올 걱정을 미리 당겨 하지 않게 하옵소서. 가까운 곳이라도 걸으며 햇볕을 쬐고 바람을 맞는 시간이 쉼이 되게 하옵소서.

그 쉼 속에서 함께하는 시간도 기쁨이 되게 하옵소서. 함께 먹는 한 끼, 짧은 산책, 마주 앉아 마시는 차 한 잔이 남편에게 새 힘이 되게 해 주세요. 쉬는 날이 또 다른 일이 되지 않도록 할 일을 줄이고 느리게 보내는 지혜를 주옵소서.

저도 남편의 쉬는 날을 지켜 주겠습니다. 쉬는 날이라고 밀린 일을 한꺼번에 부탁하지 않고, 남편이 원하는 방식의 쉼을 존중하며, 가끔은 아무 일정도 없는 하루를 함께 비워 두는 아내가 되겠습니다.

예수님의 이름으로 기도드립니다.
아멘.';
SET @Word = N'마가복음 6:31
"이르시되 너희는 따로 한적한 곳에 가서 잠깐 쉬어라 하시니 이는 오고 가는 사람이 많아 음식 먹을 겨를도 없음이라"

마가복음 2:27
"또 이르시되 안식일이 사람을 위하여 있는 것이요 사람이 안식일을 위하여 있는 것이 아니니"

시편 23:2-3
"그가 나를 푸른 풀밭에 누이시며 쉴 만한 물 가로 인도하시는도다 내 영혼을 소생시키시고 자기 이름을 위하여 의의 길로 인도하시는도다"';
IF EXISTS (SELECT 1 FROM dbo.BessingHusband WHERE PrayTitle IN (@OldTitle, @Title))
    UPDATE dbo.BessingHusband SET PrayTitle = @Title, PrayText = @Text, PrayHolyWord = @Word, UpdateDT = DATEADD(MINUTE,@N,@T)
     WHERE PrayTitle IN (@OldTitle, @Title);
ELSE
    INSERT INTO dbo.BessingHusband (PageNo, PrayTitle, PrayText, PrayHolyWord, InsertDT, UpdateDT)
    VALUES (ISNULL((SELECT MAX(PageNo) FROM dbo.BessingHusband),0) + 1, @Title, @Text, @Word, DATEADD(MINUTE,@N,@T), DATEADD(MINUTE,@N,@T));

-- ── 3 ─────────────────────────────────────────────
SET @N = 45;
SET @OldTitle = N'자녀 앞에서 한마음이 되는 부모가 되게 하소서';
SET @Title    = N'남편이 주님 안에서 기쁨을 누리게 하소서';
SET @Text = N'기쁨의 근원이신 주님,

남편이 마지막으로 마음껏 웃은 것이 언제였는지 생각해 보았습니다. 해야 할 일과 책임 사이에서 하루하루를 버티다 보니, 즐거움은 늘 다음으로 미뤄 두는 것이 되어 버렸습니다. 남편의 얼굴에서 웃음이 줄어든 것을 주님도 보고 계십니다.

주님, 남편에게 주님 안에서의 기쁨을 회복시켜 주세요. 형편이 좋아서 생기는 기쁨이 아니라, 주님이 함께하신다는 사실에서 솟아나는 기쁨을 맛보게 하옵소서. 바쁜 하루 속에서도 감사할 것을 발견하는 눈을 주시고, 작은 것에도 웃을 수 있는 여유를 허락해 주세요.

남편이 좋아하는 것을 즐기는 시간이 있게 하옵소서. 운동이든 음악이든 사람과의 만남이든, 남편의 마음을 살아나게 하는 일들을 미안해하지 않고 누리게 하시고, 그 기쁨이 다시 가정으로 흘러넘치게 해 주세요.

저는 남편의 웃음을 지켜 주는 아내가 되겠습니다. 남편이 좋아하는 것을 함께 즐기고, 집에서 함께 웃을 일을 자주 만들며, 남편이 기뻐할 때 누구보다 크게 기뻐해 주겠습니다.

예수님의 이름으로 기도드립니다.
아멘.';
SET @Word = N'빌립보서 4:4
"주 안에서 항상 기뻐하라 내가 다시 말하노니 기뻐하라"

느헤미야 8:10
"근심하지 말라 여호와로 인하여 기뻐하는 것이 너희의 힘이니라"

시편 16:11
"주께서 생명의 길을 내게 보이시리니 주의 앞에는 충만한 기쁨이 있고 주의 오른쪽에는 영원한 즐거움이 있나이다"';
IF EXISTS (SELECT 1 FROM dbo.BessingHusband WHERE PrayTitle IN (@OldTitle, @Title))
    UPDATE dbo.BessingHusband SET PrayTitle = @Title, PrayText = @Text, PrayHolyWord = @Word, UpdateDT = DATEADD(MINUTE,@N,@T)
     WHERE PrayTitle IN (@OldTitle, @Title);
ELSE
    INSERT INTO dbo.BessingHusband (PageNo, PrayTitle, PrayText, PrayHolyWord, InsertDT, UpdateDT)
    VALUES (ISNULL((SELECT MAX(PageNo) FROM dbo.BessingHusband),0) + 1, @Title, @Text, @Word, DATEADD(MINUTE,@N,@T), DATEADD(MINUTE,@N,@T));

-- ── 4 ─────────────────────────────────────────────
SET @N = 46;
SET @OldTitle = N'내 남편이 되어 준 것에 감사하는 기도';
SET @Title    = N'내 남편이 되어 준 것에 감사하는 기도';
SET @Text = N'모든 좋은 것을 주시는 주님,

오늘은 구할 것이 많은 날이 아니라 감사할 것이 많은 날입니다. 평소에는 부족한 점만 눈에 들어오고 해 준 일은 당연하게 여기던 저를 용서해 주세요. 가만히 생각해 보니 남편이 해 준 일들이 참 많습니다.

제가 아팠던 날 말없이 곁을 지켜 준 일, 힘들어하는 저를 위해 서툰 솜씨로 밥을 차려 준 일, 누구도 알아주지 않는 수많은 수고를 불평 없이 감당해 온 일들이 떠오릅니다. 표현은 서툴러도 늘 같은 자리에서 저를 지켜 준 사람입니다.

주님, 이런 남편을 허락해 주셔서 감사합니다. 부족한 저를 선택하고 지금까지 함께 걸어 준 남편에게 오늘 감사한 마음을 전할 용기를 주세요. 고맙다는 말, 수고했다는 말, 사랑한다는 말을 아끼지 않는 아내가 되게 하옵소서.

남편의 수고가 잊히지 않게 하시고, 남편이 베푼 사랑이 몇 배의 복으로 남편에게 돌아가게 하옵소서. 어떤 날이 와도 지금 이 감사를 잊지 않게 하시고, 서로를 선물로 여기며 살아가는 부부가 되게 해 주세요.

예수님의 이름으로 기도드립니다.
아멘.';
SET @Word = N'빌립보서 1:3
"내가 너희를 생각할 때마다 나의 하나님께 감사하며"

데살로니가전서 5:18
"범사에 감사하라 이것이 그리스도 예수 안에서 너희를 향하신 하나님의 뜻이니라"

시편 103:2
"내 영혼아 여호와를 송축하며 그의 모든 은택을 잊지 말지어다"';
IF EXISTS (SELECT 1 FROM dbo.BessingHusband WHERE PrayTitle IN (@OldTitle, @Title))
    UPDATE dbo.BessingHusband SET PrayTitle = @Title, PrayText = @Text, PrayHolyWord = @Word, UpdateDT = DATEADD(MINUTE,@N,@T)
     WHERE PrayTitle IN (@OldTitle, @Title);
ELSE
    INSERT INTO dbo.BessingHusband (PageNo, PrayTitle, PrayText, PrayHolyWord, InsertDT, UpdateDT)
    VALUES (ISNULL((SELECT MAX(PageNo) FROM dbo.BessingHusband),0) + 1, @Title, @Text, @Word, DATEADD(MINUTE,@N,@T), DATEADD(MINUTE,@N,@T));

COMMIT TRAN;

-- 개행 정규화 (LF → CRLF) — 신규 4편만
BEGIN TRAN;
UPDATE dbo.BessingHusband
SET PrayText = REPLACE(REPLACE(REPLACE(PrayText, CHAR(13)+CHAR(10), CHAR(10)), CHAR(13), CHAR(10)), CHAR(10), CHAR(13)+CHAR(10)),
    PrayHolyWord = REPLACE(REPLACE(REPLACE(PrayHolyWord, CHAR(13)+CHAR(10), CHAR(10)), CHAR(13), CHAR(10)), CHAR(10), CHAR(13)+CHAR(10))
WHERE PrayTitle IN (N'일의 자리가 흔들리는 남편을 위한 기도', N'쉬는 날 남편에게 진짜 쉼을 주소서', N'남편이 주님 안에서 기쁨을 누리게 하소서', N'내 남편이 되어 준 것에 감사하는 기도');
COMMIT TRAN;

PRINT '[BessingHusband] v3-E 신규 4편 반영 완료';
SELECT COUNT(*) AS [총 편수] FROM dbo.BessingHusband;
GO
