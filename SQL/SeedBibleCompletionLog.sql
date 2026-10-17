-- ============================================================
--  BibleCompletionLog 가짜(샘플) 날짜 채우기
--  · UserBibleProgressCompleted 는 UserName 기준이라 [Identity].[User] 로 UserId 를 찾는다.
--  · 마지막 완독 = 최근 30~120일 전, 그 앞 회차는 400~500일씩 거슬러 올라간다.
--  · 이미 줄이 있는 (사람, 회차)는 건드리지 않는다.
--  · 모두 IsSeed = 1 로 표시하므로 나중에 아래 한 줄로 지울 수 있다.
--      DELETE FROM dbo.BibleCompletionLog WHERE IsSeed = 1;
-- ============================================================
SET NOCOUNT ON;

;WITH src AS (
    SELECT  u.Id                                   AS UserId,
            c.UserName,
            MAX(ISNULL(c.readBibleAllCount, 0))    AS DoneCount
    FROM    dbo.UserBibleProgressCompleted c WITH (NOLOCK)
    JOIN    [Identity].[User]              u WITH (NOLOCK) ON u.UserName = c.UserName
    GROUP BY u.Id, c.UserName
    HAVING  MAX(ISNULL(c.readBibleAllCount, 0)) >= 1
),
rounds AS (
    SELECT  s.UserId, s.UserName, s.DoneCount, n.RoundNo
    FROM    src s
    JOIN    (VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10)) AS n(RoundNo)
            ON n.RoundNo <= s.DoneCount
)
INSERT INTO dbo.BibleCompletionLog (UserId, UserName, RoundNo, CompletedAt, IsSeed)
SELECT  r.UserId,
        r.UserName,
        r.RoundNo,
        CAST(DATEADD(DAY,
             - ( (ABS(CHECKSUM(r.UserId)) % 91) + 30 )                                            -- 마지막 완독까지의 간격
             - ( (r.DoneCount - r.RoundNo) * ( 400 + (ABS(CHECKSUM(r.UserId, r.RoundNo)) % 101) ) ), -- 이전 회차는 더 거슬러
             CAST(GETDATE() AS DATE)) AS DATE),
        1
FROM    rounds r
WHERE   NOT EXISTS (
            SELECT 1 FROM dbo.BibleCompletionLog b
            WHERE b.UserId = r.UserId AND b.RoundNo = r.RoundNo
        );

PRINT CONCAT('[BibleCompletionLog] 샘플 ', @@ROWCOUNT, '줄 넣음');

-- 확인용
SELECT  b.UserName, b.RoundNo, b.CompletedAt, b.IsSeed
FROM    dbo.BibleCompletionLog b
ORDER BY b.UserName, b.RoundNo;
GO
