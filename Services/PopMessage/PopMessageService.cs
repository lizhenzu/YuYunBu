using BulletinBoard.Common;
using BulletinBoard.Helpers;
using BulletinBoard.Services.Abstract;
using BulletinBoard.ViewModels.PopMessage;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;

namespace BulletinBoard.Services
{
    public class PopMessageService : IPopMessageService
    {
        private readonly IConfiguration _config;

        public PopMessageService(IConfiguration config)
        {
            _config = config;
        }

        private SqlConnection OpenConn()
            => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        // ✅ UserId(로그인) -> view_PersonMaster_2026 -> UserName
        public string? GetUserNameByUserId(string userId)
        {
            userId = (userId ?? "").Trim();
            if (string.IsNullOrWhiteSpace(userId)) return null;

            using var conn = OpenConn();
            conn.Open();

            var sql = @"
SELECT TOP 1 UserName
FROM dbo.view_PersonMaster_2026
WHERE UserId = @UserId AND UserId IS NOT NULL AND UserName IS NOT NULL
";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 256) { Value = userId });

            var val = cmd.ExecuteScalar();
            return val == null ? null : Convert.ToString(val);
        }

        // ✅ 수신 가능 대상: UserId IS NOT NULL AND UserName IS NOT NULL
        public bool CanSendTo(string toUserName)
        {
            toUserName = (toUserName ?? "").Trim();
            if (string.IsNullOrWhiteSpace(toUserName)) return false;

            using var conn = OpenConn();
            conn.Open();

            var sql = @"
SELECT TOP 1 1
FROM dbo.view_PersonMaster_2026
WHERE UserName = @ToUserName
  AND UserName IS NOT NULL
  AND UserId IS NOT NULL
";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@ToUserName", SqlDbType.NVarChar, 256) { Value = toUserName });

