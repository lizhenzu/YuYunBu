-- ============================================================
-- 가족팀 마일스톤 시상/박수 테이블 설계
-- 생성일: 2026-08-16
-- 설명: 점수 임계값별로 시상(상품선택) 또는 박수(알림만) 동작 설정
-- ============================================================

-- ── 1. 마일스톤 설정 테이블 ──────────────────────────────────
-- 연도별 / 점수 임계값별로 어떤 이벤트를 발생시킬지 정의
CREATE TABLE [dbo].[DM2_FamilyMilestoneConfig]
(
    [Id]             INT           IDENTITY(1,1) NOT NULL,
    [Year]           INT           NOT NULL,              -- 연도 (2025, 2026 ...)
    [PointThreshold] INT           NOT NULL,              -- 임계 점수 (1000, 2000, 3000, ...)
    [MilestoneType]  NVARCHAR(20)  NOT NULL,              -- 'AWARD'(시상) | 'APPLAUSE'(박수)
    [DisplayTitle]   NVARCHAR(200) NOT NULL,              -- 홈 배너에 표시할 제목
    [DisplayEmoji]   NVARCHAR(10)  NULL,                  -- 배너 이모지 (🏆, 👏 등)
    [IsActive]       BIT           NOT NULL DEFAULT 1,    -- 비활성화 시 홈에 안 보임
    [SortOrder]      INT           NOT NULL DEFAULT 0,    -- 홈 표시 순서
    [CreatedAt]      DATETIME2     NOT NULL DEFAULT GETDATE(),

    CONSTRAINT [PK_DM2_FamilyMilestoneConfig] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_DM2FMC_YearPoint] UNIQUE ([Year], [PointThreshold]),
    CONSTRAINT [CK_DM2FMC_Type] CHECK ([MilestoneType] IN ('AWARD', 'APPLAUSE'))
);
GO

-- ── 2. 박수(APPLAUSE) 마일스톤 팀별 숨기기 테이블 ─────────────
-- APPLAUSE 타입: 팀이 "숨기기" 버튼을 누르면 이 테이블에 기록
-- AWARD 타입: StudentPointGiftOrder.Status = 'HIDDEN' 로 처리 (기존 로직)
CREATE TABLE [dbo].[DM2_FamilyMilestoneHidden]
(
    [Id]                INT       IDENTITY(1,1) NOT NULL,
    [MilestoneConfigId] INT       NOT NULL,               -- FK → DM2_FamilyMilestoneConfig.Id
    [FamilyTeamId]      INT       NOT NULL,               -- FK → DM2_FamilyTeam.FamilyTeamId
    [HiddenAt]          DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT [PK_DM2_FamilyMilestoneHidden] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DM2FMH_Config] FOREIGN KEY ([MilestoneConfigId])
        REFERENCES [dbo].[DM2_FamilyMilestoneConfig]([Id]),
    CONSTRAINT [UQ_DM2FMH_ConfigTeam] UNIQUE ([MilestoneConfigId], [FamilyTeamId])
);
GO

-- ── 3. 인덱스 ────────────────────────────────────────────────
CREATE INDEX [IX_DM2FMC_Year_Active]
    ON [dbo].[DM2_FamilyMilestoneConfig] ([Year], [IsActive]);
GO

CREATE INDEX [IX_DM2FMH_ConfigId]
    ON [dbo].[DM2_FamilyMilestoneHidden] ([MilestoneConfigId]);
GO

-- ── 4. 2025년 초기 데이터 ────────────────────────────────────
-- 1000점: 시상 (상품 선택)
-- 2000점: 박수만 (알림 + 숨기기)
-- 3000점: 시상 (상품 선택)
-- 4000점: 박수만
INSERT INTO [dbo].[DM2_FamilyMilestoneConfig]
    ([Year], [PointThreshold], [MilestoneType], [DisplayTitle], [DisplayEmoji], [SortOrder])
VALUES
    (2025, 1000, 'AWARD',    N'가족팀 합산 1,000점 달성! 선물을 골라보세요 🎁', N'🏆', 1),
    (2025, 2000, 'APPLAUSE', N'가족팀 합산 2,000점 달성! 대단해요 👏',          N'👏', 2),
    (2025, 3000, 'AWARD',    N'가족팀 합산 3,000점 달성! 선물을 골라보세요 🎁', N'🎉', 3),
    (2025, 4000, 'APPLAUSE', N'가족팀 합산 4,000점 달성! 정말 대단해요 🌟',     N'🌟', 4);
GO

-- ── 5. AWARD 마일스톤 선물 주문 메모 키 규칙 ──────────────────
-- StudentPointGiftOrder.Memo 포맷:
--   1000점: '가족합산:{TeamName}'          ← 기존 레코드 하위호환
--   3000점: '가족합산:{TeamName}:3000'     ← 신규 포맷
-- 조회 시 LIKE '가족합산:%' 로 전체 조회 후 C# 에서 파싱
-- ============================================================
