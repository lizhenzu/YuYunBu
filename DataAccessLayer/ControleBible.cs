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
using SixLabors.ImageSharp.Formats.Jpeg;
using BulletinBoard.Helpers;

namespace BulletinBoard.DataAccessLayer
{
    public class ControleBible
    {
 

        public DataSet fetcheBible2(string BibleText, int FromJang, int Fromzul, int ToJang, int Tozul, string langCD, string UserId)
        {
            StringBuilder sb = new StringBuilder();

            if(string.IsNullOrEmpty(UserId))
            {
                UserId = "asdlkjhasdkhasj";
            }

            sb.AppendFormat(" select sequenceNo,  MA.Id, MA.BibleText, Jang, zul, [BibleData] , C.BibleKeyWord , C.BibleSummary ,  ");
            sb.AppendFormat("        FromJang, FromZul, ToJang, ToZul, ");
            sb.AppendFormat("        LangCD  , ");
            sb.AppendFormat("        isnull(readDone, 'f') readDone, ");
            sb.AppendFormat("        isnull(bookmarkDone, 'f') bookmarkDone ");
            sb.AppendFormat(" from ");
            sb.AppendFormat(" ( ");

            if (langCD == "al")
            {
                sb.AppendFormat(QueryByLang(BibleText, FromJang, Fromzul, ToJang, Tozul, "ko"));
                sb.AppendFormat(" union ");
                sb.AppendFormat(QueryByLang(BibleText, FromJang, Fromzul, ToJang, Tozul, "en"));
            }
            else
            {
                sb.AppendFormat(QueryByLang(BibleText, FromJang, Fromzul, ToJang, Tozul, langCD));
            }

            sb.AppendFormat(" ) MA "); sb.AppendLine();

            sb.AppendFormat($" left outer join [eBibleSummary]  C on  MA.BibleText = c.BibleText and MA.jang = c.fromjang and MA.zul = c.fromzul "); sb.AppendLine();

            sb.AppendFormat($" left outer join (select * , 't' readDone     from [dbo].[UserBibleProgress] where [UserId] = '{UserId}' ) D  on  MA.Id = D.VerseId "); sb.AppendLine();

            sb.AppendFormat($" left outer join (select * , 't' bookmarkDone from [dbo].[UserBookMark]      where [UserId] = '{UserId}' ) DM on  MA.Id = DM.VerseId "); sb.AppendLine();


            sb.AppendFormat("  ORDER BY Jang, zul , LangCD desc "); sb.AppendLine();

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());

            DbLogger.LogDebugAsync($"성경책 정보 1: {sb.ToString()}");

            return ds;
        }

