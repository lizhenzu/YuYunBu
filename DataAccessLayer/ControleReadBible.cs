using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;
using BulletinBoard.Helpers;

namespace BulletinBoard.DataAccessLayer
{
    public class ControleReadBible
    {
        /// <summary>
        /// 구약/신약 완독 수 출력
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DataSet GetBibleReadingStats(string userId)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("WITH VerseCounts AS (");
            sb.AppendLine("    SELECT BibleText, COUNT(*) AS TotalVerses");
            sb.AppendLine("    FROM eBible");
            sb.AppendLine("    GROUP BY BibleText");
            sb.AppendLine("),");
            sb.AppendLine("UserReadCounts AS (");
            sb.AppendLine("    SELECT E.BibleText, COUNT(*) AS ReadVerses");
            sb.AppendLine("    FROM eBible E");
            sb.AppendLine("    JOIN UserBibleProgress UBP ON E.Id = UBP.VerseId");
            sb.AppendLine($"    WHERE UBP.UserId = '{userId}'");
            sb.AppendLine("    GROUP BY E.BibleText");
            sb.AppendLine("),");
            sb.AppendLine("BibleWithCategory AS (");
            sb.AppendLine("    SELECT '창세기' AS BibleText, '구약' AS Category UNION ALL");
            sb.AppendLine("    SELECT '출애굽기', '구약' UNION ALL ");
            sb.AppendLine("    SELECT '레위기', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '민수기', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '신명기', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '여호수아', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '사사기', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '룻기', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '사무엘상', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '사무엘하', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '열왕기상', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '열왕기하', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '역대상', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '역대하', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '에스라', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '느헤미야', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '에스더', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '욥기', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '시편', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '잠언', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '전도서', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '아가', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '이사야', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '예레미야', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '예레미야 애가', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '에스겔', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '다니엘', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '호세아', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '요엘', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '아모스', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '오바댜', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '요나', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '미가', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '나훔', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '하박국', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '스바냐', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '학개', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '스가랴', '구약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '말라기', '구약' UNION ALL "); sb.Append(Environment.NewLine);

            sb.AppendLine("    SELECT '마태복음', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '마가복음', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '누가복음', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '요한복음', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '사도행전', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '로마서', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '고린도전서', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '고린도후서', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '갈라디아서', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '에베소서', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '빌립보서', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '골로새서', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '데살로니가전서', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '데살로니가후서', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '디모데전서', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '디모데후서', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '디도서', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '빌레몬서', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '히브리서', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '야고보서', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '베드로전서', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '베드로후서', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '요한1서', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '요한2서', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '요한3서', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '유다서', '신약' UNION ALL "); sb.Append(Environment.NewLine);
            sb.AppendLine("    SELECT '요한계시록', '신약'");
            sb.AppendLine("),");
            sb.AppendLine("Progress AS (");
            sb.AppendLine("    SELECT ");
            sb.AppendLine("        BWC.Category,");
            sb.AppendLine("        VC.BibleText,");
            sb.AppendLine("        VC.TotalVerses,");
            sb.AppendLine("        ISNULL(URC.ReadVerses, 0) AS ReadVerses,");
            sb.AppendLine("        CAST(ISNULL(URC.ReadVerses, 0) * 100.0 / VC.TotalVerses AS DECIMAL(5,2)) AS ProgressRate");
            sb.AppendLine("    FROM VerseCounts VC");
            sb.AppendLine("    LEFT JOIN UserReadCounts URC ON VC.BibleText = URC.BibleText");
            sb.AppendLine("    LEFT JOIN BibleWithCategory BWC ON VC.BibleText = BWC.BibleText");
            sb.AppendLine(")");
            sb.AppendLine("SELECT ");
            sb.AppendLine("    (SELECT COUNT(*) FROM Progress WHERE Category = '구약' AND ReadVerses = TotalVerses) AS [OldTestamentRead],");
            sb.AppendLine("    (SELECT COUNT(*) FROM Progress WHERE Category = '구약') AS [OldTestamentAll],");
            sb.AppendLine("    (SELECT COUNT(*) FROM Progress WHERE Category = '신약' AND ReadVerses = TotalVerses) AS [NewTestamentRead],");
            sb.AppendLine("    (SELECT COUNT(*) FROM Progress WHERE Category = '신약') AS [NewTestamentAll];");

