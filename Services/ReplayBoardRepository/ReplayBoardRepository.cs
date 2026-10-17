using BulletinBoard.Helpers;
using BulletinBoard.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace BulletinBoard.Services.ReplayBoard;

/// <summary>
/// ✅ ReplayBoardRepository (확장판)
/// - .NET 9.0 / SQL Server
/// - 방식 A: 게시판마다 물리 테이블
/// - 보안: BoardRegistry로 허용된 PostTable/ReplyTable만 사용
/// - 추가 기능:
///   - 로그인 작성자(UserId) 메타 저장(중앙 메타 테이블)
///   - 공지 상단 고정(IsNotice)
///   - 추천(좋아요) + 내 좋아요 여부
///   - 제목/내용 검색
/// </summary>
public sealed class ReplayBoardRepository
{
    private readonly string _cs;

    // 중앙 메타/좋아요 테이블(게시판별 테이블을 ALTER하지 않기 위해 중앙 테이블로 관리)
    private const string MetaTable = "ReplayBoardPostMeta";
    private const string LikeTable = "ReplayBoardPostLike";

    public ReplayBoardRepository(IConfiguration config)
    {
        _cs = config.GetConnectionString("DefaultConnection")!;
        if (string.IsNullOrWhiteSpace(_cs))
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection 가 비어있습니다.");
    }

    // ✅ 허용된 게시판(테이블)인지 검증 + 컨텍스트 반환
    // ✅ 변경: boardNameParam 제거 / PostTable만으로 BoardRegistry에서 BoardName을 조회
    public async Task<BoardContext?> ResolveBoardAsync(string postTableParam)
    {
        if (string.IsNullOrWhiteSpace(postTableParam))
            return null;

        await using var con = new SqlConnection(_cs);
        await con.OpenAsync();

        const string sql = @"
SELECT TOP 1 BoardName, PostTable, ReplyTable
FROM dbo.BoardRegistry
WHERE IsEnabled = 1
  AND PostTable = @PostTable
";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add(new SqlParameter("@PostTable", SqlDbType.NVarChar, 128) { Value = postTableParam });

        await using var r = await cmd.ExecuteReaderAsync();
        if (!await r.ReadAsync()) return null;

        var boardName = r.GetString(0);
        var postTable = r.GetString(1);
        var replyTable = r.GetString(2);

        return new BoardContext
        {
            BoardName = boardName,
            PostTable = postTable,
            ReplyTable = replyTable,
            FileTable = postTable + "_File"
        };
    }

    // ✅ SQL Server 식별자 안전 처리: [Name] + ']' escape
    private static string Q(string identifier) => $"[{identifier.Replace("]", "]]")}]";

    // =========================================================
    // Posts
    // =========================================================

    public async Task<(List<BoardPost> items, int total)> ListAsync(
        BoardContext board,
        int page,
        int pageSize,
        string? q = null,
        string? viewerUserId = null)
    {
        var isNoticeBoard = board.PostTable.Equals("NoticeBoard", StringComparison.OrdinalIgnoreCase);

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 15;

        var t = Q(board.PostTable);
        var mt = Q(MetaTable);
        var lt = Q(LikeTable);

        var offset = (page - 1) * pageSize;
        var keyword = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

        // 검색: 제목/내용 LIKE
        var where = "1=1";
        if (keyword != null)
        {
            where += " AND (p.Title LIKE @Q OR p.Content LIKE @Q)";
        }

        // 내가 좋아요 했는지: EXISTS 서브쿼리
        var likedExpr = viewerUserId == null
            ? "CAST(0 AS bit) AS IsLikedByMe"
            : $@"CASE WHEN EXISTS(
                    SELECT 1 FROM {lt} l
                    WHERE l.PostTable = @PostTable AND l.PostId = p.Id AND l.UserId = @ViewerUserId
                ) THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS IsLikedByMe";

        // ✅ NoticeBoard는 물리 테이블에 IsPinned/PopupUntil 컬럼이 있으므로, 선택적으로 포함
        var pinnedCols = isNoticeBoard
            ? ",\n    p.IsPinned,\n    p.PopupUntil"
            : ",\n    CAST(0 AS bit) AS IsPinned,\n    CAST(NULL AS datetime2(0)) AS PopupUntil";

        var orderBy = isNoticeBoard
            ? "ORDER BY p.IsPinned DESC, ISNULL(m.IsNotice,0) DESC, p.Id DESC"
            : "ORDER BY ISNULL(m.IsNotice,0) DESC, p.Id DESC";

        var sql = $@"