        public DataSet fetcheBibleYouTuBu(string BibleText, int Jang)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select Id, BibleText, Jang, URL, TimeMM, TimeSS  "); sb.AppendLine();
            sb.AppendFormat(" from [eBibleYouTuBu] "); sb.AppendLine();
            sb.AppendFormat(" where 1 = 1 "); sb.AppendLine();
            if (!string.IsNullOrEmpty(BibleText))
            {
                sb.AppendFormat(" and [BibleText] =  '{0}' ", BibleText); sb.Append(Environment.NewLine);
            }
            if (Jang > 0)
            {
                sb.AppendFormat(" and [Jang] =   {0} ", Jang); sb.Append(Environment.NewLine);
            }

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }


        public DataSet fetcheBibleBaseJang(string BibleText, int Jang)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select Id , BibleText, Jang, zul, BibleData  "); sb.AppendLine();
            sb.AppendFormat(" from [eBible] "); sb.AppendLine();
            sb.AppendFormat(" where 1 = 1 "); sb.AppendLine();
            if (!string.IsNullOrEmpty(BibleText))
            {
                sb.AppendFormat(" and [BibleText] =  '{0}' ", BibleText); sb.Append(Environment.NewLine);
            }
            if (Jang > 0)
            {
                sb.AppendFormat(" and [Jang] =   {0} ", Jang); sb.Append(Environment.NewLine);
            }
            sb.AppendFormat("  order by Id ", Jang); sb.Append(Environment.NewLine);

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public DataSet fetcheBibleBaseVerseID(int VerseId)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select Id , BibleText, Jang, zul, BibleData  "); sb.AppendLine();
            sb.AppendFormat(" from [eBible] "); sb.AppendLine();
            sb.AppendFormat(" where 1 = 1 "); sb.AppendLine();
            if (VerseId > 0)
            {
                sb.AppendFormat(" and [Id] =  {0} ", VerseId); sb.Append(Environment.NewLine);
            }

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }


        public DataSet fetchBookMark(string UserId, int VerseId)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($" select Id, BibleText, Jang, zul, BibleData   "); sb.AppendLine();
            sb.AppendFormat($" from (select * from [dbo].[UserBookMark]   "); sb.AppendLine();
            sb.AppendFormat($"       where 1 = 1 "); sb.AppendLine();
            if (!string.IsNullOrEmpty(UserId))
            {
                sb.AppendFormat($"       and  [UserId] = '{UserId}'  "); sb.AppendLine();
            }
            if (VerseId > 0)
            {
                sb.AppendFormat($"       and [VerseId] = {VerseId}  "); sb.AppendLine();
            }
            sb.AppendFormat($"    ) A  "); sb.AppendLine();
            sb.AppendFormat($" left outer join  [dbo].[eBible] B on A.VerseId = B.Id "); sb.AppendLine();

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public DataSet deleteBookMark(string UserId, string VerseId)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($" delete from [dbo].[UserBookMark] where [UserId] = '{UserId}' and  [VerseId] = '{VerseId}' "); sb.AppendLine();

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }



        public string QueryByLang(string BibleText, int FromJang, int Fromzul, int ToJang, int Tozul, string langCD)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" SELECT TOP 1000 0 AS sequenceNo, ");
            sb.AppendFormat($" Id, BibleText, Jang, zul, [BibleData], '{langCD}' LangCD "); sb.AppendLine();

            if (langCD == "en")
            {
                sb.AppendFormat(" FROM [dbo].[eBibleEN] "); sb.AppendLine();
            }
            else
            {
                sb.AppendFormat(" FROM [dbo].[eBible] "); sb.AppendLine();
            }

            sb.AppendFormat(" WHERE 1 = 1 "); sb.AppendLine();

            if (!string.IsNullOrEmpty(BibleText))
            {
                sb.AppendFormat(" AND [BibleText] = '{0}' ", BibleText); sb.AppendLine();

                if (FromJang > 0 && Fromzul > 0 && ToJang > 0 && Tozul > 0)
                {

                    // 복합 키로 범위 필터링 (예: 0613 ~ 0804)
                    sb.AppendLine(" AND (");
                    sb.AppendLine("   RIGHT('00' + CAST(Jang AS VARCHAR), 3) + RIGHT('00' + CAST(zul AS VARCHAR), 3) ");
                    sb.AppendFormat("     BETWEEN '{0}{1}' AND '{2}{3}' ",
                        FromJang.ToString("D3"), Fromzul.ToString("D3"),
                        ToJang.ToString("D3"), Tozul.ToString("D3"));
                    sb.AppendLine(")");
                }
                else
                {
                    // 복합 키로 범위 필터링 (예: 0613 ~ 0804)
                    sb.AppendLine($" AND Jang in ({FromJang},{ToJang})");
                }
            }

            return sb.ToString();
        }

        public DataSet fetcheBib11le2(string BibleText, int Jang, string langCD)
        {
            StringBuilder sb = new StringBuilder();

            //  sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY  Jang, zul )  sequenceNo ,");
            sb.AppendFormat(" select top 100  0  sequenceNo ,");
            sb.AppendFormat(" Id, BibleText, Jang, zul, [BibleData] "); sb.Append(Environment.NewLine);
            if (langCD == "en")
            {
                sb.AppendFormat(" from [dbo].[eBibleEN] "); sb.Append(Environment.NewLine);
            }
            else
            {
                sb.AppendFormat(" from [dbo].[eBible] "); sb.Append(Environment.NewLine);
            }
            sb.AppendFormat(" where 1 = 1                         ");
            if (!string.IsNullOrEmpty(BibleText))
            {
                sb.AppendFormat(" and [BibleText] =  '{0}' ", BibleText); sb.Append(Environment.NewLine);

                //sb.AppendFormat(" and  Jang between {0} and {1} ", FromJang, ToJang); sb.Append(Environment.NewLine);
                //sb.AppendFormat(" and  zul  between {0} and {1} ", Fromzul, Tozul); sb.Append(Environment.NewLine);
                sb.AppendFormat(" AND Jang = {0}  ", Jang); sb.Append(Environment.NewLine);
            }

            sb.AppendFormat("  order by Jang,  zul                "); sb.Append(Environment.NewLine);
            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }
        //public int fetchMaxBibleID(string UserName)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendFormat(" SELECT ISNULL(   "); sb.Append(Environment.NewLine);
        //    sb.AppendFormat("     (SELECT TOP 1 BibleID    "); sb.Append(Environment.NewLine);
        //    sb.AppendFormat("      FROM [dbo].[eBibleStamp]    "); sb.Append(Environment.NewLine);
        //    sb.AppendFormat($"      WHERE UserName = '{UserName}'    "); sb.Append(Environment.NewLine);
        //    sb.AppendFormat("      ORDER BY updateDT DESC), 1) AS BibleID   "); sb.Append(Environment.NewLine);
        //    return DataAccessModule.executeScalar(sb.ToString());
        //}

        public int fetchMinBibleIDByBibleText(string BibleText, int Jang, int Zul)
        {

            if (Jang == 0)
            {
                Jang = 1;
                Zul = 1;
            }
            return DataAccessModule.executeScalar($"SELECT id FROM [dbo].[eBible] WHERE BibleText = '{BibleText}' and Jang = {Jang} and Zul = {Zul};");
        }

        public void InsertUserBibl22213213eReadRan11ge(string bibleText, int fromJang, int fromZul, int toJang, int toZul, string userId)
        {
            // Step 1: FromZul/ToZul이 0이고 동일 장이면 해당 장의 마지막 절 조회
            if (fromJang == toJang && (fromZul == 0 || toZul == 0))
            {
                string findMaxZulSql = @"
                                        SELECT MAX(zul) 
                                        FROM eBible 
                                        WHERE BibleText = @BibleText AND Jang = @Jang  ";

                using (SqlConnection conn = new SqlConnection(DataAccessModule.connectionString_Server))
                {
                    using (SqlCommand cmd = new SqlCommand(findMaxZulSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@BibleText", bibleText);
                        cmd.Parameters.AddWithValue("@Jang", fromJang);

                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        int maxZul = (result != DBNull.Value) ? Convert.ToInt32(result) : 0;

                        // 0인 경우 전체 장으로 처리
                        if (fromZul == 0) fromZul = 1;
                        if (toZul == 0) toZul = maxZul;
                    }
                }
            }

            // Step 2: 구간에 해당하는 절들을 UserBibleProgress에 INSERT (중복 제외)
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("INSERT INTO UserBibleProgress (UserId, VerseId, ReadDate)");
            sb.AppendLine("SELECT ");
            sb.AppendLine("    @UserId,");
            sb.AppendLine("    E.Id,");
            sb.AppendLine($"    DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()) ");
            sb.AppendLine("FROM eBible E");
            sb.AppendLine("WHERE E.BibleText = @BibleText");
            sb.AppendLine("  AND (");
            sb.AppendLine("        (E.Jang > @FromJang AND E.Jang < @ToJang)");
            sb.AppendLine("     OR (E.Jang = @FromJang AND E.zul >= @FromZul)");
            sb.AppendLine("     OR (E.Jang = @ToJang AND E.zul <= @ToZul)");
            sb.AppendLine("     OR (E.Jang = @FromJang AND E.Jang = @ToJang AND E.zul BETWEEN @FromZul AND @ToZul)");
            sb.AppendLine("  )");
            sb.AppendLine("  AND NOT EXISTS (");
            sb.AppendLine("        SELECT 1 FROM UserBibleProgress UBP");
            sb.AppendLine("        WHERE UBP.UserId = @UserId AND UBP.VerseId = E.Id");
            sb.AppendLine("  );");

            using (SqlConnection conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                using (SqlCommand cmd = new SqlCommand(sb.ToString(), conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@BibleText", bibleText);
                    cmd.Parameters.AddWithValue("@FromJang", fromJang);
                    cmd.Parameters.AddWithValue("@FromZul", fromZul);
                    cmd.Parameters.AddWithValue("@ToJang", toJang);
                    cmd.Parameters.AddWithValue("@ToZul", toZul);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }


        public void InsertUserBibleReadRange(string bibleText, int fromJang, int fromZul, int toJang, int toZul, string userId)
        {
            // Step 1: 장+절을 조합한 JangZul 범위 계산
            int startJangZul = 0 ;
            int endJangZul = 0;

            if (fromJang == toJang && (fromZul == 0 || toZul == 0))
            {
                string findMaxZulSql = @"
                                        SELECT MAX(zul) 
                                        FROM eBible 
                                        WHERE BibleText = @BibleText AND Jang = @Jang  ";
            

                using (SqlConnection conn = new SqlConnection(DataAccessModule.connectionString_Server))
                {
                    using (SqlCommand cmd = new SqlCommand(findMaxZulSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@BibleText", bibleText);
                        cmd.Parameters.AddWithValue("@Jang", toJang);

                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        int maxZul = (result != DBNull.Value) ? Convert.ToInt32(result) : 0;

                        if (fromZul == 0) fromZul = 1;
                        if (toZul == 0)
                            toZul = maxZul;
                    }
                }
            } 

            startJangZul = fromJang * 1000 + fromZul;
            endJangZul = toJang * 1000 + toZul;
            // Step 2: INSERT 문 구성 (중복 제외)
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("INSERT INTO UserBibleProgress (UserId, VerseId, ReadDate)");
            sb.AppendLine("SELECT ");
            sb.AppendLine("    @UserId,");
            sb.AppendLine("    E.Id,");
            sb.AppendLine($"    DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE())");
            sb.AppendLine("FROM eBible E");
            sb.AppendLine("WHERE E.BibleText = @BibleText");
            sb.AppendLine("  AND E.JangZul BETWEEN @StartJangZul AND @EndJangZul");
            sb.AppendLine("  AND NOT EXISTS (");
            sb.AppendLine("        SELECT 1 FROM UserBibleProgress UBP");
            sb.AppendLine("        WHERE UBP.UserId = @UserId AND UBP.VerseId = E.Id");
            sb.AppendLine(");");
            
            // Step 3: DB 실행
            using (SqlConnection conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                using (SqlCommand cmd = new SqlCommand(sb.ToString(), conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@BibleText", bibleText);
                    cmd.Parameters.AddWithValue("@StartJangZul", startJangZul);
                    cmd.Parameters.AddWithValue("@EndJangZul", endJangZul);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            sb.Remove(0, sb.Length);

            sb.AppendLine(" update UserBibleProgress ");
         //   sb.AppendLine($" set [ReadDate] = DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()) ");
            sb.AppendLine($" set [ReadDate] = DATEADD(SECOND, 2, DATEADD(HOUR, {ConstatsCommon.timeDifference16}, GETDATE())) ");
            sb.AppendLine($" WHERE UserId  = '{userId}'  ");
            sb.AppendLine($" and   VerseId = (select id from  [dbo].[eBible]  where BibleText = '{bibleText}' and Jang = {toJang} and zul  = {toZul} ) "); 

            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void InsertUserBibleRe1a232132312dRange(string bibleText, int fromJang, int fromZul, int toJang, int toZul, string userId)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("INSERT INTO UserBibleProgress (UserId, VerseId, ReadDate)");
            sb.AppendLine("SELECT ");
            sb.AppendLine("    @UserId,");
            sb.AppendLine("    E.Id,");
            sb.AppendLine("    GETDATE()");
            sb.AppendLine("FROM eBible E");
            sb.AppendLine("WHERE E.BibleText = @BibleText");
            sb.AppendLine("  AND (");
            sb.AppendLine("        (E.Jang > @FromJang AND E.Jang < @ToJang)");
            sb.AppendLine("     OR (E.Jang = @FromJang AND E.zul >= @FromZul)");
            sb.AppendLine("     OR (E.Jang = @ToJang AND E.zul <= @ToZul)");
            sb.AppendLine("     OR (E.Jang = @FromJang AND E.Jang = @ToJang AND E.zul BETWEEN @FromZul AND @ToZul)");
            sb.AppendLine("  )");
            sb.AppendLine("  AND NOT EXISTS (");
            sb.AppendLine("        SELECT 1 FROM UserBibleProgress UBP");
            sb.AppendLine("        WHERE UBP.UserId = @UserId AND UBP.VerseId = E.Id");
            sb.AppendLine("  );");

            using (SqlConnection conn = new SqlConnection("your_connection_string_here"))
            {
                using (SqlCommand cmd = new SqlCommand(sb.ToString(), conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@BibleText", bibleText);
                    cmd.Parameters.AddWithValue("@FromJang", fromJang);
                    cmd.Parameters.AddWithValue("@FromZul", fromZul);
                    cmd.Parameters.AddWithValue("@ToJang", toJang);
                    cmd.Parameters.AddWithValue("@ToZul", toZul);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }



        public DataSet FetchBibleDataByBibleID(int BibleBookSerialId, int PagesForView, string LangCD, string userId)
        {
            StringBuilder sb = new StringBuilder();

            if (string.IsNullOrEmpty(LangCD) || (!string.IsNullOrEmpty(LangCD) && LangCD.ToLower() == "al"))
            {
                sb.AppendLine($"SELECT TOP {PagesForView} Id, BibleText, Jang, zul, BibleData , isnull(readDone, 'f') readDone FROM [dbo].[eBible]   MA left outer join (select * , 't' readDone from [UserBibleProgress] where [UserId] = '{userId}' ) D on  MA.Id = D.VerseId WHERE id >= {BibleBookSerialId} ");
                sb.AppendLine("UNION ALL");
                sb.AppendLine($"SELECT TOP {PagesForView} Id, BibleText, Jang, zul, BibleData , isnull(readDone, 'f') readDone FROM [dbo].[eBibleEN] MA left outer join (select * , 't' readDone from [UserBibleProgress] where [UserId] = '{userId}' ) D on  MA.Id = D.VerseId WHERE id >= {BibleBookSerialId} ");
            }
            else
            {
                if (!string.IsNullOrEmpty(LangCD) && LangCD.ToLower() == "en")
                {
                    sb.AppendLine($"SELECT TOP {PagesForView} Id, BibleText, Jang, zul, BibleData , isnull(readDone, 'f') readDone FROM [dbo].[eBibleEN] MA left outer join (select * , 't' readDone from [UserBibleProgress] where [UserId] = '{userId}' ) D on  MA.Id = D.VerseId WHERE id >= {BibleBookSerialId} ");
                }
                else
                {
                    sb.AppendLine($"SELECT TOP {PagesForView} Id, BibleText, Jang, zul, BibleData , isnull(readDone, 'f') readDone FROM [dbo].[eBible]   MA left outer join (select * , 't' readDone from [UserBibleProgress] where [UserId] = '{userId}' ) D on  MA.Id = D.VerseId WHERE id >= {BibleBookSerialId} ");
                }
            }
            sb.AppendLine("ORDER BY id;");

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public void UpsertBibleStampRange(int CurrntBibleID, int CurrntBibleIDLoop, string UserName)
        {
            for (int i = CurrntBibleID + 1; i <= CurrntBibleID + CurrntBibleIDLoop; i++)
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("IF EXISTS (SELECT 1 FROM [dbo].[eBibleStamp] WHERE [BibleID] = " + i + " AND [UserName] = '" + UserName + "')");
                sb.AppendLine("BEGIN");
                sb.AppendLine("    UPDATE [dbo].[eBibleStamp]");
                sb.AppendLine("    SET [updateDT] = GETDATE()");
                sb.AppendLine("    WHERE [BibleID] = " + i + " AND [UserName] = '" + UserName + "';");
                sb.AppendLine("END");
                sb.AppendLine("ELSE");
                sb.AppendLine("BEGIN");
                sb.AppendLine("    INSERT INTO [dbo].[eBibleStamp] ([BibleID], [UserName])");
                sb.AppendLine("    VALUES (" + i + ", '" + UserName + "');");
                sb.AppendLine("END");

                // 실제 DB 실행
                DataAccessModule.executeNonQuery(sb.ToString());
            }
        }


        public bool InsertOrDeleteUserBibleProgress(string UserId, int VerseId)
        {
            string sql = $@"
                            IF EXISTS (SELECT 1 FROM [dbo].[UserBibleProgress]
                                       WHERE [UserId] = @UserId AND [VerseId] = @VerseId)
                            BEGIN
                                DELETE FROM [dbo].[UserBibleProgress]
                                WHERE [UserId] = @UserId AND [VerseId] = @VerseId;
                                SELECT CAST(0 AS BIT);
                            END
                            ELSE
                            BEGIN
                                INSERT INTO [dbo].[UserBibleProgress] ([UserId], [VerseId], [ReadDate])
                                VALUES (@UserId, @VerseId,  DATEADD(SECOND, 2, DATEADD(HOUR, {ConstatsCommon.timeDifference16}, GETDATE())) );
                                SELECT CAST(1 AS BIT);
                            END";

            using (SqlConnection conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", UserId);
                    cmd.Parameters.AddWithValue("@VerseId", VerseId);

                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return Convert.ToBoolean(result);
                }
            }
        }

        public bool InsertOrDeleteUserBookMark(string UserId, int VerseId)
        {
            string sql = $@"
                            IF EXISTS (SELECT 1 FROM [dbo].[UserBookMark]
                                       WHERE [UserId] = @UserId AND [VerseId] = @VerseId)
                            BEGIN
                                DELETE FROM [dbo].[UserBookMark]
                                WHERE [UserId] = @UserId AND [VerseId] = @VerseId;
                                SELECT CAST(0 AS BIT);
                            END
                            ELSE
                            BEGIN
                                INSERT INTO [dbo].[UserBookMark] ([UserId], [VerseId], [InsertDT])
                                VALUES (@UserId, @VerseId,  DATEADD(SECOND, 2, DATEADD(HOUR, {ConstatsCommon.timeDifference16}, GETDATE())) );
                                SELECT CAST(1 AS BIT);
                            END";

            using (SqlConnection conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", UserId);
                    cmd.Parameters.AddWithValue("@VerseId", VerseId);

                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return Convert.ToBoolean(result);
                }
            }
        }



    }
}