            return DataAccessModule.commonQuery(sb.ToString());
        }

        /// <summary>
        /// 하루 평균 읽은 절 수
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        //public DataSet GetAverageVersesPerDay(string userId)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine("SELECT ");
        //    sb.AppendLine("    COUNT(DISTINCT CAST(ReadDate AS DATE)) AS ReadDays,");
        //    sb.AppendLine("    COUNT(*) AS TotalVerses,");
        //    sb.AppendLine("    ROUND(COUNT(*) * 1.0 / NULLIF(COUNT(DISTINCT CAST(ReadDate AS DATE)), 0), 2) AS AvgPerDay");
        //    sb.AppendLine("FROM UserBibleProgress");
        //    sb.AppendFormat("WHERE UserId = '{0}'", userId); sb.AppendLine(";");

        //    return DataAccessModule.commonQuery(sb.ToString());
        //}

        /// <summary>
        /// 현재까지의 성경 읽기 통계와 동일 속도 가정 시 완독 예상 정보를 반환한다.
        /// </summary>
        /// <param name="userId">UserBibleProgress.UserId (GUID)</param>
        /// <returns>DataSet ─ 컬럼: ReadDays, TotalVerses, AvgPerDay,
        ///          TotalBibleVerses, RemainingVerses, EstimatedDaysRemaining, ExpectedCompletionDate</returns>
        public DataSet GetReadingProgressEstimate(string userId)
        {
            var sb = new StringBuilder();

            // ① 전체 성경 절수 가져오기
            sb.AppendLine("DECLARE @TotalBibleVerses INT;");
            sb.AppendLine("SELECT @TotalBibleVerses = COUNT(*) FROM dbo.eBible;");

            // ② 사용자 진행 현황 CTE
            sb.AppendLine("WITH Progress AS (");
            sb.AppendLine("    SELECT");
            sb.AppendLine("        COUNT(DISTINCT CAST(ReadDate AS DATE)) AS ReadDays,");
            sb.AppendLine("        COUNT(*) AS TotalVerses,");
            sb.AppendLine("        ROUND(COUNT(*) * 1.0 / NULLIF(COUNT(DISTINCT CAST(ReadDate AS DATE)), 0), 2) AS AvgPerDay,");
            sb.AppendLine("        MAX(CAST(ReadDate AS DATE)) AS LastReadDate");
            sb.AppendLine("    FROM UserBibleProgress");
            sb.AppendFormat("    WHERE UserId = '{0}'", userId).AppendLine();
            sb.AppendLine(")");

            // ③ 최종 결과
            sb.AppendLine("SELECT");
            sb.AppendLine("    p.ReadDays,");
            sb.AppendLine("    p.TotalVerses,");
            sb.AppendLine("    p.AvgPerDay,");
            sb.AppendLine("    @TotalBibleVerses AS TotalBibleVerses,");
            sb.AppendLine("    (@TotalBibleVerses - p.TotalVerses) AS RemainingVerses,");
            sb.AppendLine("    CEILING( (@TotalBibleVerses - p.TotalVerses) / NULLIF(p.AvgPerDay, 0) ) AS EstimatedDaysRemaining,");
            sb.AppendLine("    DATEADD(day,");
            sb.AppendLine("            CEILING( (@TotalBibleVerses - p.TotalVerses) / NULLIF(p.AvgPerDay, 0) ),");
            sb.AppendLine("            p.LastReadDate) AS ExpectedCompletionDate");
            sb.AppendLine("FROM Progress p;");

            return DataAccessModule.commonQuery(sb.ToString());
        }


        /// <summary>
        /// 최근 읽은 날짜
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DateTime? GetLastReadDate(string userId)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("SELECT MAX(ReadDate) AS LastRead FROM UserBibleProgress WHERE UserId = '{0}'", userId);

