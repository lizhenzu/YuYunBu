using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using BulletinBoard.Enums;
using BulletinBoard.Helpers;
using BulletinBoard.Models.Gift;
using BulletinBoard.Services.Abstract;
using Microsoft.Extensions.Configuration;

namespace BulletinBoard.Services.Repos
{
    /// <summary>
    /// Persists gift popup state per user.
    /// Table: dbo.GiftPopupState (UserName PK)
    /// Log : dbo.GiftPopupActionLog (append-only)
    /// </summary>
    public class GiftPopupStateRepo : IGiftPopupStateRepo
    {
        private readonly string _connStr;

        public GiftPopupStateRepo(IConfiguration cfg)
        {
            _connStr = cfg.GetConnectionString("DefaultConnection");
        }
        public async Task UpsertAsync(GiftPopupState state)
        {
            const string sql = @"
MERGE dbo.GiftPopupState AS t
USING (SELECT @UserName AS UserName) AS s
ON t.UserName = s.UserName
WHEN MATCHED THEN
  UPDATE SET
    LastSeenMilestone = @LastSeenMilestone,
    BlockUntilUtc = @BlockUntilUtc,
    UpdatedAtUtc = @UpdatedAtUtc
WHEN NOT MATCHED THEN
  INSERT (UserName, LastSeenMilestone, BlockUntilUtc, UpdatedAtUtc)
  VALUES (@UserName, @LastSeenMilestone, @BlockUntilUtc, @UpdatedAtUtc);
";

            using var con = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@UserName", state.UserName);
            cmd.Parameters.AddWithValue("@LastSeenMilestone", state.LastSeenMilestone);
            cmd.Parameters.Add("@BlockUntilUtc", SqlDbType.DateTime2).Value =
                (object?)state.BlockUntilUtc ?? DBNull.Value;
            cmd.Parameters.Add("@BlockUntilUtc", SqlDbType.DateTime2).Value =
                (object?)DateHelper.GetKoreaNow() ?? DBNull.Value;

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<GiftPopupState?> GetAsync(string UserName)
        {
            const string sql = @"
SELECT UserName, LastSeenMilestone, BlockUntilUtc, UpdatedAtUtc
FROM dbo.GiftPopupState
WHERE UserName=@UserName;
";
            using var con = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@UserName", UserName);

            await con.OpenAsync();
             await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
            using var r = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);

            if (!await r.ReadAsync()) return null;

            return new GiftPopupState
            {
                UserName = r.GetString(0),
                LastSeenMilestone = r.GetInt32(1),
                BlockUntilUtc = r.IsDBNull(2) ? (DateTime?)null : r.GetDateTime(2),
                UpdatedAtUtc = r.GetDateTime(3)
            };
        }

        public async Task ApplyActionAsync(string UserName, int milestone, PopupActionType actionType)
        {
            // 정책:
            // - ACCUMULATE/LATER/GO_PICK : 해당 milestone을 "봤다"로 처리(같은 milestone 재팝업 방지)
            // - DEFER3/DEFER7           : 일정 기간 팝업 전체 차단 (BlockUntilUtc)
            DateTime? blockUntilUtc = null;
            var nowUtc = DateHelper.GetKoreaNow();

            if(actionType == PopupActionType.LATER)
            {
                return;
            }

            switch (actionType)
            {
                case PopupActionType.DEFER3:
                    blockUntilUtc = nowUtc.AddDays(3);
                    break;
                case PopupActionType.DEFER7:
                    blockUntilUtc = nowUtc.AddDays(7);
                    break;
            }

            const string sql = @"
MERGE dbo.GiftPopupState AS t
USING (SELECT @UserName AS UserName) AS s
ON t.UserName = s.UserName
WHEN MATCHED THEN
  UPDATE SET
    LastSeenMilestone = CASE WHEN @Milestone > t.LastSeenMilestone THEN @Milestone ELSE t.LastSeenMilestone END,
    BlockUntilUtc = CASE 
                      WHEN @BlockUntilUtc IS NULL THEN t.BlockUntilUtc
                      WHEN t.BlockUntilUtc IS NULL THEN @BlockUntilUtc
                      WHEN @BlockUntilUtc > t.BlockUntilUtc THEN @BlockUntilUtc
                      ELSE t.BlockUntilUtc
                    END,
    UpdatedAtUtc = @UpdatedAtUtc
WHEN NOT MATCHED THEN
  INSERT (UserName, LastSeenMilestone, BlockUntilUtc, UpdatedAtUtc)
  VALUES (@UserName, @Milestone, @BlockUntilUtc,  @UpdatedAtUtc);
";

            using var con = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@UserName", UserName);
            cmd.Parameters.AddWithValue("@Milestone", milestone);
            cmd.Parameters.Add("@BlockUntilUtc", SqlDbType.DateTime2).Value =
                (object?)blockUntilUtc ?? DBNull.Value;
            cmd.Parameters.Add("@UpdatedAtUtc", SqlDbType.DateTime2).Value =
                (object?)nowUtc ?? DBNull.Value;

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task LogActionAsync(string UserName, int milestone, PopupActionType actionType)
        {
            const string sql = @"
INSERT INTO dbo.GiftPopupActionLog(UserName, Milestone, ActionType, CreatedAtUtc)
VALUES (@UserName, @Milestone, @ActionType, @CreatedAtUtc);
";
            using var con = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@UserName", UserName);
            cmd.Parameters.AddWithValue("@Milestone", milestone);
            cmd.Parameters.AddWithValue("@ActionType", actionType.ToString());
            cmd.Parameters.AddWithValue("@CreatedAtUtc", DateHelper.GetKoreaNow());
            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
