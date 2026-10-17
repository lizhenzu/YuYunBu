using BulletinBoard.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace BulletinBoard.DataAccessLayer
{
	public class ControlBulkInputBible
	{
        public DataSet fetchBibleEnrollList()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select top 1005 ROW_NUMBER() OVER(ORDER BY  [BibleText],[Jang]  )  sequenceNo ,");
            sb.AppendFormat(" [BibleText],[Jang] ,count(*) DC  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[eBible] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" group by [BibleText],[Jang] "); sb.Append(Environment.NewLine);
            return DataAccessModule.commonQuery(sb.ToString());
        }

        public DataSet fetchBibleBasicList()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select   ROW_NUMBER() OVER(ORDER BY  A.Id )  sequenceNo ,");
            sb.AppendFormat(" Id,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" A.BibleText, A.BibleJang  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[BibleBasicInfo] A "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" left outer join   (select BibleText, max(InsertDT) InsertDT from [dbo].[QuestionBankData]  group by BibleText)  B on A.BibleText = b.BibleText "); sb.Append(Environment.NewLine);
            return DataAccessModule.commonQuery(sb.ToString());
        }

        public DataSet fetchBibleYouTuBuEnrollList___(string ab)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select   ROW_NUMBER() OVER(ORDER BY  MA.Id )  sequenceNo ,  ");
            sb.AppendFormat("          MA.Id, MA.BibleText, BibleJang,  isnull(DCount * 100 / BibleJang, 0) Percentage , DCount from BibleBasicInfo MA  ");
            sb.AppendFormat(" left outer join(select BibleText, count(*) DCount from[dbo].[eBibleYouTuBu] group by BibleText) data on MA.BibleText = data.BibleText"); sb.Append(Environment.NewLine);
            if (!string.IsNullOrEmpty(ab))
            {
                sb.AppendFormat($" where kind = '{ab}'"); sb.Append(Environment.NewLine);
            }
            return DataAccessModule.commonQuery(sb.ToString());
        }
        public DataSet fetchBibleGroupByName(string ab)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select Kind2, kind2Desc ");
            sb.AppendFormat(" from [dbo].[BibleBasicInfo] ");
            sb.AppendFormat($" where Kind2 like '{ab}%' ");
            sb.AppendFormat(" group by Kind2, kind2Desc ");
            sb.AppendFormat(" order by Kind2, kind2Desc ");
            return DataAccessModule.commonQuery(sb.ToString());
        }
        public DataSet fetchBibleGroupName(string ab)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select Kind2, kind2Desc ");
            sb.AppendFormat(" from [BibleBasicInfo] ");
            sb.AppendFormat($" where kind2 = '{ab}'"); sb.Append(Environment.NewLine);
            return DataAccessModule.commonQuery(sb.ToString());
        }

        public DataSet fetchBibleYouTuBuEnrollList(string ab, string userId)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("DECLARE @UserId NVARCHAR(450) = '" + userId + "';");

            sb.AppendLine("SELECT");
            sb.AppendLine("    ROW_NUMBER() OVER(ORDER BY MA.Id) AS sequenceNo,");
            sb.AppendLine("    MA.Id,");
            sb.AppendLine("    MA.BibleText,");
            sb.AppendLine("    MA.BibleJang,");
            sb.AppendLine("    ISNULL(data.DCount * 100 / MA.BibleJang, 0) AS YouTubePercentage,");
            sb.AppendLine("    ISNULL(data.DCount, 0) AS DCount,");
            sb.AppendLine("    ISNULL(ReadStats.ReadCount * 100.0 / NULLIF(TotalStats.TotalCount, 0), 0) AS ReadPercentage,");
            sb.AppendLine("    ISNULL(ReadStats.ReadCount, 0) AS ReadCount,");
            sb.AppendLine("    ISNULL(TotalStats.TotalCount, 0) AS TotalVerseCount");

            sb.AppendLine("FROM BibleBasicInfo MA");

            sb.AppendLine("LEFT OUTER JOIN (");
            sb.AppendLine("    SELECT BibleText, COUNT(*) AS DCount");
            sb.AppendLine("    FROM [dbo].[eBibleYouTuBu]");
            sb.AppendLine("    GROUP BY BibleText");
            sb.AppendLine(") data ON MA.BibleText = data.BibleText");

            sb.AppendLine("LEFT OUTER JOIN (");
            sb.AppendLine("    SELECT BibleText, COUNT(*) AS TotalCount");
            sb.AppendLine("    FROM [dbo].[eBible]");
            sb.AppendLine("    GROUP BY BibleText");
            sb.AppendLine(") TotalStats ON MA.BibleText = TotalStats.BibleText");

            sb.AppendLine("LEFT OUTER JOIN (");
            sb.AppendLine("    SELECT E.BibleText, COUNT(*) AS ReadCount");
            sb.AppendLine("    FROM [dbo].[UserBibleProgress] U");
            sb.AppendLine("    JOIN [dbo].[eBible] E ON U.VerseId = E.Id");
            sb.AppendLine("    WHERE U.UserId = @UserId");
            sb.AppendLine("    GROUP BY E.BibleText");
            sb.AppendLine(") ReadStats ON MA.BibleText = ReadStats.BibleText");

            sb.AppendLine($"WHERE MA.Kind2 = '{ab}'");
            sb.AppendLine("ORDER BY MA.Id;");

            return DataAccessModule.commonQuery(sb.ToString());
        }

        public DataSet fetchBibleBasicListByBibleText(int Id, string BibleText)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select ROW_NUMBER() OVER(ORDER BY  Id )  sequenceNo ,");
            sb.AppendFormat(" Id, BibleText, BibleJang  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[BibleBasicInfo] "); sb.Append(Environment.NewLine);

            if (Id > 0)
            {
                sb.AppendFormat($" where Id = {Id} "); sb.Append(Environment.NewLine);
            }
            else
            {
                sb.AppendFormat($" where BibleText = '{BibleText}' "); sb.Append(Environment.NewLine);
            }
            return DataAccessModule.commonQuery(sb.ToString());
        }

        public DataSet fetchBibleBasicListByBibleText2_(int Id, string BibleText)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select ROW_NUMBER() OVER(ORDER BY  Id )  sequenceNo ,");
            sb.AppendFormat(" Id, BibleText, BibleJang, OverView, WhenWritten, Author, Purpose , SummaryURL1, SummaryURL2 "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[BibleBasicInfo] "); sb.Append(Environment.NewLine);

            if (Id > 0)
            {
                sb.AppendFormat($" where Id = {Id} "); sb.Append(Environment.NewLine);
            }
            else
            {
                sb.AppendFormat($" where BibleText = '{BibleText}' "); sb.Append(Environment.NewLine);
            }
            return DataAccessModule.commonQuery(sb.ToString());
        }

        public DataSet fetchBibleBasicListByBibleText3_(int Id, string BibleText)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select ROW_NUMBER() OVER(ORDER BY  Id )  sequenceNo ,");
            sb.AppendFormat(" Id, BookName, Author, AuthorDescription, WrittenDateFrom, WrittenDateTo, AlternativeDate, WrittenPlace, WrittenTarget, HistoricalPeriod, HistoricalBackground, Keywords, ContentSummary , SummaryURL1, SummaryURL2 "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[BibleBookOverview_Detail] "); sb.Append(Environment.NewLine);

            if (Id > 0)
            {
                sb.AppendFormat($" where Id = {Id} "); sb.Append(Environment.NewLine);
            }
            else
            {
                sb.AppendFormat($" where BookName = '{BibleText}' "); sb.Append(Environment.NewLine);
            }
            return DataAccessModule.commonQuery(sb.ToString());
        }




        public DataSet fetchBibleYouTuBuList(int Id, string BibleText, string UserId)
        {
            StringBuilder sb = new StringBuilder();

            if (UserId.ToLower() == "guest")
            {
                UserId = string.Empty;
            }

            sb.AppendLine("DECLARE @maxJang INT;");
            sb.AppendLine("DECLARE @bibleText NVARCHAR(100);");
            sb.AppendFormat("SET @bibleText = '{0}';", BibleText); sb.AppendLine();

            if (Id > 0)
            {
                sb.AppendFormat("SELECT @maxJang = BibleJang FROM [dbo].[BibleBasicInfo] WHERE Id = {0};", Id); sb.AppendLine();
                sb.AppendFormat("SELECT @bibleText = BibleText FROM [dbo].[BibleBasicInfo] WHERE Id = {0};", Id); sb.AppendLine();
            }
            else
            {
                sb.AppendFormat("SELECT @maxJang = BibleJang FROM [dbo].[BibleBasicInfo] WHERE BibleText = '{0}';", BibleText); sb.AppendLine();
            }

            sb.AppendLine("SELECT ");
            sb.AppendLine("    s.SeqNo, ");
            sb.AppendLine("    e.*, ");
            sb.AppendLine("    bb.videoId, ");
            sb.AppendLine("    ISNULL(CAST(ReadCount.ReadZulCount * 100.0 / TotalCount.TotalZulCount AS DECIMAL(5,2)), 0) AS ProgressRate");
            sb.AppendLine("FROM sequenceTable s");

            // 유튜브 영상 조인
            sb.AppendLine("LEFT OUTER JOIN (");
            sb.AppendLine("    SELECT MB.* , MA.id as BibleId");
            sb.AppendLine("    FROM [dbo].[BibleBasicInfo] MA");
            sb.AppendLine("    JOIN [dbo].[eBibleYouTuBu] MB ON MA.BibleText = MB.BibleText");
            if (Id > 0)
            {
                sb.AppendFormat("    WHERE MA.Id = {0}", Id); sb.AppendLine();
            }
            else
            {
                sb.AppendFormat("    WHERE MA.BibleText = '{0}'", BibleText); sb.AppendLine();
            }
            sb.AppendLine(") e ON s.SeqNo = e.Jang");

            // SuperAdmin 유튜브 시청 이력
            sb.AppendFormat("LEFT OUTER JOIN (SELECT DISTINCT videoId, UserId FROM [dbo].[eBibleYouTuBuHit] WHERE [UserId] = '{0}') bb ", UserId); sb.AppendLine("ON e.URL = bb.videoId");

            // 장별 전체 절 수
            sb.AppendLine("LEFT OUTER JOIN (");
            sb.AppendLine("    SELECT Jang, COUNT(*) AS TotalZulCount");
            sb.AppendLine("    FROM [dbo].[eBible]");
            sb.AppendLine("    WHERE BibleText = @bibleText");
            sb.AppendLine("    GROUP BY Jang");
            sb.AppendLine(") TotalCount ON s.SeqNo = TotalCount.Jang");

            // 장별 읽은 절 수
            sb.AppendLine("LEFT OUTER JOIN (");
            sb.AppendLine("    SELECT E.Jang, COUNT(*) AS ReadZulCount");
            sb.AppendLine("    FROM [dbo].[UserBibleProgress] U");
            sb.AppendLine("    JOIN [dbo].[eBible] E ON U.VerseId = E.Id");
            sb.AppendFormat("    WHERE U.UserId = '{0}' AND E.BibleText = @bibleText", UserId); sb.AppendLine();
            sb.AppendLine("    GROUP BY E.Jang");
            sb.AppendLine(") ReadCount ON s.SeqNo = ReadCount.Jang");

            sb.AppendLine("WHERE s.SeqNo <= @maxJang");
            sb.AppendLine("ORDER BY s.SeqNo;");

            return DataAccessModule.commonQuery(sb.ToString());
        }


        public DataSet fetchBibleYo11uTuBuL1ist(int Id, string BibleText, string userName)
        {
            StringBuilder sb = new StringBuilder();

            if (userName.ToLower() == "guest")
            {
                userName = string.Empty;
            }
       
            sb.AppendFormat(" DECLARE @maxJang INT;  "); sb.Append(Environment.NewLine);
            if (Id > 0)
            {
                sb.AppendFormat(" SELECT @maxJang = BibleJang from [dbo].[BibleBasicInfo] where Id = {0} ", Id); sb.Append(Environment.NewLine);
            }
            else
            {
                sb.AppendFormat(" SELECT @maxJang = BibleJang from [dbo].[BibleBasicInfo] where BibleText = '{0}' ", BibleText); sb.Append(Environment.NewLine);
            }

            sb.AppendFormat(" SELECT s.SeqNo, e.* , bb.[videoId]  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" FROM sequenceTable s  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" LEFT OUTER JOIN (  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("                    select MB.*  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("                    from [dbo].[BibleBasicInfo] MA     "); sb.Append(Environment.NewLine);
            sb.AppendFormat("                    join [dbo].[eBibleYouTuBu]  MB on MA.BibleText = MB.BibleText    "); sb.Append(Environment.NewLine);
            if (Id > 0)
            {
                sb.AppendFormat("                    where MA.Id = {0} ", Id); sb.Append(Environment.NewLine);
            }
            else
            {
                sb.AppendFormat("                    where MA.BibleText = '{0}' ", BibleText); sb.Append(Environment.NewLine);
            }
            sb.AppendFormat(" ) e ON s.SeqNo = e.Jang  "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" LEFT OUTER JOIN (select  distinct videoId, UserName from [dbo].[eBibleYouTuBuHit] where userName = '{userName}') bb on e.URL = bb.[videoId] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" WHERE s.SeqNo <= @maxJang  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ORDER BY s.SeqNo;  "); sb.Append(Environment.NewLine);

            return DataAccessModule.commonQuery(sb.ToString());
        }

        public DataSet fetchBibleQuizEnrollList(string BibleText , int Jang)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select   ROW_NUMBER() OVER(ORDER BY  [BibleText], Jang, zul   )  sequenceNo ,");
            sb.AppendFormat(" Id,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" QuestionType, QuestionQ, Question1, Question2, Question3, Question4, CorrectAnswer, BibleText, Jang, zul  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[QuestionBankData] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1 "); sb.Append(Environment.NewLine);
            if (!string.IsNullOrEmpty(BibleText))
            {
                sb.AppendFormat(" and  BibleText = '{0}' ", BibleText); sb.Append(Environment.NewLine);
            }
            if (Jang > 0)
            {
                sb.AppendFormat(" and  Jang = {0} ", Jang); sb.Append(Environment.NewLine);
            }
            return DataAccessModule.commonQuery(sb.ToString());
        }


        public void SaveOrUpdateBibleYouTuBu(string bibleText, int jang, string url, int timeMM, int timeSS)
        {
            StringBuilder sb = new StringBuilder();

            url = SharedComm.ExtractYouTubeId(url);

            sb.AppendLine("IF EXISTS (SELECT 1 FROM [dbo].[eBibleYouTuBu] WHERE BibleText = @BibleText AND Jang = @Jang)");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    UPDATE [dbo].[eBibleYouTuBu]");
            sb.AppendLine("    SET");
            sb.AppendLine("        URL = @URL,");
            sb.AppendLine("        TimeMM = @TimeMM,");
            sb.AppendLine("        TimeSS = @TimeSS,");
            sb.AppendLine("        updateDT = DATEADD(HOUR, " + ConstatsCommon.timeDifference16 + ", GETDATE())");
            sb.AppendLine("    WHERE BibleText = @BibleText AND Jang = @Jang");
            sb.AppendLine("END");
            sb.AppendLine("ELSE");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    INSERT INTO [dbo].[eBibleYouTuBu]");
            sb.AppendLine("    (BibleText, Jang, URL, TimeMM, TimeSS, InsertDT, updateDT)");
            sb.AppendLine("    VALUES");
            sb.AppendLine("    (@BibleText, @Jang, @URL, @TimeMM, @TimeSS, ");
            sb.AppendLine("     DATEADD(HOUR, " + ConstatsCommon.timeDifference16 + ", GETDATE()), ");
            sb.AppendLine("     DATEADD(HOUR, " + ConstatsCommon.timeDifference16 + ", GETDATE()))");
            sb.AppendLine("END");

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@BibleText", bibleText),
                new SqlParameter("@Jang", jang),
                new SqlParameter("@URL", url),
                new SqlParameter("@TimeMM", timeMM),
                new SqlParameter("@TimeSS", timeSS)
            };

            DataAccessModule.executeNonQuery(sb.ToString(), parameters);
        }

        public void SaveBibleYouT11111ubeHit(string videoId, string userName)
        {
            StringBuilder sb = new StringBuilder();
            
            // 쿼리 작성
            sb.AppendLine("INSERT INTO [dbo].[eBibleYouTuBuHit]");
            sb.AppendLine("(videoId, UserName, InsertDT, updateDT)");
            sb.AppendLine("VALUES");
            sb.AppendLine("(@videoId, @UserName, ");
            sb.AppendLine(" DATEADD(HOUR, " + ConstatsCommon.timeDifference16 + ", GETDATE()),");
            sb.AppendLine(" DATEADD(HOUR, " + ConstatsCommon.timeDifference16 + ", GETDATE()))");

            // 파라미터 설정
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@videoId", videoId),
                new SqlParameter("@UserName", userName)
            };

            // 실행
            DataAccessModule.executeNonQuery(sb.ToString(), parameters);
        }

        public void SaveBibleYouTubeHit(string videoId, string userName)
        {
            StringBuilder sb = new StringBuilder();

            // 쿼리 작성
            sb.AppendLine("IF EXISTS (SELECT 1 FROM [dbo].[eBibleYouTuBuHit] WHERE videoId = @videoId)");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    UPDATE [dbo].[eBibleYouTuBuHit]");
            sb.AppendLine("    SET UserName = @UserName,");
            sb.AppendLine("        updateDT = DATEADD(HOUR, " + ConstatsCommon.timeDifference16 + ", GETDATE())");
            sb.AppendLine("    WHERE videoId = @videoId");
            sb.AppendLine("END");
            sb.AppendLine("ELSE");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    INSERT INTO [dbo].[eBibleYouTuBuHit] (videoId, UserName, InsertDT, updateDT)");
            sb.AppendLine("    VALUES (@videoId, @UserName,");
            sb.AppendLine("        DATEADD(HOUR, " + ConstatsCommon.timeDifference16 + ", GETDATE()),");
            sb.AppendLine("        DATEADD(HOUR, " + ConstatsCommon.timeDifference16 + ", GETDATE()))");
            sb.AppendLine("END");

            // 파라미터 설정
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@videoId", videoId),
                new SqlParameter("@UserName", userName)
            };

            // 실행
            DataAccessModule.executeNonQuery(sb.ToString(), parameters);
        }


        public DataSet fetchBibleNullList(string BibleText, int Jang)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("  select id, isnull(zul,0) zul ");
            sb.AppendFormat("   from [SerialNo] MA");
            sb.AppendFormat($"   left outer join (select zul from  [dbo].[QuestionBankData]  where BibleText = '{BibleText}' and jang = {Jang} ) B on MA.id =  B.zul");
            sb.AppendFormat("   where 1 = 1 ");
            sb.AppendFormat("   order by id");
            return DataAccessModule.commonQuery(sb.ToString());
        }

        public DataSet fetchBibleQuizEnrollGroupList(string BibleText)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("  select BibleText , Jang, count(*) DCount"); sb.Append(Environment.NewLine);
            sb.AppendFormat("  from [dbo].[QuestionBankData] "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  where 1 = 1 "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  and  BibleText = '{0}' ", BibleText); sb.Append(Environment.NewLine);
            sb.AppendFormat("  group by BibleText , Jang"); sb.Append(Environment.NewLine);
            sb.AppendFormat("  order by BibleText , Jang"); sb.Append(Environment.NewLine);

            return DataAccessModule.commonQuery(sb.ToString());
        }

        public DataSet fetchBibleMissingList()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select top 1005 ROW_NUMBER() OVER(ORDER BY  BibleTextToPray , BibleTextToPrayFromJang )  sequenceNo ,");
            sb.AppendFormat(" BibleTextToPray, BibleTextToPrayFromJang, isnull(Jang, -1) Jang   "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [bibleMissing] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1 "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" and Jang is null "); sb.Append(Environment.NewLine);
            return DataAccessModule.commonQuery(sb.ToString());
        }

        public DataSet fetchBibleMissingListByLang(string KO_BibleText)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select MA_BibleText, MA_Jang, KO_BibleText, KO_Jang, EN_BibleText, EN_Jang "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   from view_BibleText_KO_EN  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   WHERE ko_BibleText IS NULL "); sb.Append(Environment.NewLine);
            sb.AppendFormat($"   and  MA_BibleText = '{KO_BibleText}' "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" union "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" select MA_BibleText, MA_Jang, KO_BibleText, KO_Jang, EN_BibleText, EN_Jang "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   from view_BibleText_KO_EN  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   WHERE en_BibleText IS NULL "); sb.Append(Environment.NewLine);
            sb.AppendFormat($"   and  MA_BibleText = '{KO_BibleText}' "); sb.Append(Environment.NewLine);

            sb.AppendFormat(" ORDER BY MA_BibleText, MA_Jang  "); sb.Append(Environment.NewLine);

            return DataAccessModule.commonQuery(sb.ToString());
        }

        public DataSet fetchBibleMissingListB_KO_EN_SU()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select Id, BibleText, BibleText_EN, BibleJang, Chapters_KO, Verses_KO, Chapters_EN, Verses_EN, Chapters_NIV, Verses_NIV, Summary_Count "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [view_KO_EN_SU] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ORDER BY Id  "); sb.Append(Environment.NewLine);

            return DataAccessModule.commonQuery(sb.ToString());
        }

        public DataSet fetchBibleMissingListB_SU(string BibleText)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select A.Id,[BibleText] ,FROMJANG, DC   from  [dbo].[SerialNo] A "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" left outer join "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ( "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    SELECT   [BibleText], FROMJANG ,COUNT(*) DC  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    FROM [dbo].[eBibleSummary]  "); sb.Append(Environment.NewLine);
            sb.AppendFormat($"    where [BibleText] = '{BibleText}' "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    GROUP BY [BibleText], FROMJANG "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ) B  on A.id = b.FROMJANG "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" order by  A.Id "); sb.Append(Environment.NewLine);

            return DataAccessModule.commonQuery(sb.ToString());
        }

        public DataSet fetchBibleMissingListB_KJV(string BibleText)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select A.Id,[BibleText] ,FROMJANG, DC   from  [dbo].[SerialNo] A "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" left outer join "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ( "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    SELECT   [BibleText], FROMJANG ,COUNT(*) DC  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    FROM [dbo].[eBibleSummary]  "); sb.Append(Environment.NewLine);
            sb.AppendFormat($"    where [BibleText] = '{BibleText}' "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    GROUP BY [BibleText], FROMJANG "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ) B  on A.id = b.FROMJANG "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" order by  A.Id "); sb.Append(Environment.NewLine);

            return DataAccessModule.commonQuery(sb.ToString());
        }


        public DataSet fetchBibleMissingList_NIV(string BibleText)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select"); sb.Append(Environment.NewLine);

            sb.AppendFormat("  K.[BibleText] [BibleText_K], K.[Jang] [Jang_K] , K.DC DC_K ,"); sb.Append(Environment.NewLine);
            sb.AppendFormat("  E.[BibleText] [BibleText_E], E.[Jang] [Jang_E] , E.DC DC_E ,"); sb.Append(Environment.NewLine);
            sb.AppendFormat("  N.[BibleText] [BibleText_N], N.[Jang] [Jang_N] , N.DC DC_N "); sb.Append(Environment.NewLine);


            sb.AppendFormat(" from "); sb.Append(Environment.NewLine);
            sb.AppendFormat($"                  (select [BibleText] , [Jang] , count(*)  DC from [dbo].[eBible]       where bibletext ='{BibleText}'  group by [BibleText] , [Jang]) K "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" left outer join  (select [BibleText] , [Jang] , count(*)  DC from [dbo].[eBibleEN]     where bibletext ='{BibleText}'  group by [BibleText] , [Jang]) E on K.[BibleText] = E.[BibleText] and K.jang = E.[Jang]"); sb.Append(Environment.NewLine);
            sb.AppendFormat($" left outer join  (select [BibleText] , [Jang] , count(*)  DC from [dbo].[eBibleEN_NIV] where bibletext ='{BibleText}'  group by [BibleText] , [Jang]) N on K.[BibleText] = N.[BibleText] and K.jang = N.[Jang]"); sb.Append(Environment.NewLine);

            sb.AppendFormat(" order by K.Jang"); sb.Append(Environment.NewLine);

            return DataAccessModule.commonQuery(sb.ToString());
        }



        public DataSet fetchBibleMissingListByGroupByLang()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select MA_BibleText "); sb.Append(Environment.NewLine);

            sb.AppendFormat(" from ( "); sb.Append(Environment.NewLine);

            sb.AppendFormat(" select MA_BibleText, MA_Jang, KO_BibleText, KO_Jang, EN_BibleText, EN_Jang "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   from view_BibleText_KO_EN  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   WHERE ko_BibleText IS NULL "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" union "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" select MA_BibleText, MA_Jang, KO_BibleText, KO_Jang, EN_BibleText, EN_Jang "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   from view_BibleText_KO_EN  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   WHERE en_BibleText IS NULL "); sb.Append(Environment.NewLine);
            
            
            sb.AppendFormat(" ) Data "); sb.Append(Environment.NewLine);

            sb.AppendFormat(" group by MA_BibleText "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ORDER BY MA_BibleText "); sb.Append(Environment.NewLine);

            return DataAccessModule.commonQuery(sb.ToString());
        }

        public void SyncENtoNIC()
        {

            DataAccessModule.executeNonQuery($"  [dbo].[Update_eBibleEN_From_NIV]  ");
        }
     

        public void CreateBulkInputBible(string BibleTitle, int BibleJang, string BibleText, string LangCD = "ko")
		{
			string BibleData;
			  
			int zul = 1;

			BibleText = BibleText.Trim();

            if (string.IsNullOrEmpty(LangCD) || LangCD.ToLower() == "ko")
            {
                DataAccessModule.executeNonQuery($"delete from [dbo].[eBible] where BibleText = '{BibleTitle}' and Jang = {BibleJang}   ");
            }
            else
            {
                DataAccessModule.executeNonQuery($"delete from [dbo].[eBible{LangCD}] where BibleText = '{BibleTitle}' and Jang = {BibleJang}   ");
            }


            string[] lines = BibleText.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

			foreach (string line in lines)
			{
				BibleData = line.Trim();

				if (!string.IsNullOrEmpty(BibleData))
				{
                    string ddd = "";

                    if (string.IsNullOrEmpty(LangCD) || LangCD.ToLower() == "ko")
                    {
                        ddd = $"insert into [dbo].[eBible](BibleText, Jang, zul, BibleData) values('{BibleTitle}', {BibleJang}, {zul}, '{SharedComm.appendSpecialChar(BibleData).Trim()}')";
                    }
                    else
                    {
                        ddd = $"insert into [dbo].[eBible{LangCD}](BibleText, Jang, zul, BibleData) values('{BibleTitle}', {BibleJang}, {zul}, '{SharedComm.appendSpecialChar(BibleData).Trim()}')";
                    }
                    zul += 1;

					DataAccessModule.executeNonQuery(ddd);
				}

			}
		}


        public void CreateBulkQuiz(string param)
        { 
             

            DataAccessModule.executeNonQuery(param);

             
        }
    }
}
