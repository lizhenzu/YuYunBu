-- ============================================================
--  아내를 위한 남편의 기도 — Id 58 "명절에 수고하는 아내를 위한 기도" 교체 (2026-10-12)
--  특정일(명절) 기도 → 언제든 드릴 수 있는 "아내의 보이지 않는 수고를 알아주는 기도"
--
--  ⚠️ 운영 DB 입니다.  실행 전 백업표(BessingWife_bak_20261012)가 자동으로 만들어집니다.
--     PrayTitle · PrayText · PrayHolyWord · UpdateDT 만 바꿉니다.  PageNo · visited · InsertDT 는 건드리지 않습니다.
--     RenewBessingWife_v3_F.sql 을 이미 실행했어도, 아직 안 했어도 이 파일을 실행하면 됩니다. (v3_F 도 같은 내용으로 고쳐 두었습니다)
-- ============================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BessingWife_bak_20261012' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    SELECT * INTO dbo.BessingWife_bak_20261012 FROM dbo.BessingWife;
    PRINT '[BessingWife_bak_20261012] 백업 생성';
END
ELSE
    PRINT '[BessingWife_bak_20261012] 백업이 이미 있습니다 - 건너뜀';

BEGIN TRAN;

UPDATE dbo.BessingWife SET
 PrayTitle = N'아내의 보이지 않는 수고를 알아주는 기도',
 PrayText = N'수고하는 자를 쉬게 하시는 주님,

집 안에는 아무도 알아주지 않는 일이 참 많습니다. 비어 있는 냉장고가 채워져 있고, 쌓여 있던 빨래가 개어져 있고, 어지럽던 방이 어느새 정돈되어 있습니다. 누가 했는지 묻지 않아도 아내가 한 일이라는 것을 압니다. 그런데 저는 그 수고를 당연하게 여기며 살았습니다.

주님, 아내의 수고를 주님이 아시고 기억해 주세요. 아무도 알아주지 않아도 하루하루 가정을 지켜 온 아내의 손길이 헛되지 않게 하시고, 지친 몸과 마음에 주님이 주시는 쉼과 새 힘을 부어 주옵소서. 아내가 자신의 수고를 스스로 하찮게 여기지 않게 해 주세요.

그 수고가 아내 혼자만의 몫이 되지 않게 하옵소서. 서로의 짐을 나누어 지는 가정이 되게 하시고, 우리 집에 고맙다는 말과 수고했다는 말이 아낌없이 오가게 해 주세요.

저는 오늘부터 아내가 한 일을 눈여겨보겠습니다. 말하지 않아도 보이는 일은 제가 먼저 하고, 아내가 해 둔 일에는 고맙다는 말을 빼놓지 않으며, 아내가 아무것도 하지 않고 쉴 수 있는 시간을 일부러 만들어 주는 남편이 되겠습니다.

예수님의 이름으로 기도드립니다.
아멘.',
 PrayHolyWord = N'마태복음 11:28
"수고하고 무거운 짐 진 자들아 다 내게로 오라 내가 너희를 쉬게 하리라"

히브리서 6:10
"하나님은 불의하지 아니하사 너희 행위와 그의 이름을 위하여 나타낸 사랑으로 이미 성도를 섬겼고 이제도 섬기고 있음을 잊어버리지 아니하시느니라"

갈라디아서 6:2
"너희가 짐을 서로 지라 그리하여 그리스도의 법을 성취하라"',
 UpdateDT = GETDATE()
WHERE Id = 58 AND PrayTitle IN (N'명절에 수고하는 아내를 위한 기도', N'아내의 보이지 않는 수고를 알아주는 기도');

IF @@ROWCOUNT <> 1
BEGIN
    ROLLBACK TRAN;
    PRINT '⚠️ Id 58 이 예상한 기도문이 아니라서 중단했습니다 - 아무것도 바꾸지 않았습니다.';
END
ELSE
BEGIN
    COMMIT TRAN;
    PRINT 'Id 58 교체 완료';
END

SELECT Id, PageNo, PrayTitle, UpdateDT FROM dbo.BessingWife WHERE Id = 58;
