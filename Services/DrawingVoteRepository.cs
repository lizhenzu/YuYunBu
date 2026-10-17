using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BulletinBoard.Helpers;
using Microsoft.Extensions.Configuration;

public class DrawingVoteRepository : IDrawingVoteRepository
{
    private readonly string _connString;

    public DrawingVoteRepository(IConfiguration config)
    {
        _connString = config.GetConnectionString("DefaultConnection");
    }

    // sortBy: "likes" or "number"
    // voterIp: 현재 투표 요청자의 IP
    public List<DrawingVoteViewModel> GetAllWithUserState(string sortBy, string voterIp)
    {
        string orderBy = "v.ImageNumber ASC";

        if (!string.IsNullOrEmpty(sortBy))
        {
            if (sortBy.Equals("likes", StringComparison.OrdinalIgnoreCase))
            {
                orderBy = "v.LikeCount DESC, v.ImageNumber ASC";
            }
            else if (sortBy.Equals("number", StringComparison.OrdinalIgnoreCase))
            {
                orderBy = "v.ImageNumber ASC";
            }
        }

        string sql = $@"
            SELECT
                v.Id,
                v.ImageNumber,
                v.Who,
                v.ImageUrl,
                v.LikeCount,
                CASE WHEN l.Id IS NULL THEN 0 ELSE 1 END AS IsLikedByMe
            FROM dbo.DrawingVote v WITH (NOLOCK)
            LEFT JOIN dbo.DrawingVoteLog l WITH (NOLOCK)
                ON l.DrawingVoteId = v.Id
               AND l.VoterIp = @VoterIp
            ORDER BY {orderBy};
        ";

        var list = new List<DrawingVoteViewModel>();

        using (var conn = new SqlConnection(_connString))
        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.Add(new SqlParameter("@VoterIp", SqlDbType.NVarChar, 64)
            {
                Value = (object)(voterIp ?? "")
            });

            conn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new DrawingVoteViewModel
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        ImageNumber = reader.GetInt32(reader.GetOrdinal("ImageNumber")),
                        ImageUrl = reader.GetString(reader.GetOrdinal("ImageUrl")),
                        Who = reader.GetString(reader.GetOrdinal("Who")),
                        LikeCount = reader.GetInt32(reader.GetOrdinal("LikeCount")),
                        IsLikedByMe = reader.GetInt32(reader.GetOrdinal("IsLikedByMe")) == 1
                    });
                }
            }
        }

        return list;
    }

    public ToggleLikeResult ToggleLike(int id, string voterIp)
    {
        var result = new ToggleLikeResult
        {
            Success = false,
            IsLikedNow = false,
            LikeCount = 0,
            Id = id
        };

        using (var conn = new SqlConnection(_connString))
        using (var cmd = new SqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = @"
DECLARE @AlreadyLiked BIT = 0;

IF EXISTS (
    SELECT 1
    FROM dbo.DrawingVoteLog WITH (NOLOCK)
    WHERE DrawingVoteId = @Id
      AND VoterIp = @VoterIp
)
BEGIN
    SET @AlreadyLiked = 1;
END

IF (@AlreadyLiked = 1)
BEGIN
    -- 이미 좋아요 눌렀으면: 취소 (로그 삭제 / LikeCount -1)
    DELETE FROM dbo.DrawingVoteLog
    WHERE DrawingVoteId = @Id
      AND VoterIp = @VoterIp;

    UPDATE dbo.DrawingVote
    SET LikeCount = CASE WHEN LikeCount > 0 THEN LikeCount - 1 ELSE 0 END,
        UpdatedAt = @UpdatedAt
    WHERE Id = @Id;
END
ELSE
BEGIN
    -- 아직 안 눌렀으면: 좋아요 추가 (로그 insert / LikeCount +1)
    INSERT INTO dbo.DrawingVoteLog (DrawingVoteId, VoterIp)
    VALUES (@Id, @VoterIp);

    UPDATE dbo.DrawingVote
    SET LikeCount = LikeCount + 1,
        UpdatedAt = @UpdatedAt
    WHERE Id = @Id;
END

-- 최종 상태 반환
SELECT
    v.LikeCount,
    CASE WHEN EXISTS(
        SELECT 1
        FROM dbo.DrawingVoteLog
        WHERE DrawingVoteId = v.Id
          AND VoterIp = @VoterIp
    )
    THEN 1 ELSE 0 END AS IsLikedNow
FROM dbo.DrawingVote v
WHERE v.Id = @Id;
";

            cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
            cmd.Parameters.Add(new SqlParameter("@VoterIp", SqlDbType.NVarChar, 64)
            {
                Value = (object)(voterIp ?? "")
            });
            cmd.Parameters.AddWithValue("@UpdatedAt", DateHelper.GetKoreaNow());
            conn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    result.LikeCount = reader.GetInt32(reader.GetOrdinal("LikeCount"));
                    result.IsLikedNow = reader.GetInt32(reader.GetOrdinal("IsLikedNow")) == 1;
                    result.Success = true;
                }
            }
        }

        return result;
    }
}