            return cmd.ExecuteScalar() != null;
        }

        public PopMessageComposeVM GetComposeVM(string toUserName, string? returnUrl)
        {
            toUserName = (toUserName ?? "").Trim();

            using var conn = OpenConn();
            conn.Open();

            var sql = @"
SELECT TOP 1
    ISNULL(fullName,'') AS FullName,
    UserName
FROM dbo.view_PersonMaster_2026
WHERE UserName = @ToUserName
  AND UserName IS NOT NULL
  AND UserId IS NOT NULL
";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@ToUserName", SqlDbType.NVarChar, 256) { Value = toUserName });

            using var rd = cmd.ExecuteReader();
            if (!rd.Read())
            {
                return new PopMessageComposeVM { ToUserName = toUserName, ReturnUrl = returnUrl };
            }

            return new PopMessageComposeVM
            {
                ToUserName = toUserName,
                ToFullName = Convert.ToString(rd["FullName"]),
                ReturnUrl = returnUrl,
                MaxLen = 500
            };
        }

        public void Send(string toUserName, string fromUserName, string messageText)
        {
            toUserName = (toUserName ?? "").Trim();
            fromUserName = (fromUserName ?? "").Trim();
            messageText = (messageText ?? "").Trim();

            if (string.IsNullOrWhiteSpace(toUserName)) throw new ArgumentException("toUserName is empty");
            if (string.IsNullOrWhiteSpace(fromUserName)) throw new ArgumentException("fromUserName is empty");
            if (string.IsNullOrWhiteSpace(messageText)) throw new ArgumentException("messageText is empty");

            if (!CanSendTo(toUserName))
                throw new InvalidOperationException("수신자(UserName)가 없거나 UserId가 NULL이라 메시지를 보낼 수 없습니다.");

            using var conn = OpenConn();
            conn.Open();

            // ✅ Visited/readYN/UpdateDT/InsertDT는 DEFAULT 사용 가능
            var sql = @"
INSERT INTO dbo.PopMessage (MessageText, ToWhom, FromWhom)
VALUES (@MessageText, @ToWhom, @FromWhom);
";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@MessageText", SqlDbType.NVarChar) { Value = messageText });
            cmd.Parameters.Add(new SqlParameter("@ToWhom", SqlDbType.NVarChar, 256) { Value = toUserName });
            cmd.Parameters.Add(new SqlParameter("@FromWhom", SqlDbType.NVarChar, 256) { Value = fromUserName });
            cmd.ExecuteNonQuery();
        }

        public int GetInboxUnreadCount(string myUserName)
        {
            using var conn = OpenConn();
            conn.Open();

            var sql = @"
SELECT COUNT(1)
FROM dbo.PopMessage
WHERE ToWhom = @Me
  AND ISNULL(Visited,0) = 0
";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@Me", SqlDbType.NVarChar, 256) { Value = myUserName });

            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
        }

        // ✅ [추가] 받은함 전체 건수(Visited 상관없이 전체)
        public int GetInboxTotalCount(string myUserName)
        {
            myUserName = (myUserName ?? "").Trim();
            if (string.IsNullOrWhiteSpace(myUserName)) return 0;

            using var conn = OpenConn();
            conn.Open();

            var sql = @"
SELECT COUNT(1)
FROM dbo.PopMessage
WHERE ToWhom = @Me
";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@Me", SqlDbType.NVarChar, 256) { Value = myUserName });

            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
        }

        // ✅ [추가] 보낸함 전체 건수
        public int GetSentTotalCount(string myUserName)
        {
            myUserName = (myUserName ?? "").Trim();
            if (string.IsNullOrWhiteSpace(myUserName)) return 0;

            using var conn = OpenConn();
            conn.Open();

            var sql = @"
SELECT COUNT(1)
FROM dbo.PopMessage
WHERE FromWhom = @Me
";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@Me", SqlDbType.NVarChar, 256) { Value = myUserName });

            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
        }

        private static string ToExecutableSql(string sql, SqlCommand cmd)
        {
            var result = sql;

            foreach (SqlParameter p in cmd.Parameters)
            {
                string value;

                if (p.Value == null || p.Value == DBNull.Value)
                {
                    value = "NULL";
                }
                else if (p.DbType == DbType.String || p.DbType == DbType.AnsiString)
                {
                    value = $"N'{p.Value.ToString().Replace("'", "''")}'";
                }
                else if (p.DbType == DbType.DateTime || p.DbType == DbType.DateTime2)
                {
                    value = $"'{Convert.ToDateTime(p.Value):yyyy-MM-dd HH:mm:ss.fff}'";
                }
                else
                {
                    value = p.Value.ToString()!;
                }

                result = result.Replace(p.ParameterName, value);
            }

            return result;
        }

        // ✅ 받은함
        public List<PopMessageListItemVM> GetInbox(string myUserName, int page, int pageSize)
        {
            using var conn = OpenConn();
            conn.Open();

            var sql = @"
;WITH X AS (
    SELECT
        p.Id,
        p.MessageText,
        p.FromWhom,
        p.ToWhom,
        ISNULL(p.Visited,0) AS Visited,
        p.InsertDT,
        v.fullName AS PeerFullName,
        v.UserName  AS PeerUserName
    FROM dbo.PopMessage p
    LEFT JOIN dbo.view_PersonMaster_2026 v
      ON v.UserName = p.FromWhom
     AND v.UserName IS NOT NULL
     AND v.UserId IS NOT NULL
    WHERE p.ToWhom = @Me
)
SELECT *
FROM X
ORDER BY CASE WHEN Visited=0 THEN 0 ELSE 1 END, InsertDT DESC, Id DESC
OFFSET @Offset ROWS FETCH NEXT @Take ROWS ONLY;
";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@Me", SqlDbType.NVarChar, 256) { Value = myUserName });
            cmd.Parameters.AddWithValue("@Offset", Math.Max(0, page) * Math.Max(1, pageSize));
            cmd.Parameters.AddWithValue("@Take", Math.Max(1, pageSize));

            // ✅ 실행 전 로그
            var executableSql = ToExecutableSql(sql, cmd);
            DbLogger.LogDebugAsync($@"
[PopMessage.GetInbox]
UserName: {myUserName}

Executable SQL:
{executableSql}
");

            var list = new List<PopMessageListItemVM>();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                var text = Convert.ToString(rd["MessageText"]) ?? "";
                var peerFull = Convert.ToString(rd["PeerFullName"]) ?? "";
                var peerUser = Convert.ToString(rd["PeerUserName"]) ?? "";
                var fromWhom = Convert.ToString(rd["FromWhom"]) ?? "";
                list.Add(new PopMessageListItemVM
                {
                    Id = Convert.ToInt32(rd["Id"]),
                    MessageText = text,
                    MessagePreview = MakePreview(text),
                    FromWhom = fromWhom,
                    ToWhom = Convert.ToString(rd["ToWhom"]),
                    Visited = Convert.ToInt32(rd["Visited"]),
                    PeerFullName = !string.IsNullOrEmpty(peerFull) ? peerFull : fromWhom,
                    PeerUserName = !string.IsNullOrEmpty(peerUser) ? peerUser : fromWhom,
                    InsertDT = FormatKst(Convert.ToDateTime(rd["InsertDT"]))
                });
            }

            return list;
        }

        // ✅ 보낸함
        public List<PopMessageListItemVM> GetSent(string myUserName, int page, int pageSize)
        {
            using var conn = OpenConn();
            conn.Open();

            var sql = @"
;WITH X AS (
    SELECT
        p.Id,
        p.MessageText,
        p.FromWhom,
        p.ToWhom,
        ISNULL(p.Visited,0) AS Visited,
        p.InsertDT,
        v.fullName AS PeerFullName,
        v.UserName  AS PeerUserName
    FROM dbo.PopMessage p
    LEFT JOIN dbo.view_PersonMaster_2026 v
      ON v.UserName = p.ToWhom
     AND v.UserName IS NOT NULL
     AND v.UserId IS NOT NULL
    WHERE p.FromWhom = @Me
)
SELECT *
FROM X
ORDER BY InsertDT DESC, Id DESC
OFFSET @Offset ROWS FETCH NEXT @Take ROWS ONLY;
";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@Me", SqlDbType.NVarChar, 256) { Value = myUserName });
            cmd.Parameters.AddWithValue("@Offset", Math.Max(0, page) * Math.Max(1, pageSize));
            cmd.Parameters.AddWithValue("@Take", Math.Max(1, pageSize));

            // ✅ 실행 전 로그
            var executableSql = ToExecutableSql(sql, cmd);
            DbLogger.LogDebugAsync($@"
[PopMessage.GetSent]
UserName: {myUserName}

Executable SQL:
{executableSql}
");

            var list = new List<PopMessageListItemVM>();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                var text = Convert.ToString(rd["MessageText"]) ?? "";
                var peerFull2 = Convert.ToString(rd["PeerFullName"]) ?? "";
                var peerUser2 = Convert.ToString(rd["PeerUserName"]) ?? "";
                var toWhom2 = Convert.ToString(rd["ToWhom"]) ?? "";
                list.Add(new PopMessageListItemVM
                {
                    Id = Convert.ToInt32(rd["Id"]),
                    MessageText = text,
                    MessagePreview = MakePreview(text),
                    FromWhom = Convert.ToString(rd["FromWhom"]),
                    ToWhom = toWhom2,
                    Visited = Convert.ToInt32(rd["Visited"]),
                    PeerFullName = !string.IsNullOrEmpty(peerFull2) ? peerFull2 : toWhom2,
                    PeerUserName = !string.IsNullOrEmpty(peerUser2) ? peerUser2 : toWhom2,
                    InsertDT = FormatKst(Convert.ToDateTime(rd["InsertDT"]))
                });
            }
            return list;
        }

        public int MarkReadBulk(string myUserName, int[] ids)
        {
            if (ids == null || ids.Length == 0) return 0;

            using var conn = OpenConn();
            conn.Open();

            // ✅ 받은함에서만 읽음처리 (ToWhom=나)
            // ids를 IN으로 넣기 위해 파라미터 배열 구성
            var sb = new System.Text.StringBuilder();
            sb.Append("UPDATE dbo.PopMessage SET Visited=1, UpdateDT=GETDATE() WHERE ToWhom=@Me AND Id IN (");
            for (int i = 0; i < ids.Length; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append("@Id" + i);
            }
            sb.Append(");");

            using var cmd = new SqlCommand(sb.ToString(), conn);
            cmd.Parameters.Add(new SqlParameter("@Me", SqlDbType.NVarChar, 256) { Value = myUserName });
            for (int i = 0; i < ids.Length; i++)
                cmd.Parameters.AddWithValue("@Id" + i, ids[i]);

            return cmd.ExecuteNonQuery();
        }

        public int DeleteMessage(string myUserName, int id)
        {
            using var conn = OpenConn();
            conn.Open();

            // ✅ 내가 받은/보낸 것만 삭제 가능
            var sql = @"
DELETE FROM dbo.PopMessage
WHERE Id = @Id
  AND (ToWhom = @Me OR FromWhom = @Me);
";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.Add(new SqlParameter("@Me", SqlDbType.NVarChar, 256) { Value = myUserName });

            return cmd.ExecuteNonQuery();
        }
        public async Task AdminBroadcastAllUsersAsync(string messageText, string fromWhom)
        {
            messageText = (messageText ?? "").Trim();
            fromWhom = (fromWhom ?? "").Trim();

            if (string.IsNullOrWhiteSpace(messageText))
                throw new ArgumentException("messageText is empty");
            if (string.IsNullOrWhiteSpace(fromWhom))
                throw new ArgumentException("fromWhom is empty");

            using var conn = OpenConn();
            using var cmd = new SqlCommand("dbo.InsertPopMessagesForAllUsers", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            // ✅ 유니코드 NVARCHAR(MAX)
            cmd.Parameters.Add("@message", SqlDbType.NVarChar, -1).Value = messageText;

            // ✅ 유니코드 NVARCHAR(256)
            cmd.Parameters.Add("@fromWhom", SqlDbType.NVarChar, 256).Value = fromWhom;

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
        public async Task<int> DeleteAllByFromWhomAsync(string fromWhom)
        {
            if (string.IsNullOrWhiteSpace(fromWhom))
                throw new ArgumentException("fromWhom is empty");

            using var conn = OpenConn();
            await conn.OpenAsync();

            var sql = @"DELETE FROM dbo.PopMessage WHERE FromWhom = @FromWhom;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@FromWhom", fromWhom.Trim());

            var affected = await cmd.ExecuteNonQueryAsync();
            return affected;
        }

        public List<PopMessagePersonVM> SearchPeople(string q, int take = 20)
        {
            q = (q ?? "").Trim();

            using var conn = OpenConn();
            conn.Open();

            var sql = @"
SELECT TOP (@Take)
    ISNULL(UserName,'') AS UserName,
    ISNULL(fullName,'') AS FullName
FROM dbo.view_PersonMaster_2026
WHERE UserId IS NOT NULL
  AND UserName IS NOT NULL
  AND (
        fullName LIKE '%' + @Q + '%'
     OR UserName LIKE '%' + @Q + '%'
     OR PhoneNumber LIKE '%' + @Q + '%'
     OR PhoneNumber2 LIKE '%' + @Q + '%'
  )
ORDER BY fullName ASC;
";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Take", Math.Max(1, take));
            cmd.Parameters.AddWithValue("@Q", q);

            var list = new List<PopMessagePersonVM>();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new PopMessagePersonVM
                {
                    UserName = Convert.ToString(rd["UserName"]) ?? "",
                    FullName = Convert.ToString(rd["FullName"]) ?? ""
                });
            }
            return list;
        }

        private static string MakePreview(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            var t = text.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
            t = t.Replace("\n", " ");
            if (t.Length <= 60) return t;
            return t.Substring(0, 60) + "…";
        }

        private static string FormatKst(DateTime dt)
        {
            // DB 시간이 KST라고 가정(기존 프로젝트 패턴 유지)
            return dt.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
