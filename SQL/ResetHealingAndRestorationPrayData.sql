-- ============================================================
--  YunYunBu2025 — 환자를 위한 기도문 : 전체 삭제 후 재생성
--
--  주제: 하나님께 치료 · 회복 · 은혜를 구하는 기도 (10편)
--        상황별로 나누지 않고, 같은 종류의 기도를 조금씩 달리 반복합니다.
--
--  ⚠️ 기존 기도문과 조회수(ClickCount)가 모두 지워집니다.
--     실행 전: USE [YourDatabaseName] 확인
--
--  * 테이블이 없으면 만들고, 있으면 내용만 갈아끼웁니다.
--  * 여러 번 실행해도 항상 아래 10편 상태가 됩니다.
--  * 10편이므로 10일 주기로 순환합니다. 나중에 늘리면 그 건수대로 순환합니다.
-- ============================================================

-- ────────────────────────────────────────────────────────────
--  0) 테이블이 없으면 생성
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.tables
    WHERE name = 'HealingAndRestorationPray' AND schema_id = SCHEMA_ID('dbo')
)
BEGIN
    CREATE TABLE [dbo].[HealingAndRestorationPray] (
        [PrayId]     INT             IDENTITY(1,1) NOT NULL,
        [SortOrder]  INT             NOT NULL CONSTRAINT [DF_HealingAndRestorationPray_SortOrder]  DEFAULT (0),
        [Title]      NVARCHAR(200)   NOT NULL,
        [PrayText]   NVARCHAR(MAX)   NOT NULL,
        [BibleRef]   NVARCHAR(100)   NULL,
        [BibleText]  NVARCHAR(1000)  NULL,
        [ClickCount] INT             NOT NULL CONSTRAINT [DF_HealingAndRestorationPray_ClickCount] DEFAULT (0),
        [UseYN]      CHAR(1)         NOT NULL CONSTRAINT [DF_HealingAndRestorationPray_UseYN]      DEFAULT ('Y'),
        [CreatedAt]  DATETIME        NOT NULL CONSTRAINT [DF_HealingAndRestorationPray_CreatedAt]  DEFAULT (GETDATE()),
        [UpdatedAt]  DATETIME        NULL,
        CONSTRAINT [PK_HealingAndRestorationPray] PRIMARY KEY CLUSTERED ([PrayId] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_HealingAndRestorationPray_UseYN_SortOrder]
        ON [dbo].[HealingAndRestorationPray] ([UseYN] ASC, [SortOrder] ASC, [PrayId] ASC);

    PRINT '[HealingAndRestorationPray] 테이블 생성 완료';
END
GO

-- ────────────────────────────────────────────────────────────
--  1) 기존 기도문 전체 삭제 + 번호 초기화
-- ────────────────────────────────────────────────────────────
DECLARE @before INT = (SELECT COUNT(*) FROM [dbo].[HealingAndRestorationPray]);

DELETE FROM [dbo].[HealingAndRestorationPray];
DBCC CHECKIDENT ('[dbo].[HealingAndRestorationPray]', RESEED, 0) WITH NO_INFOMSGS;

PRINT CONCAT('[HealingAndRestorationPray] 기존 기도문 ', @before, '건 삭제 완료');
GO

-- ────────────────────────────────────────────────────────────
--  2) 치료 · 회복 · 은혜를 구하는 기도 10편
-- ────────────────────────────────────────────────────────────
INSERT INTO [dbo].[HealingAndRestorationPray] ([SortOrder], [Title], [PrayText], [BibleRef], [BibleText])
VALUES
(1, N'주님께 맡기며 치료를 구합니다',
 N'선하신 하나님, 제 뜻대로 되기를 얼마나 바랐는지 아십니다. 낫게 해 주시기를 오늘도 구합니다. 그러나 주님의 길이 제 계산보다 선한 줄 믿으니, 쥐고 있던 손을 펴고 주님께 올려 드립니다. 어느 길로 인도하셔도 그 길에서 주님을 놓지 않게 해 주세요. 예수님의 이름으로 기도합니다. 아멘.',
 N'누가복음 22:42', N'내 원대로 마시옵고 아버지의 원대로 되기를 원하나이다'),

(2, N'고치시는 주님을 믿습니다',
 N'고치시는 하나님, 주님이 하실 수 있음을 믿습니다. 이 병을 거두어 주시기를 오늘도 구합니다. 사람의 손이 닿지 못하는 곳까지 주님의 손은 닿는 줄 아오니, 이 몸을 주님 앞에 그대로 내어 놓습니다. 고쳐 주시든 견디게 하시든 그 안에 주님의 선하심이 있음을 믿게 해 주세요. 예수님의 이름으로 기도합니다. 아멘.',
 N'마태복음 8:2', N'주여 원하시면 저를 깨끗하게 하실 수 있나이다'),

(3, N'회복을 구합니다',
 N'회복시키시는 하나님, 무너진 이 몸을 다시 세워 주시기를 구합니다. 오늘도 낫게 해 주시기를, 어제보다 나은 하루를 주시기를 간구합니다. 회복이 제 바람보다 더뎌도 주님이 손을 놓으신 것이 아님을 믿습니다. 조급한 마음을 내려놓고 주님의 때를 기다리게 해 주세요. 예수님의 이름으로 기도합니다. 아멘.',
 N'예레미야 30:17', N'내가 너의 상처로부터 새 살이 돋아나게 하여 너를 고쳐 주리라'),

