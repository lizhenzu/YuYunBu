-- 2026-10-06  가족 사진 콘테스트 · AI 리뷰 보드 스티커 표
-- 사진마다 기기(쿠키) 하나에 스티커 하나 (Kind 0=🥰 1=😂 2=👍 3=🙏).  여러 번 실행해도 안전합니다.
IF OBJECT_ID(N'dbo.FamilyPhotoSticker', N'U') IS NULL
CREATE TABLE dbo.FamilyPhotoSticker (
    StickerId  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FamilyPhotoSticker PRIMARY KEY,
    PhotoId    INT          NOT NULL,
    DeviceKey  NVARCHAR(64) NOT NULL,
    Kind       TINYINT      NOT NULL,
    Ip         NVARCHAR(64) NULL,
    CreatedAt  DATETIME2    NOT NULL
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_FamilyPhotoSticker_PhotoDevice')
    CREATE UNIQUE INDEX UX_FamilyPhotoSticker_PhotoDevice ON dbo.FamilyPhotoSticker (PhotoId, DeviceKey);
GO
SELECT COUNT(*) AS StickerRows FROM dbo.FamilyPhotoSticker;
