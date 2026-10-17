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
    public class ControlWeeklyWordToRead
    {

       

        public DataSet fetchUploadDataWeeklyWordToReadAssoc(Category CategoryData, int WeeklyWordToReadID)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select top 1500 ROW_NUMBER() OVER(ORDER BY  CategoryData,URLData )  sequenceNo ,"); 
            sb.AppendFormat(" BPID.[UploadID],  CategoryData, TEXTData, URLData, case when [WeeklyWordToReadID] > 0 then CAST(1 as bit)  else CAST(0 as bit)  end checkData  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from (select *  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("       from [dbo].[UploadData]  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("       where 1 = 1 ", CategoryData); sb.Append(Environment.NewLine);

            if(CategoryData == Category.All)
            { 
            }
            else
            {
                sb.AppendFormat("       and   CategoryData = '{0}'   ", CategoryData.ToString()); sb.Append(Environment.NewLine);

            }
            sb.AppendFormat("      )    BPID  ", CategoryData); sb.Append(Environment.NewLine);
            sb.AppendFormat(" left outer join ( "); sb.Append(Environment.NewLine);
            sb.AppendFormat("                   select UploadWeeklyWordToReadAssocID, UploadID, WeeklyWordToReadID "); sb.Append(Environment.NewLine);
            sb.AppendFormat("                   from [dbo].[UploadWeeklyWordToReadAssoc] "); sb.Append(Environment.NewLine);
            sb.AppendFormat("                   where [WeeklyWordToReadID] = '{0}' ) BPW on  BPID.[UploadID] = BPW.[UploadID]  ", WeeklyWordToReadID); sb.Append(Environment.NewLine);
  
            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public DataSet fetchUploadDataWeeklyWordToReadAssoc2(Category CategoryData, int WeeklyWordToReadID)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("  SELECT TOP 500 "); sb.Append(Environment.NewLine);
            sb.AppendFormat("      ROW_NUMBER() OVER(ORDER BY UD.CategoryData, UD.URLData) AS sequenceNo, "); sb.Append(Environment.NewLine);
            sb.AppendFormat("      UD.UploadID, "); sb.Append(Environment.NewLine);
            sb.AppendFormat("      UD.CategoryData, "); sb.Append(Environment.NewLine);
            sb.AppendFormat("      UD.TEXTData, "); sb.Append(Environment.NewLine);
            sb.AppendFormat("      UD.URLData, "); sb.Append(Environment.NewLine);
            sb.AppendFormat("      CASE  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("          WHEN UWR.WeeklyWordToReadID > 0 THEN CAST(1 AS BIT) "); sb.Append(Environment.NewLine);
            sb.AppendFormat("          ELSE CAST(0 AS BIT) "); sb.Append(Environment.NewLine);
            sb.AppendFormat("      END AS checkData "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  FROM dbo.UploadData UD "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  JOIN dbo.UploadWeeklyWordToReadAssoc UWR "); sb.Append(Environment.NewLine);
            sb.AppendFormat($"      ON UD.UploadID = UWR.UploadID AND UWR.WeeklyWordToReadID = '{WeeklyWordToReadID}' "); sb.Append(Environment.NewLine);
            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public DataSet fetchWeeklyWordToRead(int WeeklyWordToReadID, bool activeData, DateTime workDT, int cYear)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select top 125 ROW_NUMBER() OVER(ORDER BY  (MA.DateFrom ) asc )  sequenceNo ,");
            sb.AppendFormat(" MA.Id, ");
            sb.AppendFormat(" MA.WeekTitle_DW       , "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" MB.WeekTitle_DW_Title , "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" WeekTitle_GUA,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" BibleTitle,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" OnePoint,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" LearningObjectives1, LearningObjectives2, LearningObjectives3, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" BibleTextToRead   , BibleTextToReadFromJang   , BibleTextToReadFromZul   , BibleTextToReadToJang   , BibleTextToReadToZul   , "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" BibleTextToRead_1 , BibleTextToReadFromJang_1 , BibleTextToReadFromZul_1 , BibleTextToReadToJang_1 , BibleTextToReadToZul_1 , "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" BibleTextToRecite, BibleTextToReciteFromJang, BibleTextToReciteFromZul, BibleTextToReciteToJang, BibleTextToReciteToZul,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" YoJulSongURL, YoJulSongText,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" CharcaterMovingURL, CharcaterMovingURLText, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" Charcatercard1Text, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" Charcatercard2Text, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" Charcatercard3Text, "); sb.Append(Environment.NewLine);

            sb.AppendFormat(" ReadAndWriteOneWord, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" PrayText, "); sb.Append(Environment.NewLine);

            sb.AppendFormat(" verseText1, verseText2, verseText3, verseText4, verseText5,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" WelcomeA, WelcomeB, WelcomeTeacherGuide, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" Adventure, AdventureTeacherGuide,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ValueData1, ValueData2, ValueData3, ValueTeacherGuide,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" EncourageStudyPoint1, EncourageStudyPoint2, EncourageTeacherGuide,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ThisWeekMission,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" publishYn      ,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" visited        ,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" relativeImagePath1      ,  "); sb.Append(Environment.NewLine); 
            sb.AppendFormat(" relativeImagePath2      ,  "); sb.Append(Environment.NewLine); 
            sb.AppendFormat(" relativeImagePath3      ,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" DateFrom, DateTo , "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ClickDate_Max, ClickDate_Min , isnull(CountCount, -1) CountCount "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from            [dbo].[WeeklyWordToRead]             MA  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" left outer join [dbo].[WeeklyWordToRead_DanWonTitle] MB on MA.WeekTitle_DW = MB.WeekTitle_DW "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" left outer join ( SELECT [WeeklyWordToReadId], MIN(ClickDate) ClickDate_Min, MAX(ClickDate) ClickDate_Max,COUNT(*) CountCount  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("                   FROM  [dbo].[ClickHistoryForWeeklyWordToRead]  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("                   GROUP BY [WeeklyWordToReadId] ) MC on MA.Id = MC.WeeklyWordToReadId "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1                         ");
            sb.AppendFormat($" and cYear   = {cYear}         ");
            if (WeeklyWordToReadID > 0)
            {
                sb.AppendFormat(" and MA.Id =  {0} ", WeeklyWordToReadID); sb.Append(Environment.NewLine);
            }
            else if (activeData)
            {
                sb.AppendFormat($" and CONVERT(DATE,DATEADD(HOUR,{ConstatsCommon.timeDifference16},'{workDT.ToString("yyyy-MM-dd")}')) between DateFrom and DateTo "); sb.Append(Environment.NewLine);
            }
            DbLogger.LogDebugAsync(sb.ToString());

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

 

        public void CreateWeeklyWordToRead(int WeekTitle_DW, int WeekTitle_GUA, string BibleTitle,  string OnePoint, 
            string LearningObjectives1, string LearningObjectives2, string LearningObjectives3,
            string BibleTextToRead, int BibleTextToReadFromJang, int BibleTextToReadFromZul, int BibleTextToReadToJang, int BibleTextToReadToZul,
            string BibleTextToRead_1, int BibleTextToReadFromJang_1, int BibleTextToReadFromZul_1, int BibleTextToReadToJang_1, int BibleTextToReadToZul_1,
            string BibleTextToRecite, int BibleTextToReciteFromJang, int BibleTextToReciteFromZul, int BibleTextToReciteToJang, 
            int BibleTextToReciteToZul,  
            string YoJulSongURL, string YoJulSongText, 
            string CharcaterMovingURL,string CharcaterMovingURLText,
            string Charcatercard1Text, string Charcatercard2Text, string Charcatercard3Text, 
            string verseText1, string verseText2, string verseText3, string verseText4,  string verseText5, 
            string WelcomeA, string WelcomeB, string WelcomeTeacherGuide, 
            string Adventure, string AdventureTeacherGuide,
            string ValueData1, string ValueData2, string ValueData3, string ValueTeacherGuide,
            string EncourageStudyPoint1, string EncourageStudyPoint2, string EncourageTeacherGuide,
            string ThisWeekMission, DateTime DateFrom, DateTime DateTo)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" insert into [dbo].[WeeklyWordToRead] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ("); sb.Append(Environment.NewLine);
            sb.AppendFormat(" WeekTitle_DW,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" WeekTitle_GUA,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" BibleTitle,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" OnePoint,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" LearningObjectives1, LearningObjectives2, LearningObjectives3, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" BibleTextToRead    , BibleTextToReadFromJang    , BibleTextToReadFromZul    , BibleTextToReadToJang    , BibleTextToReadToZul  ,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" BibleTextToRead_1  , BibleTextToReadFromJang_1  , BibleTextToReadFromZul_1  , BibleTextToReadToJang_1  , BibleTextToReadToZul_1,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" BibleTextToRecite, BibleTextToReciteFromJang, BibleTextToReciteFromZul, BibleTextToReciteToJang, BibleTextToReciteToZul,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" YoJulSongURL, YoJulSongText,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" CharcaterMovingURL, CharcaterMovingURLText,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" Charcatercard1Text, Charcatercard2Text, Charcatercard3Text,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" verseText1, verseText2, verseText3, verseText4, verseText5,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" WelcomeA, WelcomeB, WelcomeTeacherGuide, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" Adventure, AdventureTeacherGuide, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ValueData1, ValueData2, ValueData3,  ValueTeacherGuide,"); sb.Append(Environment.NewLine);
            sb.AppendFormat(" EncourageStudyPoint1, EncourageStudyPoint2, EncourageTeacherGuide, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ThisWeekMission,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" DateFrom, DateTo "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   )"); sb.Append(Environment.NewLine);
            sb.AppendFormat($" values({WeekTitle_DW},{WeekTitle_GUA},'{SharedComm.TrimString(BibleTitle)}','{SharedComm.TrimString(OnePoint)}',");
            sb.AppendFormat($" '{SharedComm.TrimString(LearningObjectives1)}','{SharedComm.TrimString(LearningObjectives2)}','{SharedComm.TrimString(LearningObjectives3)}',");
            sb.AppendFormat($" '{SharedComm.TrimString(BibleTextToRead)}',{BibleTextToReadFromJang},{BibleTextToReadFromZul},{BibleTextToReadToJang},{BibleTextToReadToZul},");
            sb.AppendFormat($" '{SharedComm.TrimString(BibleTextToRead_1)}',{BibleTextToReadFromJang_1},{BibleTextToReadFromZul_1},{BibleTextToReadToJang_1},{BibleTextToReadToZul_1},");
            sb.AppendFormat($" '{SharedComm.TrimString(BibleTextToRecite)}',{BibleTextToReciteFromJang},{BibleTextToReciteFromZul},{BibleTextToReciteToJang},{BibleTextToReciteToZul},");
            sb.AppendFormat($" '{SharedComm.TrimString(YoJulSongURL)}','{SharedComm.TrimString(YoJulSongText)}',");
            sb.AppendFormat($" '{SharedComm.TrimString(CharcaterMovingURL)}',");
            sb.AppendFormat($" '{SharedComm.TrimString(CharcaterMovingURLText)}',");
            sb.AppendFormat($" '{SharedComm.TrimString(Charcatercard1Text)}',");
            sb.AppendFormat($" '{SharedComm.TrimString(Charcatercard2Text)}',");
            sb.AppendFormat($" '{SharedComm.TrimString(Charcatercard3Text)}',");
            sb.AppendFormat($" '{SharedComm.TrimString(verseText1)}','{SharedComm.TrimString(verseText2)}','{SharedComm.TrimString(verseText3)}','{SharedComm.TrimString(verseText4)}','{SharedComm.TrimString(verseText5)}',");
            sb.AppendFormat($" '{SharedComm.TrimString(WelcomeA)}','{SharedComm.TrimString(WelcomeB)}','{SharedComm.TrimString(WelcomeTeacherGuide)}', ");
            sb.AppendFormat($" '{SharedComm.TrimString(Adventure)}','{SharedComm.TrimString(AdventureTeacherGuide)}',");
            sb.AppendFormat($" '{SharedComm.TrimString(ValueData1)}','{SharedComm.TrimString(ValueData2)}','{SharedComm.TrimString(ValueData3)}','{SharedComm.TrimString(ValueTeacherGuide)}',");
            sb.AppendFormat($" '{SharedComm.TrimString(EncourageStudyPoint1)}','{SharedComm.TrimString(EncourageStudyPoint2)}','{SharedComm.TrimString(EncourageTeacherGuide)}',");
            sb.AppendFormat($" '{SharedComm.TrimString(ThisWeekMission)}','{DateFrom.ToString("yyyy-MM-dd")}','{DateTo.ToString("yyyy-MM-dd")}')"); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
 
        public void MakePublishYn(int Id, string publishYn)
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" update  [dbo].[WeeklyWordToRead] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set "); sb.Append(Environment.NewLine);
            
            sb.AppendFormat(" [publishYn] = '{0}'     ,", publishYn); sb.Append(Environment.NewLine);
            sb.AppendFormat($" updateDT    = DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()) "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id]  = {0}       ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void HitWeeklyWordToRead(int Id)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" update [dbo].[WeeklyWordToRead] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set visited = visited + 1 ", Id); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void DecreasetHitWeeklyWordToRead(int Id)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" update [dbo].[WeeklyWordToRead] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set visited = visited - 1 ", Id); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void UpdateWeeklyWordToRead_RelativeImagePath(int Id, string uploadPathSub , string fileNameA, string fileNameB, string fileNameC)
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" update  [dbo].[WeeklyWordToRead] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set "); sb.Append(Environment.NewLine);

            if (!string.IsNullOrEmpty(fileNameA))
            {
                sb.AppendFormat("relativeImagePath1   =  N'{0}', ", $"/{uploadPathSub}/{fileNameA}"); sb.Append(Environment.NewLine);
            }
            else
            {
                sb.AppendFormat("relativeImagePath1   =  null , "); sb.Append(Environment.NewLine);
            }

            if (!string.IsNullOrEmpty(fileNameB))
            {
                sb.AppendFormat("relativeImagePath2   =  N'{0}', ", $"/{uploadPathSub}/{fileNameB}"); sb.Append(Environment.NewLine);
            }
            else
            {
                sb.AppendFormat("relativeImagePath2   =  null , "); sb.Append(Environment.NewLine);
            }

            if (!string.IsNullOrEmpty(fileNameC))
            {
                sb.AppendFormat("relativeImagePath3   =  N'{0}', ", $"/{uploadPathSub}/{fileNameC}"); sb.Append(Environment.NewLine);
            }
            else
            {
                sb.AppendFormat("relativeImagePath3   =  null , "); sb.Append(Environment.NewLine);
            }

            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }


        public void UpdateWeeklyWordToRead(int Id, int WeekTitle_DW, int WeekTitle_GUA, string BibleTitle, string OnePoint,
            string LearningObjectives1, 
            string LearningObjectives2, string LearningObjectives3,

            string BibleTextToRead, int BibleTextToReadFromJang, int BibleTextToReadFromZul, int BibleTextToReadToJang, int BibleTextToReadToZul,
            string BibleTextToRead_1, int BibleTextToReadFromJang_1, int BibleTextToReadFromZul_1, int BibleTextToReadToJang_1, int BibleTextToReadToZul_1,

            string BibleTextToRecite, int BibleTextToReciteFromJang, int BibleTextToReciteFromZul, int BibleTextToReciteToJang, int BibleTextToReciteToZul,
            
            string YoJulSongURL, string YoJulSongText, 
            string CharcaterMovingURL, string CharcaterMovingURLText, 
            string Charcatercard1Text, string Charcatercard2Text, string Charcatercard3Text, 
            string verseText1, string verseText2, string verseText3, string verseText4, string verseText5, 
            string WelcomeA, string WelcomeB, string WelcomeTeacherGuide,
            string Adventure, string AdventureTeacherGuide,
            string ValueData1, string ValueData2, string ValueData3, string ValueTeacherGuide,
            string EncourageStudyPoint1, string EncourageStudyPoint2, string EncourageTeacherGuide, 
            string ThisWeekMission, DateTime DateFrom, DateTime DateTo)
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" update  [dbo].[WeeklyWordToRead] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set "); sb.Append(Environment.NewLine);

            sb.AppendFormat("WeekTitle_DW	          =  '{0}', ", WeekTitle_DW); sb.Append(Environment.NewLine);
            sb.AppendFormat("WeekTitle_GUA	          =  '{0}', ", WeekTitle_GUA); sb.Append(Environment.NewLine); 
            sb.AppendFormat("BibleTitle	              =  '{0}', ", SharedComm.appendSpecialChar(BibleTitle).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("OnePoint	              =  '{0}', ", SharedComm.appendSpecialChar(OnePoint).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("LearningObjectives1	  =  '{0}', ", SharedComm.appendSpecialChar(LearningObjectives1).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("LearningObjectives2	  =  '{0}', ", SharedComm.appendSpecialChar(LearningObjectives2).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("LearningObjectives3	  =  '{0}', ", SharedComm.appendSpecialChar(LearningObjectives3).Trim()); sb.Append(Environment.NewLine);

            sb.AppendFormat("BibleTextToRead          =  '{0}', ", SharedComm.appendSpecialChar(BibleTextToRead).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("BibleTextToReadFromJang =   {0} , ", BibleTextToReadFromJang); sb.Append(Environment.NewLine);
            sb.AppendFormat("BibleTextToReadFromZul   =   {0} , ", BibleTextToReadFromZul); sb.Append(Environment.NewLine);
            sb.AppendFormat("BibleTextToReadToJang    =   {0} , ", BibleTextToReadToJang); sb.Append(Environment.NewLine);
            sb.AppendFormat("BibleTextToReadToZul	  =   {0} , ", BibleTextToReadToZul); sb.Append(Environment.NewLine);

            sb.AppendFormat("BibleTextToRead_1          =  '{0}', ", SharedComm.appendSpecialChar(BibleTextToRead_1).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("BibleTextToReadFromJang_1 =   {0} , ", BibleTextToReadFromJang_1); sb.Append(Environment.NewLine);
            sb.AppendFormat("BibleTextToReadFromZul_1  =   {0} , ", BibleTextToReadFromZul_1); sb.Append(Environment.NewLine);
            sb.AppendFormat("BibleTextToReadToJang_1   =   {0} , ", BibleTextToReadToJang_1); sb.Append(Environment.NewLine);
            sb.AppendFormat("BibleTextToReadToZul_1	   =   {0} , ", BibleTextToReadToZul_1); sb.Append(Environment.NewLine);

            sb.AppendFormat("BibleTextToRecite        =  '{0}', ", SharedComm.appendSpecialChar(BibleTextToRecite).Trim()); sb.Append(Environment.NewLine);        
            sb.AppendFormat("BibleTextToReciteFromJang =  {0} , ", BibleTextToReciteFromJang); sb.Append(Environment.NewLine);
            sb.AppendFormat("BibleTextToReciteFromZul  =  {0} , ", BibleTextToReciteFromZul); sb.Append(Environment.NewLine);
            sb.AppendFormat("BibleTextToReciteToJang   =  {0} , ", BibleTextToReciteToJang); sb.Append(Environment.NewLine);
            sb.AppendFormat("BibleTextToReciteToZul	   =  {0} , ", BibleTextToReciteToZul); sb.Append(Environment.NewLine);
            sb.AppendFormat("YoJulSongURL  =  '{0}', ", SharedComm.appendSpecialChar(YoJulSongURL).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("YoJulSongText =  '{0}', ", SharedComm.appendSpecialChar(YoJulSongText).Trim()); sb.Append(Environment.NewLine); 
            sb.AppendFormat("CharcaterMovingURL  =  '{0}', ", SharedComm.appendSpecialChar(CharcaterMovingURL).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("CharcaterMovingURLText  =  '{0}', ", SharedComm.appendSpecialChar(CharcaterMovingURLText).Trim()); sb.Append(Environment.NewLine);

            sb.AppendFormat("Charcatercard1Text	   =  '{0}', ", SharedComm.appendSpecialChar(Charcatercard1Text).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("Charcatercard2Text	   =  '{0}', ", SharedComm.appendSpecialChar(Charcatercard2Text).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("Charcatercard3Text	   =  '{0}', ", SharedComm.appendSpecialChar(Charcatercard3Text).Trim()); sb.Append(Environment.NewLine);

            sb.AppendFormat("verseText1	   =  '{0}', ", SharedComm.appendSpecialChar(verseText1).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("verseText2	   =  '{0}', ", SharedComm.appendSpecialChar(verseText2).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("verseText3	   =  '{0}', ", SharedComm.appendSpecialChar(verseText3).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("verseText4	   =  '{0}', ", SharedComm.appendSpecialChar(verseText4).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("verseText5	   =  '{0}', ", SharedComm.appendSpecialChar(verseText5).Trim()); sb.Append(Environment.NewLine);
         
            sb.AppendFormat("WelcomeA  	  =  '{0}', ", SharedComm.appendSpecialChar(WelcomeA).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("WelcomeB	  =  '{0}', ", SharedComm.appendSpecialChar(WelcomeB).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("WelcomeTeacherGuide	  =  '{0}', ", SharedComm.appendSpecialChar(WelcomeTeacherGuide).Trim()); sb.Append(Environment.NewLine);
        
            sb.AppendFormat("Adventure	           =  '{0}', ", SharedComm.appendSpecialChar(Adventure).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("AdventureTeacherGuide =  '{0}', ", SharedComm.appendSpecialChar(AdventureTeacherGuide).Trim()); sb.Append(Environment.NewLine);

            sb.AppendFormat("ValueData1	  =  '{0}', ", SharedComm.appendSpecialChar(ValueData1).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("ValueData2   =  '{0}', ", SharedComm.appendSpecialChar(ValueData2).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("ValueData3	  =  '{0}', ", SharedComm.appendSpecialChar(ValueData3).Trim()); sb.Append(Environment.NewLine);          
            sb.AppendFormat("ValueTeacherGuide	  =  '{0}', ", SharedComm.appendSpecialChar(ValueTeacherGuide).Trim()); sb.Append(Environment.NewLine);
          
            sb.AppendFormat("EncourageStudyPoint1  =  '{0}', ", SharedComm.appendSpecialChar(EncourageStudyPoint1).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("EncourageStudyPoint2  =  '{0}', ", SharedComm.appendSpecialChar(EncourageStudyPoint2).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("EncourageTeacherGuide =  '{0}', ", SharedComm.appendSpecialChar(EncourageTeacherGuide).Trim()); sb.Append(Environment.NewLine);

            sb.AppendFormat("ThisWeekMission =  '{0}', ", SharedComm.appendSpecialChar(ThisWeekMission).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat(" DateFrom       =  '{0}', ", DateFrom.ToString("yyyy-MM-dd")); sb.Append(Environment.NewLine);
            sb.AppendFormat(" DateTo         =  '{0}'  ", DateTo.ToString("yyyy-MM-dd")); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void DeleteWeeklyWordToRead(int Id)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" delete from [dbo].[WeeklyWordToRead] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
    }
}