SELECT COUNT(1)
FROM {t} p
LEFT JOIN {mt} m ON m.PostTable=@PostTable AND m.PostId=p.Id
WHERE {where};

SELECT
    p.Id, p.Title, p.Content, p.Writer, p.ViewCount, p.CreatedAt, p.UpdatedAt,
    m.AuthorUserId,
    ISNULL(m.IsNotice, 0) AS IsNotice,
    ISNULL(m.LikeCount, 0) AS LikeCount,
    {likedExpr}
    {pinnedCols}
FROM {t} p
LEFT JOIN {mt} m ON m.PostTable=@PostTable AND m.PostId=p.Id
WHERE {where}
{orderBy}
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
";

        var items = new List<BoardPost>();
        int total = 0;

        await using var con = new SqlConnection(_cs);
        await con.OpenAsync();
        await using var cmd = new SqlCommand(sql, con);

        cmd.Parameters.Add(new SqlParameter("@PostTable", SqlDbType.NVarChar, 128) { Value = board.PostTable });
        cmd.Parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = offset });
        cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

        if (keyword != null)
            cmd.Parameters.Add(new SqlParameter("@Q", SqlDbType.NVarChar, 400) { Value = "%" + keyword + "%" });

        if (viewerUserId != null)
            cmd.Parameters.Add(new SqlParameter("@ViewerUserId", SqlDbType.NVarChar, 450) { Value = viewerUserId });

        await using var r = await cmd.ExecuteReaderAsync();

        if (await r.ReadAsync())
            total = r.GetInt32(0);

        await r.NextResultAsync();
        while (await r.ReadAsync())
        {
            items.Add(new BoardPost
            {
                Id = r.GetInt32(0),
                Title = r.GetString(1),
                Content = r.GetString(2),
                Writer = r.GetString(3),
                ViewCount = r.GetInt32(4),
                CreatedAt = r.GetDateTime(5),
                UpdatedAt = r.IsDBNull(6) ? null : r.GetDateTime(6),

                AuthorUserId = r.IsDBNull(7) ? null : r.GetString(7),
                IsNotice = !r.IsDBNull(8) && r.GetBoolean(8),
                LikeCount = r.IsDBNull(9) ? 0 : Convert.ToInt32(r.GetValue(9)),
                IsLikedByMe = r.GetBoolean(10),

                // NoticeBoard 전용 (다른 게시판은 기본값)
                IsPinned = r.GetBoolean(11),
                PopupUntil = r.IsDBNull(12) ? null : r.GetDateTime(12)
            });
        }

        return (items, total);
    }

    public async Task<BoardPost?> GetAsync(BoardContext board, int id, bool increaseView, string? viewerUserId = null)
    {
        var isNoticeBoard = board.PostTable.Equals("NoticeBoard", StringComparison.OrdinalIgnoreCase);

        var t = Q(board.PostTable);
        var mt = Q(MetaTable);
        var lt = Q(LikeTable);

        // 내가 좋아요 했는지: EXISTS
        var likedExpr = viewerUserId == null
            ? "CAST(0 AS bit) AS IsLikedByMe"
            : $@"CASE WHEN EXISTS(
                SELECT 1 FROM {lt} l
                WHERE l.PostTable = @PostTable AND l.PostId = p.Id AND l.UserId = @ViewerUserId
            ) THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS IsLikedByMe";

        // ✅ bit/int 혼재 방지: IsNotice/LikeCount는 int로 강제 캐스팅 (C# GetInt32 안전)
        var pinnedCols = isNoticeBoard
            ? ",\n    p.IsPinned,\n    p.PopupUntil"
            : ",\n    CAST(0 AS bit) AS IsPinned,\n    CAST(NULL AS datetime2(0)) AS PopupUntil";

        var selectSql = $@"
