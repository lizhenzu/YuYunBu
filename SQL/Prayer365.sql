-- ════════════════════════════════════════════════════════════════
--  자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 날짜(월일)별 한 편  (2026-09-27)
--  Services/Prayer365/Prayer365Service.cs 의 Ddl 과 같은 내용 — 서비스가 처음 쓸 때 자동으로 만든다
-- ════════════════════════════════════════════════════════════════
IF OBJECT_ID(N'dbo.Prayer365', N'U') IS NULL
CREATE TABLE dbo.Prayer365 (
    MonthDay    CHAR(4)        NOT NULL CONSTRAINT PK_Prayer365 PRIMARY KEY,   -- '0101' ~ '1231', '0229' 포함
    Title       NVARCHAR(200)  NOT NULL CONSTRAINT DF_Prayer365_Title DEFAULT(N''),
    VerseRef    NVARCHAR(100)  NULL,
    VerseText   NVARCHAR(1000) NULL,
    Body        NVARCHAR(MAX)  NOT NULL CONSTRAINT DF_Prayer365_Body DEFAULT(N''),
    Fixes       NVARCHAR(MAX)  NULL,        -- Claude 교정 기록 JSON [{"From":"…","To":"…"}]
    IsReviewed  BIT            NOT NULL CONSTRAINT DF_Prayer365_Rev DEFAULT(0),
    UpdatedAt   DATETIME2      NOT NULL,
    UpdatedBy   NVARCHAR(100)  NULL
);

-- 2026-10-03 : 조회 수 (관리자 보기는 세지 않는다)
IF COL_LENGTH(N'dbo.Prayer365', N'ViewCount') IS NULL
    ALTER TABLE dbo.Prayer365 ADD ViewCount INT NOT NULL CONSTRAINT DF_Prayer365_View DEFAULT(0);

-- 2026-10-17 : "오늘 기도 했어요" 누른 수 (합계)
IF COL_LENGTH(N'dbo.Prayer365', N'PrayCount') IS NULL
    ALTER TABLE dbo.Prayer365 ADD PrayCount INT NOT NULL CONSTRAINT DF_Prayer365_Pray DEFAULT(0);

-- ════════════════════════════════════════════════════════════════
--  2026-10-17 : "오늘 기도 했어요" 개인별 기록 — 로그인한 회원이 누를 때마다 한 줄
--   · 한 사람 · 한 날(KST) · 한 기도문에 한 줄만 (UQ) — 하루 한 번
--   · 화면 숫자 = 이 표의 줄 수 + Prayer365.PrayCount (PrayCount 는 로그인 안 한 사람의 클릭만 올린다)
--     → 이 표를 비우면 숫자도 같이 줄어든다
-- ════════════════════════════════════════════════════════════════
IF OBJECT_ID(N'dbo.Prayer365PrayLog', N'U') IS NULL
BEGIN
CREATE TABLE dbo.Prayer365PrayLog (
    LogId     BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Prayer365PrayLog PRIMARY KEY,
    MonthDay  CHAR(4)        NOT NULL,   -- 어느 날의 기도문 ('1017')
    PrayDate  DATE           NOT NULL,   -- 기도한 날 (KST)
    UserName  NVARCHAR(256)  NOT NULL,   -- 로그인 아이디
    UserId    NVARCHAR(450)  NULL,       -- AspNetUsers.Id
    PrayedAt  DATETIME2      NOT NULL,   -- 누른 시각 (KST)
    CONSTRAINT UQ_Prayer365PrayLog UNIQUE (UserName, PrayDate, MonthDay)
);
CREATE INDEX IX_Prayer365PrayLog_Day ON dbo.Prayer365PrayLog (PrayDate, MonthDay);
END