            DataTable dt = DataAccessModule.commonQuery(sb.ToString()).Tables[0];
            if (dt.Rows.Count > 0 && dt.Rows[0]["LastRead"] != DBNull.Value)
            {
                return Convert.ToDateTime(dt.Rows[0]["LastRead"]);
            }
            return null;
        }

        /// <summary>
        /// 미완독 책 리스트와 진행률
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// 
        public DataSet GetUnfinishedBooksWithLastRead(string userId, string BibleText)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("WITH VerseCounts AS (");
            sb.AppendLine("    SELECT BibleText, COUNT(*) AS TotalVerses");
            sb.AppendLine("    FROM eBible");
            sb.AppendLine("    GROUP BY BibleText");
            sb.AppendLine("),");
            sb.AppendLine("UserReadCounts AS (");
            sb.AppendLine("    SELECT E.BibleText, COUNT(*) AS ReadVerses");
            sb.AppendLine("    FROM eBible E");
            sb.AppendLine("    JOIN UserBibleProgress UBP ON E.Id = UBP.VerseId");
            sb.AppendFormat("    WHERE UBP.UserId = '{0}'", userId); sb.AppendLine();
            sb.AppendLine("    GROUP BY E.BibleText");
            sb.AppendLine("),");
            sb.AppendLine("LastReadInfo AS (");
            sb.AppendLine("    SELECT ");
            sb.AppendLine("        E.ID,");
            sb.AppendLine("        E.BibleText,");
            sb.AppendLine("        E.Jang,");
            sb.AppendLine("        E.Zul,");
            sb.AppendLine("        UBP.ReadDate,");
            sb.AppendLine("        ROW_NUMBER() OVER (PARTITION BY E.BibleText ORDER BY UBP.ReadDate DESC) AS rn");
            sb.AppendLine("    FROM eBible E");
            sb.AppendLine("    JOIN UserBibleProgress UBP ON E.Id = UBP.VerseId");
            sb.AppendFormat("    WHERE UBP.UserId = '{0}'", userId); sb.AppendLine();
            sb.AppendLine("),");
            sb.AppendLine("Progress AS (");
            sb.AppendLine("    SELECT ");
            sb.AppendLine("        VC.BibleText,");
            sb.AppendLine("        VC.TotalVerses,");
            sb.AppendLine("        ISNULL(URC.ReadVerses, 0) AS ReadVerses,");
            sb.AppendLine("        CAST(ISNULL(URC.ReadVerses, 0) * 100.0 / VC.TotalVerses AS DECIMAL(5,2)) AS ProgressRate");
            sb.AppendLine("    FROM VerseCounts VC");
            sb.AppendLine("    LEFT JOIN UserReadCounts URC ON VC.BibleText = URC.BibleText");
            sb.AppendLine(")");
            sb.AppendLine("SELECT ");
            sb.AppendLine("    P.BibleText, ");
            sb.AppendLine("    P.ProgressRate, ");
            sb.AppendLine("    BBI.Id   AS BibleBookId,");
            sb.AppendLine("    LRI.Id   AS BibleBookSerialId,");
            sb.AppendLine("    LRI.Jang AS LastReadJang,");
            sb.AppendLine("    LRI.Zul  AS LastReadZul,");
            sb.AppendLine("    LRI.ReadDate AS LastReadDate");
            sb.AppendLine("FROM Progress P");
            sb.AppendLine("JOIN BibleBasicInfo BBI ON P.BibleText = BBI.BibleText");
            sb.AppendLine("LEFT JOIN LastReadInfo LRI ON P.BibleText = LRI.BibleText AND LRI.rn = 1");
            sb.AppendLine("WHERE P.ReadVerses < P.TotalVerses");
            if (!string.IsNullOrEmpty(BibleText))
            {
                sb.AppendLine($"and P.BibleText = '{BibleText}'");
            }
            sb.AppendLine("ORDER BY P.ProgressRate DESC;");

            return DataAccessModule.commonQuery(sb.ToString());
        }

  
        /// <summary>
        /// 전체 통독률
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public decimal GetTotalBibleProgressRate(string userId)
        {
            StringBuilder sb = new StringBuilder();

            // ⚠️ 2026-10-04 — 통독률은 사이트 전체에서 "장 기준" 하나로 통일한다.
            //    다 읽은 장(그 장의 절을 모두 읽은 장) ÷ 전체 장(1,189) × 100, 소수 1자리.
            //    홈 카드(HomeController) · 성경통독(/Bible2, DashboardViewModel.ProgressPercent) 과 같은 계산.
            //    (예전 : 읽은 절 ÷ 전체 절 — 화면마다 숫자가 달라 보였다)
            string uid = (userId ?? "").Replace("'", "''");
            sb.AppendLine("WITH Ch AS (");
            sb.AppendLine("    SELECT BibleText, Jang, COUNT(*) AS TotalVerses FROM eBible GROUP BY BibleText, Jang");
            sb.AppendLine("), Rd AS (");
            sb.AppendLine("    SELECT E.BibleText, E.Jang, COUNT(DISTINCT UBP.VerseId) AS ReadVerses");
            sb.AppendLine("    FROM UserBibleProgress UBP JOIN eBible E ON E.Id = UBP.VerseId");
            sb.AppendFormat("    WHERE UBP.UserId = '{0}'", uid); sb.AppendLine();
            sb.AppendLine("    GROUP BY E.BibleText, E.Jang");
            sb.AppendLine(")");
            sb.AppendLine("SELECT");
            sb.AppendLine("    (SELECT COUNT(*) FROM Rd JOIN Ch ON Ch.BibleText = Rd.BibleText AND Ch.Jang = Rd.Jang AND Rd.ReadVerses >= Ch.TotalVerses) * 100.0");
            sb.AppendLine("    / NULLIF((SELECT COUNT(*) FROM Ch), 0) AS TotalProgressRate;");

            DataTable dt = DataAccessModule.commonQuery(sb.ToString()).Tables[0];

            if (dt.Rows.Count > 0 && dt.Rows[0]["TotalProgressRate"] != DBNull.Value)
            {
                return Math.Round(Convert.ToDecimal(dt.Rows[0]["TotalProgressRate"]), 1);
            }

            return 0m;
        }


        public DataSet GetUserTotalProgressRatesByUser(BilbeProcessOption param)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(SharedSQL.MakeSQL_ReadBible(param));
            DbLogger.LogDebugAsync(sb.ToString());
            return DataAccessModule.commonQuery(sb.ToString());
        }

        public DataSet GetUserTotalProgressRatesByUserCompleted()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(SharedSQL.MakeSQL_ReadBibleCompleted());
            DbLogger.LogDebugAsync(sb.ToString());
            return DataAccessModule.commonQuery(sb.ToString());
        }


        /// <summary>
        /// 최근 읽은 성경
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DataSet FetchLastReadVerse(string userId, int from, int to)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SELECT TOP 1 ");
            sb.AppendLine("    E.ID as BibleBookSerialId , ");
            sb.AppendLine("    E.BibleText, ");
            sb.AppendLine("    E.Jang, ");
            sb.AppendLine("    E.zul, ");
            sb.AppendLine("    U.ReadDate, ");
            sb.AppendLine("    B.Id AS BibleBookId ");
            sb.AppendLine("FROM UserBibleProgress U ");
            sb.AppendLine("JOIN eBible E ON U.VerseId = E.Id ");
            sb.AppendLine("JOIN BibleBasicInfo B ON E.BibleText = B.BibleText ");
            sb.AppendFormat("WHERE U.UserId = '{0}' ", userId); sb.AppendLine();
            if (from > 0 && to > 0)
            {
                sb.AppendFormat($"and E.Id between {from} and {to}  "); sb.AppendLine();
            }
            sb.AppendLine("ORDER BY U.ReadDate DESC;");

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());

            return ds;
        }
        /// <summary>
        /// 최근 읽은 성경
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int FetchTodayReadVerseCount(string userId)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("  SELECT COUNT(*) AS TodayReadCount ");
            sb.AppendLine("  FROM [dbo].[UserBibleProgress] "); 
            sb.AppendLine($"  WHERE CAST(ReadDate AS DATE) = CAST(DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE())  AS DATE) ");

            sb.AppendLine($"  and userid = '{userId}' ");

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            if (ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0 && dt.Rows[0]["TodayReadCount"] != DBNull.Value)
                {
                    return int.Parse(dt.Rows[0]["TodayReadCount"].ToString());
                }
            }

            return 0;
        }

    }
}