SELECT
    p.Id, p.Title, p.Content, p.Writer, p.ViewCount, p.CreatedAt, p.UpdatedAt,
    m.AuthorUserId,
    CAST(ISNULL(m.IsNotice, 0) AS int) AS IsNotice,
    CAST(ISNULL(m.LikeCount, 0) AS int) AS LikeCount,
    {likedExpr}
    {pinnedCols}
FROM {t} p
LEFT JOIN {mt} m ON m.PostTable=@PostTable AND m.PostId=p.Id
WHERE p.Id=@Id;
";

        await using var con = new SqlConnection(_cs);
        await con.OpenAsync();

        // ✅ 1) 조회수 증가 쿼리 분리 실행 (ExecuteNonQuery)
        if (increaseView)
        {
            var updateSql = $@"UPDATE {t} SET ViewCount = ViewCount + 1 WHERE Id=@Id;";
            await using var updateCmd = new SqlCommand(updateSql, con);
            updateCmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
            await updateCmd.ExecuteNonQueryAsync();
        }

        // ✅ 2) SELECT는 별도 실행 (ExecuteReader)
        await using var cmd = new SqlCommand(selectSql, con);
        cmd.Parameters.Add(new SqlParameter("@PostTable", SqlDbType.NVarChar, 128) { Value = board.PostTable });
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });

        if (!string.IsNullOrWhiteSpace(viewerUserId))
            cmd.Parameters.Add(new SqlParameter("@ViewerUserId", SqlDbType.NVarChar, 450) { Value = viewerUserId });

        await using var r = await cmd.ExecuteReaderAsync();

        if (!await r.ReadAsync())
            return null;

        return new BoardPost
        {
            Id = r.GetInt32(0),
            Title = r.GetString(1),
            Content = r.GetString(2),
            Writer = r.GetString(3),
            ViewCount = r.GetInt32(4),
            CreatedAt = r.GetDateTime(5),
            UpdatedAt = r.IsDBNull(6) ? null : r.GetDateTime(6),

            AuthorUserId = r.IsDBNull(7) ? null : r.GetString(7),
            IsNotice = r.GetInt32(8) == 1,
            LikeCount = r.GetInt32(9),
            IsLikedByMe = r.GetBoolean(10),

            // NoticeBoard 전용 (다른 게시판은 기본값)
            IsPinned = r.GetBoolean(11),
            PopupUntil = r.IsDBNull(12) ? null : r.GetDateTime(12)
        };
    }

    /// <summary>
    /// ✅ 글 작성 + 메타 저장(작성자 UserId/공지여부)
    /// </summary>
 




 
