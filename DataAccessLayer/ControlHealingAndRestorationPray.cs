using BulletinBoard.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace BulletinBoard.DataAccessLayer
{
    /// <summary>
    /// 치유와 회복을 위한 기도문 — 데이터 접근
    /// 쓰기(Insert/Update/Delete)는 전부 파라미터 바인딩을 사용합니다.
    /// </summary>
    public class ControlHealingAndRestorationPray
    {
        private const string SELECT_BASE = @"
            SELECT  PrayId, SortOrder, Title, PrayText,
                    ISNULL(BibleRef,  '') AS BibleRef,
                    ISNULL(BibleText, '') AS BibleText,
                    ISNULL(ClickCount, 0) AS ClickCount,
                    ISNULL(LikeCount,  0) AS LikeCount,
                    ISNULL(UseYN, 'Y')    AS UseYN,
                    CreatedAt, UpdatedAt ";

        /// <summary>검증 칸까지 포함한 조회 칸 — 칸이 없으면(SQL 실행 전) 빈 값으로 채운다</summary>
        private string SELECT_COLS => SELECT_BASE + (HasVerifyColumns()
            ? ", CAST(ISNULL(IsVerified, 0) AS bit) AS IsVerified, VerifiedDT, VerifiedBy "
            : ", CAST(0 AS bit) AS IsVerified, CAST(NULL AS datetime) AS VerifiedDT, CAST(NULL AS nvarchar(100)) AS VerifiedBy ");

        // ── 검증 칸 (IsVerified · VerifiedDT · VerifiedBy) ──────────────────
        // SQL\AddVerifyColumns_HealingPray_20261011.sql 을 실행하기 전에도 화면이 깨지지 않도록
        // 칸이 있는지 먼저 확인한다. (결과는 60초 동안 기억 — SQL 을 실행하면 곧 켜진다)  부부 기도와 같은 방식
        private static (bool Ready, long At) _verifyReady = (false, long.MinValue);

        public bool HasVerifyColumns()
        {
            long now = Environment.TickCount64;
            var c = _verifyReady;
            if (c.At != long.MinValue && now - c.At < 60_000) return c.Ready;

            bool ready = false;
            try
            {
                // COL_LENGTH 는 smallint — int 로 바꿔서 받는다
                ready = DataAccessModule.executeScalar(
                    "SELECT CAST(ISNULL(COL_LENGTH('dbo.HealingAndRestorationPray', 'IsVerified'), 0) AS int);") > 0;
            }
            catch { ready = false; }

            _verifyReady = (ready, now);
            return ready;
        }

        /// <summary>검증 완료로 표시 — 이미 검증된 기도문은 그대로 둔다. 칸이 없으면 false.</summary>
        public bool Verify(int prayId, string by)
        {
            if (!HasVerifyColumns()) return false;

            var sql = @"
                UPDATE [dbo].[HealingAndRestorationPray]
                   SET IsVerified = 1,
                       VerifiedDT = DATEADD(HOUR, " + ConstatsCommon.timeDifference16 + @", GETDATE()),
                       VerifiedBy = @By
                 WHERE PrayId = @PrayId AND ISNULL(IsVerified, 0) = 0;";

            var who = by ?? string.Empty;
            if (who.Length > 100) who = who.Substring(0, 100);

            DataAccessModule.executeNonQuery(sql, new List<SqlParameter>
            {
                new SqlParameter("@PrayId", SqlDbType.Int)           { Value = prayId },
                new SqlParameter("@By",     SqlDbType.NVarChar, 100) { Value = who }
            });
            return true;
        }

        /// <summary>검증 취소 — 다시 "검증 전" 으로 되돌린다. 칸이 없으면 false.</summary>
        public bool Unverify(int prayId)
        {
            if (!HasVerifyColumns()) return false;

            DataAccessModule.executeNonQuery(@"
                UPDATE [dbo].[HealingAndRestorationPray]
                   SET IsVerified = 0, VerifiedDT = NULL, VerifiedBy = NULL
                 WHERE PrayId = @PrayId AND ISNULL(IsVerified, 0) = 1;",
                new List<SqlParameter> { new SqlParameter("@PrayId", SqlDbType.Int) { Value = prayId } });
            return true;
        }

        /// <summary>사용중(UseYN='Y') 기도문 건수</summary>
        public int GetActiveCount()
        {
            return DataAccessModule.executeScalar(
                "SELECT COUNT(*) FROM [dbo].[HealingAndRestorationPray] WHERE ISNULL(UseYN,'Y') = 'Y';");
        }

        /// <summary>
        /// 오늘의 기도문 — 누적 일수 라운드로빈
        /// idx = DATEDIFF(day, 기준일, 오늘) % 사용중건수
        ///
        /// ⚠️ 오늘 날짜는 반드시 호출자(앱)가 KST 로 계산해 넘깁니다.
        ///    예전처럼 SQL 안에서 GETDATE() 로 구하면 DB 서버 시계를 쓰게 되어
        ///    화면의 "N번째" 표시(앱 시계 기준)와 어긋날 수 있습니다.
        /// </summary>
        public DataSet GetTodayPray(string baseDate, DateTime todayKst)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DECLARE @cnt INT = (SELECT COUNT(*) FROM [dbo].[HealingAndRestorationPray] WHERE ISNULL(UseYN,'Y') = 'Y');");
            sb.AppendLine("IF @cnt > 0");
            sb.AppendLine("BEGIN");
            sb.AppendLine($"    DECLARE @today DATE = '{todayKst:yyyy-MM-dd}';");
            sb.AppendLine($"    DECLARE @diff  INT  = DATEDIFF(DAY, '{baseDate}', @today);");
            sb.AppendLine("    DECLARE @idx   INT  = ((@diff % @cnt) + @cnt) % @cnt;");   // 음수 방어
            sb.AppendLine("    SELECT * FROM (");
            sb.AppendLine("        " + SELECT_COLS + ",");
            sb.AppendLine("               ROW_NUMBER() OVER (ORDER BY SortOrder ASC, PrayId ASC) - 1 AS rn");
            sb.AppendLine("        FROM [dbo].[HealingAndRestorationPray] WHERE ISNULL(UseYN,'Y') = 'Y'");
            sb.AppendLine("    ) T WHERE T.rn = @idx;");
            sb.AppendLine("END");

            return DataAccessModule.commonQuery(sb.ToString());
        }

        /// <summary>
        /// 전체 목록 (관리자용 — 미사용 포함)
        ///
        /// ⚠️ 두 가지 순서가 섞여 있으니 주의하세요.
        ///    · RotationNo — 매일 돌아가며 노출되는 차례. 반드시 SortOrder 기준이라야 하며
        ///      GetTodayPray 의 ROW_NUMBER 와 같은 식이어야 합니다.
        ///    · 바깥 ORDER BY — 화면에 늘어놓는 순서. 최근에 손댄 기도문을 위로 올립니다.
        ///      (UpdatedAt 이 비어 있으면 등록 시각으로 대신합니다)
        /// </summary>
        public DataSet GetList(bool includeUnused)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SELECT * FROM (");
            sb.AppendLine("    " + SELECT_COLS + ",");
            sb.AppendLine("           ROW_NUMBER() OVER (ORDER BY SortOrder ASC, PrayId ASC) AS RotationNo");
            sb.AppendLine("    FROM [dbo].[HealingAndRestorationPray]");
            if (!includeUnused)
                sb.AppendLine("    WHERE ISNULL(UseYN,'Y') = 'Y'");
            sb.AppendLine(") T ORDER BY ISNULL(T.UpdatedAt, T.CreatedAt) DESC, T.PrayId DESC;");

            return DataAccessModule.commonQuery(sb.ToString());
        }

        /// <summary>단건 조회</summary>
        public DataSet GetById(int prayId)
        {
            var sb = new StringBuilder();
            sb.AppendLine(SELECT_COLS);
            sb.AppendLine("FROM [dbo].[HealingAndRestorationPray]");
            sb.AppendLine($"WHERE PrayId = {prayId};");
            return DataAccessModule.commonQuery(sb.ToString());
        }

        /// <summary>클릭 수 +1</summary>
        public void IncreaseClick(int prayId)
        {
            var sql = "UPDATE [dbo].[HealingAndRestorationPray] SET ClickCount = ISNULL(ClickCount,0) + 1 WHERE PrayId = @PrayId;";
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
                UPDATE [dbo].[HealingAndRestorationPray]
                   SET LikeCount = ISNULL(LikeCount,0) + 1
                 WHERE PrayId = " + prayId + @";
                SELECT ISNULL(LikeCount,0) FROM [dbo].[HealingAndRestorationPray] WHERE PrayId = " + prayId + ";";

            return DataAccessModule.executeScalar(sql);
        }

        /// <summary>현재 좋아요 수</summary>
        public int GetLikeCount(int prayId)
        {
            return DataAccessModule.executeScalar(
                "SELECT ISNULL(LikeCount,0) FROM [dbo].[HealingAndRestorationPray] WHERE PrayId = " + prayId + ";");
        }

        /// <summary>등록</summary>
        public void Insert(string title, string prayText, string bibleRef, string bibleText, int sortOrder, string useYN)
        {
            var sql = @"
                INSERT INTO [dbo].[HealingAndRestorationPray]
                    (SortOrder, Title, PrayText, BibleRef, BibleText, UseYN, CreatedAt)
                VALUES
                    (@SortOrder, @Title, @PrayText, @BibleRef, @BibleText, @UseYN,
                     DATEADD(HOUR, " + ConstatsCommon.timeDifference16 + @", GETDATE()));";

            DataAccessModule.executeNonQuery(sql, new List<SqlParameter>
            {
                new SqlParameter("@SortOrder", SqlDbType.Int)          { Value = sortOrder },
                new SqlParameter("@Title",     SqlDbType.NVarChar, 200){ Value = title     ?? "" },
                new SqlParameter("@PrayText",  SqlDbType.NVarChar, -1) { Value = prayText  ?? "" },
                new SqlParameter("@BibleRef",  SqlDbType.NVarChar, 100){ Value = bibleRef  ?? "" },
                new SqlParameter("@BibleText", SqlDbType.NVarChar,1000){ Value = bibleText ?? "" },
                new SqlParameter("@UseYN",     SqlDbType.Char, 1)      { Value = string.IsNullOrWhiteSpace(useYN) ? "Y" : useYN }
            });
        }

        /// <summary>
        /// 수정 — ClickCount 는 건드리지 않습니다.
        /// 내용이 바뀌었으니 검증 칸이 있으면 "검증 전"으로 되돌립니다 (부부 기도와 같은 규칙).
        /// </summary>
        public void Update(int prayId, string title, string prayText, string bibleRef, string bibleText, int sortOrder, string useYN)
        {
            var resetVerify = HasVerifyColumns()
                ? ", IsVerified = 0, VerifiedDT = NULL, VerifiedBy = NULL"
                : "";

            var sql = @"
                UPDATE [dbo].[HealingAndRestorationPray]
                   SET SortOrder = @SortOrder,
                       Title     = @Title,
                       PrayText  = @PrayText,
                       BibleRef  = @BibleRef,
                       BibleText = @BibleText,
                       UseYN     = @UseYN,
                       UpdatedAt = DATEADD(HOUR, " + ConstatsCommon.timeDifference16 + @", GETDATE())" + resetVerify + @"
                 WHERE PrayId = @PrayId;";

            DataAccessModule.executeNonQuery(sql, new List<SqlParameter>
            {
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
            var sql = "DELETE FROM [dbo].[HealingAndRestorationPray] WHERE PrayId = @PrayId;";
            DataAccessModule.executeNonQuery(sql, new List<SqlParameter>
            {
                new SqlParameter("@PrayId", SqlDbType.Int) { Value = prayId }
            });
        }
    }
}
