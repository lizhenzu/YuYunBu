-- ════════════════════════════════════════════════════════════════
--  추석 가족 사진 콘테스트 — 가짜 참가 가족 지우기 (2026-09-27)
--  대상: EntryId 7 · 가족명 "ㅇㅇㅇ" · 2026-09-22 06:16 · 사진 0장 · 좋아요 0
--  EntryId 와 가족명이 둘 다 맞을 때만 지운다 (다른 가족은 건드리지 않음)
--  순서: 좋아요 → 사진 → 가족  (한 트랜잭션, 1줄이 아니면 되돌림)
-- ════════════════════════════════════════════════════════════════
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @EntryId INT = 7;

-- 0) 지우기 전 확인
SELECT e.EntryId, e.FamilyName, e.CreatedAt,
       (SELECT COUNT(*) FROM dbo.FamilyPhotoItem p WHERE p.EntryId = e.EntryId) AS Photos
FROM dbo.FamilyPhotoEntry e
WHERE e.EntryId = @EntryId AND e.FamilyName = N'ㅇㅇㅇ';

-- 1) 좋아요 (지금 0건)
DELETE l
FROM dbo.FamilyPhotoLike l
JOIN dbo.FamilyPhotoItem p ON p.PhotoId = l.PhotoId
WHERE p.EntryId = @EntryId;

-- 2) 사진 (지금 0장)
DELETE FROM dbo.FamilyPhotoItem WHERE EntryId = @EntryId;

-- 3) 가족
DELETE FROM dbo.FamilyPhotoEntry WHERE EntryId = @EntryId AND FamilyName = N'ㅇㅇㅇ';

IF @@ROWCOUNT <> 1
BEGIN
    ROLLBACK TRAN;
    RAISERROR(N'가족 1줄이 지워지지 않아 되돌렸어요. EntryId · 가족명을 확인하세요.', 16, 1);
    RETURN;
END

COMMIT TRAN;

-- 4) 지운 뒤 확인 (0줄이면 끝)
SELECT * FROM dbo.FamilyPhotoEntry WHERE EntryId = 7;