public async Task<int> CreateAsync(
    BoardContext board,
    string title,
    string content,
    string writerSnapshot,
    string? authorUserId,
    bool isNotice,
    bool isPinned = false,
    string? popupUntilRaw = null)
{
    if (board == null) throw new ArgumentNullException(nameof(board));
    title ??= "";
    content ??= "";
    writerSnapshot ??= "";

    var isNoticeBoard = board.PostTable.Equals("NoticeBoard", StringComparison.OrdinalIgnoreCase);

    var t = Q(board.PostTable);
    var mt = Q(MetaTable);

    // ✅ NoticeBoard 전용: IsPinned 체크가 아니면 PopupUntil은 무조건 NULL
    if (!isPinned) popupUntilRaw = null;

    // ✅ 화면 입력값을 ISO 8601(126)로 정규화(타임존 변환 없음)
    popupUntilRaw = isNoticeBoard && isPinned
        ? NormalizePopupUntilIso126(popupUntilRaw)
        : null;

    var insertCols = isNoticeBoard
        ? "Title, Content, Writer, IsPinned, PopupUntil"
        : "Title, Content, Writer";

    // ✅ TRY_CONVERT로 변환 실패 시 예외 방지(실패하면 NULL)
    var insertVals = isNoticeBoard
        ? @"@Title, @Content, @Writer, @IsPinned,
            CASE
              WHEN @PopupUntil IS NULL OR LTRIM(RTRIM(@PopupUntil))='' THEN NULL
              ELSE TRY_CONVERT(datetime2(0), @PopupUntil, 126)
            END"
        : "@Title, @Content, @Writer";

    var sql = $@"
BEGIN TRAN;

INSERT INTO {t}({insertCols})
VALUES({insertVals});
DECLARE @NewId INT = CAST(SCOPE_IDENTITY() AS INT);

MERGE {mt} AS target
USING (SELECT @PostTable AS PostTable, @NewId AS PostId) AS src
ON target.PostTable = src.PostTable AND target.PostId = src.PostId
WHEN MATCHED THEN
    UPDATE SET AuthorUserId=@AuthorUserId, WriterSnapshot=@WriterSnapshot, IsNotice=@IsNotice
WHEN NOT MATCHED THEN
    INSERT (PostTable, PostId, AuthorUserId, WriterSnapshot, IsNotice, LikeCount, CreatedAt)
    VALUES (@PostTable, @NewId, @AuthorUserId, @WriterSnapshot, @IsNotice, 0, SYSDATETIME());

COMMIT TRAN;

SELECT @NewId;
";

    await using var con = new SqlConnection(_cs);
    await con.OpenAsync();
    await using var cmd = new SqlCommand(sql, con);

    cmd.Parameters.Add(new SqlParameter("@PostTable", SqlDbType.NVarChar, 128) { Value = board.PostTable });
    cmd.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = title });
    cmd.Parameters.Add(new SqlParameter("@Content", SqlDbType.NVarChar) { Value = content });
    cmd.Parameters.Add(new SqlParameter("@Writer", SqlDbType.NVarChar, 60) { Value = writerSnapshot });

    if (isNoticeBoard)
    {
        cmd.Parameters.Add(new SqlParameter("@IsPinned", SqlDbType.Bit) { Value = isPinned });
        cmd.Parameters.Add(new SqlParameter("@PopupUntil", SqlDbType.NVarChar, 40)
        {
            Value = (object?)popupUntilRaw ?? DBNull.Value
        });
    }

    cmd.Parameters.Add(new SqlParameter("@AuthorUserId", SqlDbType.NVarChar, 450) { Value = (object?)authorUserId ?? DBNull.Value });
    cmd.Parameters.Add(new SqlParameter("@WriterSnapshot", SqlDbType.NVarChar, 60) { Value = writerSnapshot });
    cmd.Parameters.Add(new SqlParameter("@IsNotice", SqlDbType.Bit) { Value = isNotice });
        await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
        return (int)(await cmd.ExecuteScalarAsync() ?? 0);
}

/// <summary>
/// ✅ 화면(datetime-local) 입력을 DB 변환용 ISO(126) 문자열로 정규화
/// - 허용: yyyy-MM-ddTHH:mm / yyyy-MM-ddTHH:mm:ss
/// - 방어: 끝의 Z 제거, +09:00/-05:00 같은 offset 제거
/// - 절대 타임존 변환하지 않음(그대로 저장 목적)
/// </summary>
private static string? NormalizePopupUntilIso126(string? raw)
{
    if (string.IsNullOrWhiteSpace(raw)) return null;

    raw = raw.Trim();

    // 1) Z(UTC 표시) 제거
    if (raw.EndsWith("Z", StringComparison.OrdinalIgnoreCase))
        raw = raw[..^1];

    // 2) +09:00 / -05:00 같은 offset이 붙어있으면 제거
    //    예: 2026-02-25T21:30:00+09:00 -> 2026-02-25T21:30:00
    if (raw.Length >= 6 && (raw[^6] == '+' || raw[^6] == '-') && raw[^3] == ':')
        raw = raw[..^6];

    // 3) milliseconds 제거(있으면)
    //    예: 2026-02-25T21:30:00.000 -> 2026-02-25T21:30:00
    var dotIdx = raw.IndexOf('.');
    if (dotIdx > 0)
        raw = raw.Substring(0, dotIdx);    // ✅ NEW: date-only (yyyy-MM-dd) 입력은 해당 "일자 끝(23:59:59)"으로 저장
    if (DateTime.TryParseExact(raw, "yyyy-MM-dd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var d0))
    {
        var end = new DateTime(d0.Year, d0.Month, d0.Day, 23, 59, 59);
        return end.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
    }



    // 4) datetime-local 기본형 (초 없음)
    if (DateTime.TryParseExact(raw, "yyyy-MM-ddTHH:mm",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt1))
    {
        return dt1.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
    }

    // 5) 초 포함형
    if (DateTime.TryParseExact(raw, "yyyy-MM-ddTHH:mm:ss",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt2))
    {
        return dt2.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
    }

    // 6) 예외 케이스: 브라우저/JS가 다른 ISO 변형을 보낼 수 있으니 TryParse로 마지막 방어
    if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt3))
    {
        // 초가 0이면 분까지만, 아니면 초까지(둘 다 style 126 변환 가능)
        return dt3.Second == 0
            ? dt3.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture)
            : dt3.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
    }

    return null;
}






