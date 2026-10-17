-- 성경통독 완독 선물 : '전달 완료' 단계 추가
SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF COL_LENGTH('dbo.BibleGiftOrder', 'DeliveredAt') IS NULL
BEGIN
    ALTER TABLE dbo.BibleGiftOrder ADD DeliveredAt DATETIME NULL;
    PRINT '[BibleGiftOrder] DeliveredAt 추가';
END
ELSE PRINT '[BibleGiftOrder] DeliveredAt 이미 있음';
GO
