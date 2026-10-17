using BulletinBoard.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace BulletinBoard.DataAccessLayer
{
    /// <summary>
    /// 수시·수능 준비생을 위한 기도문 — 데이터 접근
    /// 쓰기(Insert/Update/Delete)는 전부 파라미터 바인딩을 사용합니다.
    /// ※ ControlHealingAndRestorationPray 와 같은 구조입니다.
    /// </summary>
    public class ControlEntranceExamPray
    {
        private const string SELECT_COLS = @"
            SELECT  PrayId, SortOrder, Title, PrayText,
                    ISNULL(BibleRef,  '') AS BibleRef,
                    ISNULL(BibleText, '') AS BibleText,
                    ISNULL(ClickCount, 0) AS ClickCount,
                    ISNULL(LikeCount,  0) AS LikeCount,
                    ISNULL(UseYN, 'Y')    AS UseYN,
                    ISNULL(RotateYN, 'Y') AS RotateYN,
                    CreatedAt, UpdatedAt ";

        /// <summary>
        /// 순환에 들어가는 조건.
        /// ⚠️ 순환을 다루는 모든 질의(오늘의 기도문 · 순번 · 건수)가 반드시 이 조건을
        ///    똑같이 써야 합니다. 한 곳만 어긋나면 "N편 중 M번째" 가 실제와 달라집니다.
        /// </summary>
        private const string ROTATE_WHERE = " ISNULL(UseYN,'Y') = 'Y' AND ISNULL(RotateYN,'Y') = 'Y' ";

        /// <summary>목록에 보이는 기도문 건수 (UseYN='Y')</summary>
        public int GetActiveCount()
        {
            return DataAccessModule.executeScalar(
                "SELECT COUNT(*) FROM [dbo].[EntranceExamPray] WHERE ISNULL(UseYN,'Y') = 'Y';");
        }

        /// <summary>매일 순환에 들어가는 기도문 건수 (UseYN='Y' AND RotateYN='Y')</summary>
        public int GetRotationCount()
        {
            return DataAccessModule.executeScalar(
                "SELECT COUNT(*) FROM [dbo].[EntranceExamPray] WHERE" + ROTATE_WHERE + ";");
        }

        /// <summary>
        /// 오늘의 기도문 — 누적 일수 라운드로빈
        /// idx = DATEDIFF(day, 기준일, 오늘) % 사용중건수
        ///
        /// ⚠️ 오늘 날짜는 반드시 호출자(앱)가 KST 로 계산해 넘깁니다.
        ///    SQL 안에서 GETDATE() 로 구하면 DB 서버 시계를 쓰게 되어
        ///    화면의 "N번째" 표시와 어긋납니다.
        /// </summary>
        public DataSet GetTodayPray(string baseDate, DateTime todayKst)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DECLARE @cnt INT = (SELECT COUNT(*) FROM [dbo].[EntranceExamPray] WHERE" + ROTATE_WHERE + ");");
            sb.AppendLine("IF @cnt > 0");
            sb.AppendLine("BEGIN");
            sb.AppendLine($"    DECLARE @today DATE = '{todayKst:yyyy-MM-dd}';");
            sb.AppendLine($"    DECLARE @diff  INT  = DATEDIFF(DAY, '{baseDate}', @today);");
            sb.AppendLine("    DECLARE @idx   INT  = ((@diff % @cnt) + @cnt) % @cnt;");   // 음수 방어
            sb.AppendLine("    SELECT * FROM (");
            sb.AppendLine("        " + SELECT_COLS + ",");
            sb.AppendLine("               ROW_NUMBER() OVER (ORDER BY SortOrder ASC, PrayId ASC) - 1 AS rn");
            sb.AppendLine("        FROM [dbo].[EntranceExamPray] WHERE" + ROTATE_WHERE);
            sb.AppendLine("    ) T WHERE T.rn = @idx;");
            sb.AppendLine("END");

            return DataAccessModule.commonQuery(sb.ToString());
        }

        /// <summary>
        /// 전체 목록
        ///
        /// ⚠️ 두 가지 순서가 섞여 있습니다.
        ///    · RotationNo — 매일 돌아가며 노출되는 차례. 반드시 SortOrder 기준이라야 하며
        ///      GetTodayPray 의 ROW_NUMBER 와 같은 식이어야 합니다.
        ///    · 바깥 ORDER BY — 화면에 늘어놓는 순서.
        /// </summary>
        /// <param name="includeUnused">미사용(UseYN='N') 기도문도 포함할지</param>
        /// <param name="orderByRecent">true 면 최근 수정 순, false 면 순환 차례(SortOrder) 순</param>
        public DataSet GetList(bool includeUnused, bool orderByRecent)
        {
            var sb = new StringBuilder();
            // ⚠️ RotationNo 는 순환에 들어가는 기도문끼리만 매깁니다.
            //    PARTITION 으로 갈라 두면 순환에서 빠진 행은 다른 무리에서 번호를 받고,
            //    바깥에서 0 으로 지웁니다. (화면에서 순번을 감추기 위한 표시)
            sb.AppendLine("SELECT T.*, CASE WHEN T.inRotation = 1 THEN T.rnRaw ELSE 0 END AS RotationNo FROM (");
            sb.AppendLine("    " + SELECT_COLS + ",");
            sb.AppendLine("           CASE WHEN ISNULL(UseYN,'Y') = 'Y' AND ISNULL(RotateYN,'Y') = 'Y' THEN 1 ELSE 0 END AS inRotation,");
            sb.AppendLine("           ROW_NUMBER() OVER (");
            sb.AppendLine("               PARTITION BY CASE WHEN ISNULL(UseYN,'Y') = 'Y' AND ISNULL(RotateYN,'Y') = 'Y' THEN 1 ELSE 0 END");
            sb.AppendLine("               ORDER BY SortOrder ASC, PrayId ASC) AS rnRaw");
            sb.AppendLine("    FROM [dbo].[EntranceExamPray]");
            if (!includeUnused)
                sb.AppendLine("    WHERE ISNULL(UseYN,'Y') = 'Y'");
            sb.AppendLine(orderByRecent
                ? ") T ORDER BY ISNULL(T.UpdatedAt, T.CreatedAt) DESC, T.PrayId DESC;"
                : ") T ORDER BY T.SortOrder ASC, T.PrayId ASC;");

            return DataAccessModule.commonQuery(sb.ToString());
        }

        /// <summary>단건 조회</summary>
        public DataSet GetById(int prayId)
        {
            var sb = new StringBuilder();
            sb.AppendLine(SELECT_COLS);
            sb.AppendLine("FROM [dbo].[EntranceExamPray]");
            sb.AppendLine($"WHERE PrayId = {prayId};");
            return DataAccessModule.commonQuery(sb.ToString());
        }

        /// <summary>
        /// 이 기도문이 순환 목록에서 몇 번째인지 (1부터). 숨김이거나 없으면 0.
        /// ※ 반드시 GetTodayPray / GetList 의 ROW_NUMBER 와 같은 식이라야 합니다.
        /// </summary>
        public int GetRotationNo(int prayId)
        {
            // ⚠️ ROW_NUMBER() 는 BIGINT 를 돌려줍니다. executeScalar 가 (Int32) 로
            //    바로 캐스팅하므로 여기서 INT 로 줄여 두지 않으면 예외가 나고 0 이 됩니다.
            var sql = @"
                SELECT ISNULL((
                    SELECT CAST(T.rn AS INT) FROM (
                        SELECT PrayId, ROW_NUMBER() OVER (ORDER BY SortOrder ASC, PrayId ASC) AS rn
                          FROM [dbo].[EntranceExamPray]
                         WHERE" + ROTATE_WHERE + @"
                    ) T WHERE T.PrayId = " + prayId + @"
                ), 0);";

            return DataAccessModule.executeScalar(sql);
        }

        /// <summary>
        /// 검증 완료 표시 — 내용은 그대로 두고 수정 시각만 지금(KST)으로 올립니다.
        /// 훑어본 기도문을 목록 맨 뒤로 보내, 오래 보지 않은 것부터 검토하기 위한 것입니다.
        /// </summary>
        public void TouchUpdatedAt(int prayId)
        {
            var sql = @"
                UPDATE [dbo].[EntranceExamPray]
                   SET UpdatedAt = DATEADD(HOUR, " + ConstatsCommon.timeDifference16 + @", GETDATE())
                 WHERE PrayId = @PrayId;";

            DataAccessModule.executeNonQuery(sql, new List<SqlParameter>
            {
                new SqlParameter("@PrayId", SqlDbType.Int) { Value = prayId }
            });
        }

        /// <summary>
        /// 가장 오래 손대지 않은 기도문의 PrayId. 없으면 0.
        /// ※ 한 번도 고친 적 없는 행은 UpdatedAt 이 비어 있으므로 등록일로 대신 봅니다.
        /// </summary>
        public int GetOldestUpdatedPrayId()
        {
            var sql = @"
                SELECT ISNULL((
                    SELECT TOP 1 PrayId
                      FROM [dbo].[EntranceExamPray]
                     ORDER BY ISNULL(UpdatedAt, CreatedAt) ASC, PrayId ASC
                ), 0);";

            return DataAccessModule.executeScalar(sql);
        }

        /// <summary>클릭 수 +1</summary>
        public void IncreaseClick(int prayId)
        {
            var sql = "UPDATE [dbo].[EntranceExamPray] SET ClickCount = ISNULL(ClickCount,0) + 1 WHERE PrayId = @PrayId;";
            DataAccessModule.executeNonQuery(sql, new List<SqlParameter>
            {
                new SqlParameter("@PrayId", SqlDbType.Int) { Value = prayId }
            });
        }

        /// <summary>
        /// 좋아요 +1 하고, 올라간 뒤의 값을 돌려준다.
        /// (UPDATE 와 조회를 한 번에 처리해 화면이 최신 값을 바로 받게 한다)
        /// </summary>
        public int IncreaseLike(int prayId)
        {
            var sql = @"
                UPDATE [dbo].[EntranceExamPray]
                   SET LikeCount = ISNULL(LikeCount,0) + 1
                 WHERE PrayId = " + prayId + @";
                SELECT ISNULL(LikeCount,0) FROM [dbo].[EntranceExamPray] WHERE PrayId = " + prayId + ";";

            return DataAccessModule.executeScalar(sql);
        }

        /// <summary>현재 좋아요 수</summary>
        public int GetLikeCount(int prayId)
        {
            return DataAccessModule.executeScalar(
                "SELECT ISNULL(LikeCount,0) FROM [dbo].[EntranceExamPray] WHERE PrayId = " + prayId + ";");
        }

        /// <summary>등록</summary>
        public void Insert(string title, string prayText, string bibleRef, string bibleText, int sortOrder, string useYN, string rotateYN)
        {
            var sql = @"
                INSERT INTO [dbo].[EntranceExamPray]
                    (SortOrder, Title, PrayText, BibleRef, BibleText, UseYN, RotateYN, CreatedAt)
                VALUES
                    (@SortOrder, @Title, @PrayText, @BibleRef, @BibleText, @UseYN, @RotateYN,
                     DATEADD(HOUR, " + ConstatsCommon.timeDifference16 + @", GETDATE()));";

            DataAccessModule.executeNonQuery(sql, new List<SqlParameter>
            {
                new SqlParameter("@RotateYN",  SqlDbType.Char, 1)      { Value = string.IsNullOrWhiteSpace(rotateYN) ? "Y" : rotateYN },
                new SqlParameter("@SortOrder", SqlDbType.Int)          { Value = sortOrder },
                new SqlParameter("@Title",     SqlDbType.NVarChar, 200){ Value = title     ?? "" },
                new SqlParameter("@PrayText",  SqlDbType.NVarChar, -1) { Value = prayText  ?? "" },
                new SqlParameter("@BibleRef",  SqlDbType.NVarChar, 100){ Value = bibleRef  ?? "" },
                new SqlParameter("@BibleText", SqlDbType.NVarChar,1000){ Value = bibleText ?? "" },
                new SqlParameter("@UseYN",     SqlDbType.Char, 1)      { Value = string.IsNullOrWhiteSpace(useYN) ? "Y" : useYN }
            });
        }

        /// <summary>수정 — ClickCount / LikeCount 는 건드리지 않습니다</summary>
        public void Update(int prayId, string title, string prayText, string bibleRef, string bibleText, int sortOrder, string useYN, string rotateYN)
        {
            var sql = @"
                UPDATE [dbo].[EntranceExamPray]
                   SET SortOrder = @SortOrder,
                       Title     = @Title,
                       PrayText  = @PrayText,
                       BibleRef  = @BibleRef,
                       BibleText = @BibleText,
                       UseYN     = @UseYN,
                       RotateYN  = @RotateYN,
                       UpdatedAt = DATEADD(HOUR, " + ConstatsCommon.timeDifference16 + @", GETDATE())
                 WHERE PrayId = @PrayId;";

            DataAccessModule.executeNonQuery(sql, new List<SqlParameter>
            {
                new SqlParameter("@RotateYN",  SqlDbType.Char, 1)      { Value = string.IsNullOrWhiteSpace(rotateYN) ? "Y" : rotateYN },
                new SqlParameter("@PrayId",    SqlDbType.Int)          { Value = prayId },
                new SqlParameter("@SortOrder", SqlDbType.Int)          { Value = sortOrder },
                new SqlParameter("@Title",     SqlDbType.NVarChar, 200){ Value = title     ?? "" },
                new SqlParameter("@PrayText",  SqlDbType.NVarChar, -1) { Value = prayText  ?? "" },
                new SqlParameter("@BibleRef",  SqlDbType.NVarChar, 100){ Value = bibleRef  ?? "" },
                new SqlParameter("@BibleText", SqlDbType.NVarChar,1000){ Value = bibleText ?? "" },
                new SqlParameter("@UseYN",     SqlDbType.Char, 1)      { Value = string.IsNullOrWhiteSpace(useYN) ? "Y" : useYN }
            });
        }

        /// <summary>삭제</summary>
        public void Delete(int prayId)
        {
            var sql = "DELETE FROM [dbo].[EntranceExamPray] WHERE PrayId = @PrayId;";
            DataAccessModule.executeNonQuery(sql, new List<SqlParameter>
            {
                new SqlParameter("@PrayId", SqlDbType.Int) { Value = prayId }
            });
        }
    }
}