(4, N'은혜를 구합니다',
 N'은혜의 하나님, 제가 무엇을 잘해서 구하는 것이 아닙니다. 그저 주님의 긍휼에 기대어 낫게 해 주시기를 구합니다. 받을 자격 없는 저에게 오늘까지 숨을 주신 것도 은혜인 줄 압니다. 이 몸에 은혜를 부어 주시고, 은혜로 사는 법을 배우게 해 주세요. 예수님의 이름으로 기도합니다. 아멘.',
 N'고린도후서 12:9', N'내 은혜가 네게 족하도다 이는 내 능력이 약한 데서 온전하여짐이라'),

(5, N'응답을 기다리며 다시 구합니다',
 N'오래 참으시는 하나님, 기도한 지 오래되었는데 아직 그대로입니다. 그래도 오늘 다시 구합니다. 낫게 해 주세요. 응답이 없는 것 같은 이 시간에도 주님은 침묵하고 계시지 않음을 믿습니다. 지치지 않게 붙들어 주시고, 끝까지 구하는 사람이 되게 해 주세요. 예수님의 이름으로 기도합니다. 아멘.',
 N'누가복음 18:1', N'항상 기도하고 낙심하지 말아야 할 것을 비유로 말씀하여'),

(6, N'주님의 손이 닿기를 구합니다',
 N'만지시는 하나님, 주님의 손이 이 몸에 닿기를 구합니다. 아픈 자리마다 주님의 손이 지나가게 하시고 상한 곳을 고쳐 주세요. 약과 치료를 주신 분도 주님이시니 그것들이 이 몸에 잘 듣게 하옵소서. 주님이 하시는 일을 제 눈으로 보게 해 주세요. 예수님의 이름으로 기도합니다. 아멘.',
 N'마가복음 1:41', N'예수께서 민망히 여기사 손을 내밀어 그에게 대시며 이르시되 내가 원하노니 깨끗함을 받으라'),

(7, N'오늘 하루의 힘을 구합니다',
 N'힘 주시는 하나님, 오늘 하루 살아 낼 힘이 없습니다. 낫게 해 주시기를 구하며, 낫기까지 버틸 힘도 함께 구합니다. 아침마다 새로운 주님의 자비를 오늘도 부어 주세요. 제 힘이 다한 그 자리에서 주님의 힘이 시작됨을 믿습니다. 예수님의 이름으로 기도합니다. 아멘.',
 N'예레미야애가 3:23', N'이것들이 아침마다 새로우니 주의 성실하심이 크시도소이다'),

(8, N'잊지 않으심을 믿으며 구합니다',
 N'신실하신 하나님, 낫게 해 주시기를 오늘도 구합니다. 언제 응답하실지 저는 모르지만 주님이 저를 잊지 않으셨음은 압니다. 기다리는 동안 원망이 자라지 않게 하시고 믿음이 더 깊어지게 해 주세요. 응답의 날에 가장 먼저 주님께 감사하게 하옵소서. 예수님의 이름으로 기도합니다. 아멘.',
 N'이사야 49:15', N'여인이 어찌 그 젖 먹는 자식을 잊겠으며 그의 태에서 난 아들을 긍휼히 여기지 않겠느냐'),

(9, N'아픔 중에 붙들어 주시기를 구합니다',
 N'함께하시는 하나님, 아픔이 저를 흔듭니다. 이 아픔을 거두어 주시기를 구하며, 거두어지기 전까지 저를 붙들어 주시기를 구합니다. 주님이 곁에 계시니 이 자리가 버려진 자리가 아님을 믿습니다. 아픔보다 주님이 크심을 오늘 알게 해 주세요. 예수님의 이름으로 기도합니다. 아멘.',
 N'이사야 41:10', N'두려워하지 말라 내가 너와 함께 함이라 내가 너를 굳세게 하리라 참으로 너를 도와 주리라'),

(10, N'온전한 회복과 감사를 구합니다',
 N'완전하신 하나님, 반쪽짜리가 아닌 온전한 회복을 구합니다. 이 몸을 고쳐 주시고 마음도 함께 고쳐 주세요. 나은 뒤에 주님을 잊는 사람이 되지 않게 하시고, 회복의 첫날부터 감사로 살게 하옵소서. 다시 주신 이 몸으로 주님께 영광 돌리게 해 주세요. 예수님의 이름으로 기도합니다. 아멘.',
 N'시편 103:2', N'내 영혼아 여호와를 송축하며 그의 모든 은택을 잊지 말지어다');
GO

-- ────────────────────────────────────────────────────────────
--  3) 결과 확인
-- ────────────────────────────────────────────────────────────
DECLARE @after INT = (SELECT COUNT(*) FROM [dbo].[HealingAndRestorationPray]);
PRINT CONCAT('[HealingAndRestorationPray] 새 기도문 ', @after, '건 삽입 완료');

SELECT [SortOrder], [Title], LEN([PrayText]) AS [글자수], [BibleRef]
FROM   [dbo].[HealingAndRestorationPray]
ORDER  BY [SortOrder];
GO