/// <summary>
/// 기존 시그니처 호환용(레거시 호출 방지)
/// </summary>
    public Task<int> CreateAsync(BoardContext board, string title, string content, string writer)
        => CreateAsync(board, title, content, writer, null, false, false, null);

    public async Task<bool> UpdateAsync(
        BoardContext board,
        int id,
        string title,
        string content,
        bool? isPinned = null,
        string? popupUntilRaw = null)
    {
        var isNoticeBoard = board.PostTable.Equals("NoticeBoard", StringComparison.OrdinalIgnoreCase);

        var t = Q(board.PostTable);

        // NoticeBoard 전용: isPinned이 false(또는 null로 들어와도 false로 해석)면 PopupUntil NULL
        if (isNoticeBoard)
        {
            var pinned = isPinned ?? false;
            if (!pinned) popupUntilRaw = null;
        }


        // ✅ date 입력(yyyy-MM-dd)도 허용 + Create와 동일하게 정규화(ISO 126)
        if (isNoticeBoard && (isPinned ?? false))
            popupUntilRaw = NormalizePopupUntilIso126(popupUntilRaw);
        var pinnedSet = isNoticeBoard
            ? ", IsPinned=@IsPinned, PopupUntil=CASE WHEN @PopupUntil IS NULL OR LTRIM(RTRIM(@PopupUntil))='' THEN NULL ELSE CONVERT(datetime2(0), @PopupUntil, 126) END"
            : "";

        var sql = $@"
UPDATE {t}
SET Title=@Title, Content=@Content, UpdatedAt=SYSDATETIME(){pinnedSet}
WHERE Id=@Id;
SELECT @@ROWCOUNT;
";

        await using var con = new SqlConnection(_cs);
        await con.OpenAsync();
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
        cmd.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = title });
        cmd.Parameters.Add(new SqlParameter("@Content", SqlDbType.NVarChar) { Value = content });

        if (isNoticeBoard)
        {
            cmd.Parameters.Add(new SqlParameter("@IsPinned", SqlDbType.Bit) { Value = (isPinned ?? false) });
            cmd.Parameters.Add(new SqlParameter("@PopupUntil", SqlDbType.NVarChar, 40) { Value = (object?)popupUntilRaw ?? DBNull.Value });
        }
 
        await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
        var rows = (int)(await cmd.ExecuteScalarAsync() ?? 0);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(BoardContext board, int id)
    {
        var t = Q(board.PostTable);
        var rt = Q(board.ReplyTable);
        var ft = Q(board.FileTable);
        var mt = Q(MetaTable);
        var lt = Q(LikeTable);

        // ✅ 삭제 순서: 첨부 -> 댓글 -> 좋아요 -> 메타 -> 게시글
        var sql = $@"
BEGIN TRAN;
DELETE FROM {ft} WHERE PostId=@Id;
DELETE FROM {rt} WHERE PostId=@Id;
DELETE FROM {lt} WHERE PostTable=@PostTable AND PostId=@Id;
DELETE FROM {mt} WHERE PostTable=@PostTable AND PostId=@Id;
DELETE FROM {t} WHERE Id=@Id;
DECLARE @Rows INT = @@ROWCOUNT;
COMMIT TRAN;
SELECT @Rows;
";

        await using var con = new SqlConnection(_cs);
        await con.OpenAsync();
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
        cmd.Parameters.Add(new SqlParameter("@PostTable", SqlDbType.NVarChar, 128) { Value = board.PostTable });
        await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
        var rows = (int)(await cmd.ExecuteScalarAsync() ?? 0);
        return rows > 0;
    }

    // =========================================================
    // Meta / Permissions
    // =========================================================

    public async Task<(string? authorUserId, bool isNotice, int likeCount)> GetMetaAsync(BoardContext board, int postId)
    {
        var mt = Q(MetaTable);

        var sql = $@"
SELECT AuthorUserId, ISNULL(IsNotice,0), ISNULL(LikeCount,0)
FROM {mt}
WHERE PostTable=@PostTable AND PostId=@PostId;
";
        await using var con = new SqlConnection(_cs);
        await con.OpenAsync();
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add(new SqlParameter("@PostTable", SqlDbType.NVarChar, 128) { Value = board.PostTable });
        cmd.Parameters.Add(new SqlParameter("@PostId", SqlDbType.Int) { Value = postId });

        await using var r = await cmd.ExecuteReaderAsync();
        if (!await r.ReadAsync()) return (null, false, 0);

        return (
            r.IsDBNull(0) ? null : r.GetString(0),
            r.GetInt32(1) == 1,
            r.GetInt32(2)
        );
    }

    public async Task<bool> SetNoticeAsync(BoardContext board, int postId, bool isNotice)
    {
        var mt = Q(MetaTable);
        var sql = $@"
MERGE {mt} AS t
USING (SELECT @PostTable AS PostTable, @PostId AS PostId) AS s
ON t.PostTable=s.PostTable AND t.PostId=s.PostId
WHEN MATCHED THEN UPDATE SET IsNotice=@IsNotice
WHEN NOT MATCHED THEN
    INSERT(PostTable, PostId, IsNotice, LikeCount, CreatedAt)
    VALUES(@PostTable, @PostId, @IsNotice, 0, SYSDATETIME());
SELECT 1;
";
        await using var con = new SqlConnection(_cs);
        await con.OpenAsync();
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add(new SqlParameter("@PostTable", SqlDbType.NVarChar, 128) { Value = board.PostTable });
        cmd.Parameters.Add(new SqlParameter("@PostId", SqlDbType.Int) { Value = postId });
        cmd.Parameters.Add(new SqlParameter("@IsNotice", SqlDbType.Bit) { Value = isNotice });
        await cmd.ExecuteNonQueryAsync();
        return true;
    }

    /// <summary>
    /// ✅ 좋아요 토글 (Insert/Cancel) + LikeCount 갱신
    /// </summary>
    public async Task<(bool liked, int likeCount)> ToggleLikeAsync(BoardContext board, int postId, string userId)
    {
        var mt = Q(MetaTable);
        var lt = Q(LikeTable);

        var sql = $@"
BEGIN TRAN;

-- 메타 없으면 생성
IF NOT EXISTS(SELECT 1 FROM {mt} WHERE PostTable=@PostTable AND PostId=@PostId)
BEGIN
    INSERT INTO {mt}(PostTable, PostId, LikeCount, IsNotice, CreatedAt)
    VALUES(@PostTable, @PostId, 0, 0, SYSDATETIME());
END

DECLARE @Liked BIT = 0;

IF EXISTS(SELECT 1 FROM {lt} WHERE PostTable=@PostTable AND PostId=@PostId AND UserId=@UserId)
BEGIN
    DELETE FROM {lt} WHERE PostTable=@PostTable AND PostId=@PostId AND UserId=@UserId;
    UPDATE {mt} SET LikeCount = CASE WHEN LikeCount>0 THEN LikeCount-1 ELSE 0 END
    WHERE PostTable=@PostTable AND PostId=@PostId;
    SET @Liked = 0;
END
ELSE
BEGIN
    INSERT INTO {lt}(PostTable, PostId, UserId, CreatedAt)
    VALUES(@PostTable, @PostId, @UserId, SYSDATETIME());
    UPDATE {mt} SET LikeCount = LikeCount + 1
    WHERE PostTable=@PostTable AND PostId=@PostId;
    SET @Liked = 1;
END

DECLARE @Cnt INT = (SELECT LikeCount FROM {mt} WHERE PostTable=@PostTable AND PostId=@PostId);

COMMIT TRAN;

SELECT @Liked AS Liked, @Cnt AS LikeCount;
";

        await using var con = new SqlConnection(_cs);
        await con.OpenAsync();
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add(new SqlParameter("@PostTable", SqlDbType.NVarChar, 128) { Value = board.PostTable });
        cmd.Parameters.Add(new SqlParameter("@PostId", SqlDbType.Int) { Value = postId });
        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 450) { Value = userId });

        await using var r = await cmd.ExecuteReaderAsync();
        await r.ReadAsync();

        var liked = r.GetBoolean(0);
        var likeCount = r.GetInt32(1);

        return (liked, likeCount);
    }

    // =========================================================
    // Replies
    // =========================================================

    public async Task<List<BoardReply>> ListRepliesAsync(BoardContext board, int postId)
    {
        var rt = Q(board.ReplyTable);

        var sql = $@"
SELECT Id, PostId, Content, Writer, CreatedAt
FROM {rt}
WHERE PostId=@PostId
ORDER BY CreatedAt ASC;
";

        var list = new List<BoardReply>();

        await using var con = new SqlConnection(_cs);
        await con.OpenAsync();
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add(new SqlParameter("@PostId", SqlDbType.Int) { Value = postId });

        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            list.Add(new BoardReply
            {
                Id = r.GetInt32(0),
                PostId = r.GetInt32(1),
                Content = r.GetString(2),
                Writer = r.GetString(3),
                CreatedAt = r.GetDateTime(4)
            });
        }
        return list;
    }

    public async Task<int> CreateReplyAsync(BoardContext board, int postId, string content, string writer, DateTime createdAt)
    {
        var rt = Q(board.ReplyTable);

        var sql = $@"
INSERT INTO {rt}(PostId, Content, Writer, CreatedAt)
VALUES(@PostId, @Content, @Writer, @CreatedAt);
SELECT CAST(SCOPE_IDENTITY() AS INT);
";

        await using var con = new SqlConnection(_cs);
        await con.OpenAsync();
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add(new SqlParameter("@PostId", SqlDbType.Int) { Value = postId });
        cmd.Parameters.Add(new SqlParameter("@Content", SqlDbType.NVarChar, 1000) { Value = content });
        cmd.Parameters.Add(new SqlParameter("@Writer", SqlDbType.NVarChar, 60) { Value = writer });
        cmd.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime2) { Value = createdAt });
        await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
        return (int)(await cmd.ExecuteScalarAsync() ?? 0);
    }

    /// <summary>댓글 수정 (관리자) — 작성자 · 내용.  그 글(PostId)의 댓글일 때만 고친다</summary>
    public async Task<bool> UpdateReplyAsync(BoardContext board, int postId, int replyId, string content, string writer)
    {
        var rt = Q(board.ReplyTable);

        var sql = $@"
UPDATE {rt} SET Content=@Content, Writer=@Writer WHERE Id=@Id AND PostId=@PostId;
SELECT @@ROWCOUNT;
";

        await using var con = new SqlConnection(_cs);
        await con.OpenAsync();
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = replyId });
        cmd.Parameters.Add(new SqlParameter("@PostId", SqlDbType.Int) { Value = postId });
        cmd.Parameters.Add(new SqlParameter("@Content", SqlDbType.NVarChar, 1000) { Value = content });
        cmd.Parameters.Add(new SqlParameter("@Writer", SqlDbType.NVarChar, 60) { Value = writer });
        await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
        var rows = (int)(await cmd.ExecuteScalarAsync() ?? 0);
        return rows > 0;
    }

    public async Task<bool> DeleteReplyAsync(BoardContext board, int replyId)
    {
        var rt = Q(board.ReplyTable);

        var sql = $@"
DELETE FROM {rt} WHERE Id=@Id;
SELECT @@ROWCOUNT;
";

        await using var con = new SqlConnection(_cs);
        await con.OpenAsync();
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = replyId });
        await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
        var rows = (int)(await cmd.ExecuteScalarAsync() ?? 0);
        return rows > 0;
    }

    // =========================================================
    // Files (Attachments)
    // =========================================================

    public async Task<List<BoardFile>> ListFilesAsync(BoardContext board, int postId)
    {
        var ft = Q(board.FileTable);

        var sql = $@"
SELECT Id, PostId, OriginalFileName, StoredFileName, ContentType, FileSize, RelativePath, CreatedAt
FROM {ft}
WHERE PostId=@PostId
ORDER BY Id ASC;
";

        var list = new List<BoardFile>();

        await using var con = new SqlConnection(_cs);
        await con.OpenAsync();
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add(new SqlParameter("@PostId", SqlDbType.Int) { Value = postId });

        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            list.Add(new BoardFile
            {
                Id = r.GetInt32(0),
                PostId = r.GetInt32(1),
                OriginalFileName = r.GetString(2),
                StoredFileName = r.GetString(3),
                ContentType = r.GetString(4),
                FileSize = r.GetInt64(5),
                RelativePath = r.GetString(6),
                CreatedAt = r.GetDateTime(7)
            });
        }

        return list;
    }

    public async Task<int> AddFileAsync(
        BoardContext board,
        int postId,
        string originalName,
        string storedName,
        string contentType,
        long fileSize,
        string relativePath)
    {
        var ft = Q(board.FileTable);

        var sql = $@"
INSERT INTO {ft}(PostId, OriginalFileName, StoredFileName, ContentType, FileSize, RelativePath)
VALUES(@PostId, @Original, @Stored, @ContentType, @FileSize, @RelPath);
SELECT CAST(SCOPE_IDENTITY() AS INT);
";

        await using var con = new SqlConnection(_cs);
        await con.OpenAsync();
        await using var cmd = new SqlCommand(sql, con);

        cmd.Parameters.Add(new SqlParameter("@PostId", SqlDbType.Int) { Value = postId });
        cmd.Parameters.Add(new SqlParameter("@Original", SqlDbType.NVarChar, 260) { Value = originalName });
        cmd.Parameters.Add(new SqlParameter("@Stored", SqlDbType.NVarChar, 80) { Value = storedName });
        cmd.Parameters.Add(new SqlParameter("@ContentType", SqlDbType.NVarChar, 100) { Value = contentType });
        cmd.Parameters.Add(new SqlParameter("@FileSize", SqlDbType.BigInt) { Value = fileSize });
        cmd.Parameters.Add(new SqlParameter("@RelPath", SqlDbType.NVarChar, 500) { Value = relativePath });
        await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
        return (int)(await cmd.ExecuteScalarAsync() ?? 0);
    }

    public async Task<BoardFile?> GetFileAsync(BoardContext board, int fileId)
    {
        var ft = Q(board.FileTable);

        var sql = $@"
SELECT Id, PostId, OriginalFileName, StoredFileName, ContentType, FileSize, RelativePath, CreatedAt
FROM {ft}
WHERE Id=@Id;
";

        await using var con = new SqlConnection(_cs);
        await con.OpenAsync();
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = fileId });

        await using var r = await cmd.ExecuteReaderAsync();
        if (!await r.ReadAsync()) return null;

        return new BoardFile
        {
            Id = r.GetInt32(0),
            PostId = r.GetInt32(1),
            OriginalFileName = r.GetString(2),
            StoredFileName = r.GetString(3),
            ContentType = r.GetString(4),
            FileSize = r.GetInt64(5),
            RelativePath = r.GetString(6),
            CreatedAt = r.GetDateTime(7)
        };
    }

    public async Task<bool> DeleteFileAsync(BoardContext board, int fileId)
    {
        var ft = Q(board.FileTable);

        var sql = $@"
DELETE FROM {ft} WHERE Id=@Id;
SELECT @@ROWCOUNT;
";

        await using var con = new SqlConnection(_cs);
        await con.OpenAsync();
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = fileId });
        await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
        var rows = (int)(await cmd.ExecuteScalarAsync() ?? 0);
        return rows > 0;
    }
}