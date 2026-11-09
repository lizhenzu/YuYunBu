/* Original definitions before SQL/RemoveDept_20261109.sql (for rollback reference). Run a block to restore that object. */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- ===== GetPointByDate
IF OBJECT_ID(N'dbo.GetPointByDate') IS NOT NULL DROP FUNCTION dbo.GetPointByDate;
GO
CREATE  FUNCTION [dbo].[GetPointByDate] (@AddHours int,@AddDays int)
RETURNS TABLE
AS
RETURN
(
  
 select   DC , userName  , [LastName], [FirstName] , ClassYear, ClassBan  , DeptCode , RoleCode
   from (   
   		select data.userName, DC , [LastName], [FirstName] , isnull(ClassYear,0) ClassYear, isnull(ClassBan,0) ClassBan ,RoleCode,DeptCode
  	     	from ( 
                  select userName, count(*) DC  
  		           from ( 
  						  select * from [dbo].[view_click_data_all_person]  where   [clickDate] >=  DATEADD(HOUR,@AddHours, DATEADD(Day,@AddDays, GETDATE()))                    
  					     ) data  
  			       group by userName  
  		         ) data 
  		  join view_UserCompositeInfo US on  data.userName = US.userName 
       ) data  


);
GO

-- ===== GetThisWeekRewarding____
IF OBJECT_ID(N'dbo.GetThisWeekRewarding____') IS NOT NULL DROP FUNCTION dbo.GetThisWeekRewarding____;
GO
CREATE FUNCTION [dbo].[GetThisWeekRewarding](@deptCode nvarchar(50))
RETURNS TABLE
AS
RETURN
(
    SELECT 
        ROW_NUMBER() OVER(ORDER BY DC DESC) AS sequenceNo,
        *,
        '' AS ProfilePicturePath
    FROM (
        SELECT 
            (DC + ISNULL(SDC, 0) - ISNULL(AD.AwardLevel, 0)) AS DC,
            (DC + ISNULL(SDC, 0)) AS DC_Total,
            MA.userName,
            MA.LastName,
            MA.FirstName,
            MA.FullName,
            MA.ClassYear,
            MA.ClassBan,
            MA.RoleNM,
            MA.DeptNM,
            MA.RoleCode,
            MA.DeptCode,
            minclickDate,
            maxclickDate,
            ISNULL(AD.AwardLevel, 0) AS AwardLevel,
            MA.UserId,
            0 AS AwardLevel1000Value
        FROM [dbo].view_Ranking_By_Person MA
        LEFT JOIN [dbo].[awardData] AD ON MA.userName = AD.userName
        left outer join 
	  (
			 SELECT 
				username AS userName,
				SUM(point) AS SDC,
				MIN(clickDate) AS A,
				MAX(clickDate) AS B
			FROM ClickHistoryForPoint
			GROUP BY username
	  )
	   UAP1 on MA.userName = UAP1.userName  

        WHERE 
            DeptCode = @deptCode
    ) data
    WHERE 
        (AwardLevel = 0   AND DC_Total >= 100)
        OR (AwardLevel = 100 AND DC_Total >= 400)
        OR (AwardLevel = 400 AND DC_Total >= 700)
        OR (AwardLevel = 700 AND DC_Total >= 1000)
);
GO

-- ===== GetThisWeekRewardingP
IF OBJECT_ID(N'dbo.GetThisWeekRewardingP') IS NOT NULL DROP FUNCTION dbo.GetThisWeekRewardingP;
GO
CREATE FUNCTION [dbo].[GetThisWeekRewardingP](@deptCode nvarchar(50))
RETURNS TABLE
AS
RETURN
(
    SELECT 
        ROW_NUMBER() OVER(ORDER BY DC DESC) AS sequenceNo,
        *,
        '' AS ProfilePicturePath
    FROM (
        SELECT 
            DC,
            DC_Total,
            (DC_Total + SDC) Total,
			SDC,
            MA.userName,
            MA.LastName,
            MA.FirstName,
            MA.FullName,
            MA.ClassYear,
            MA.ClassBan,
            MA.RoleNM,
            MA.DeptNM,
            MA.RoleCode,
            MA.DeptCode,
            minclickDate,
            maxclickDate,
            AwardLevel,
            MA.UserId,
            0 AS AwardLevel1000Value
        FROM [dbo].view_Ranking_By_Person_ext MA

        
            WHERE (@deptCode = '' OR @deptCode IS NULL OR DeptCode = @deptCode)
    ) data
    WHERE 
        (AwardLevel = 0   AND (DC_Total +SDC) >= 100)
        OR (AwardLevel = 100 AND (DC_Total +SDC) >= 400)
        OR (AwardLevel = 400 AND (DC_Total +SDC) >= 700)
        OR (AwardLevel = 700 AND (DC_Total +SDC) >= 1000)
        OR (AwardLevel = 1000 AND (DC_Total +SDC) >= 1500)
);
GO

-- ===== view_All_Person_2026
IF OBJECT_ID(N'dbo.view_All_Person_2026') IS NOT NULL DROP VIEW dbo.view_All_Person_2026;
GO
CREATE view [dbo].[view_All_Person_2026] as 
select 'D1001' DeptCode ,	'T1001' RoleCode, b.userid,  a.personid , fullName from [dbo].[ClassInfo_2026]  A join [dbo].[view_PersonMaster_2026] b on a.personid = b.personid
union
select 'D1001' DeptCode ,	'T1002' RoleCode, b.userid,  b.personid , fullName from ClassTeacherMap_2026   A join [dbo].[view_PersonMaster_2026] b on a.Teacherpersonid = b.personid
union
select 'D1001' DeptCode ,	'T1002' RoleCode, b.userid,  b.personid , fullName from ClassTeacherMap_2026   A join [dbo].[view_PersonMaster_2026] b on a.SubTeacherPersonId = b.personid
union
select  [DeptCode], RoleCode , b.userid,  a.personid , fullName                from [DeptTeacherMap_2026]   A join [dbo].[view_PersonMaster_2026] b on a.personid = b.personid
GO

-- ===== view_all_point_info
IF OBJECT_ID(N'dbo.view_all_point_info') IS NOT NULL DROP VIEW dbo.view_all_point_info;
GO
create view [dbo].[view_all_point_info] as

  select  *  
   from   
   (  
   select  MA.UserId, MA.userName,  ((DC_Total + SDC) - isnull(AD.AwardLevel, 0)) DC ,  (DC_Total + SDC) DC_Total, SDC ,isnull(AD.AwardLevel, 0) AwardLevel , MA.[LastName], MA.[FirstName] , MA.FullName  ,MA.ClassYear, MA.ClassBan, RoleCode, DeptCode ,RoleNM, DeptNM , 
               [minclickDate], [maxclickDate] , isnull(AP.ProductID, 0) ProductID, isnull(AP.PickID, 0) PickID
      from [dbo].[view_Ranking_By_Person_ext]  MA   
      left outer join [dbo].[awardData]     AD on MA.userName = AD.userName  
      left outer join [dbo].[AwardItemPick] AP on MA.UserId   = AP.UserId  and  MA.UserId   = AP.UserId
      where 1 = 1   
    ) data
GO

-- ===== view_Ranking_By_Person
IF OBJECT_ID(N'dbo.view_Ranking_By_Person') IS NOT NULL DROP VIEW dbo.view_Ranking_By_Person;
GO
create view [dbo].[view_Ranking_By_Person] as
 
 select   DC , userName  , [LastName], [FirstName] , FullName, RoleCode, RoleNM, DeptCode, DeptNM, ClassYear, ClassBan  ,minclickDate , maxclickDate , UserId
   from (   
   		select data.userName, TotalDC DC , [LastName], [FirstName] , FullName , isnull(RoleCode,'') RoleCode ,isnull(RoleNM,'') RoleNM ,isnull(DeptCode,'') DeptCode ,isnull(DeptNM,'') DeptNM , isnull(ClassYear,0) ClassYear, isnull(ClassBan,0) ClassBan , FirstClickDate minclickDate ,  LastClickDate maxclickDate , UserId
  	     	from view_Total_Summary_Point data 
  		  join [dbo].[view_UserCompositeInfo_2026] US on  data.userName = US.userName 
  		   
       ) data
GO

-- ===== view_Ranking_By_Person_ext
IF OBJECT_ID(N'dbo.view_Ranking_By_Person_ext') IS NOT NULL DROP VIEW dbo.view_Ranking_By_Person_ext;
GO
create view [dbo].[view_Ranking_By_Person_ext] as

   select  UserId, MA.userName , MA.FullName,   (DC - isnull(AD.AwardLevel, 0)) DC ,  DC DC_Total, (isnull(Total, 0)  + isnull(SDC,0)) Total , isnull(SDC,0) SDC, MA.ClassYear, MA.ClassBan, RoleCode, DeptCode ,RoleNM, DeptNM,isnull(AD.AwardLevel, 0) AwardLevel, isnull(AwardLevel1000Value, 0) AwardLevel1000Value ,
               [minclickDate], [maxclickDate] , MA.[LastName], MA.[FirstName]
      from [dbo].view_Ranking_By_Person  MA   
      left outer join [dbo].[awardData] AD on MA.userName = AD.userName  
      left outer join [dbo].[awardDataUnit1000] AD1000 on MA.UserId = AD1000.Award1000UserId  
      left outer join dbo.ufn_UserAreaPointsByDate(NULL, NULL) UAP on MA.userName = UAP.userName  
      left outer join 
	  (
			 SELECT 
				username AS userName,
				SUM(point) AS SDC,
				MIN(clickDate) AS A,
				MAX(clickDate) AS B
			FROM ClickHistoryForPoint
			GROUP BY username
	  )
	   UAP1 on MA.userName = UAP1.userName  
	  where  MA.userName not like '%test%'
GO

-- ===== view_UserCompositeInfo_2025
IF OBJECT_ID(N'dbo.view_UserCompositeInfo_2025') IS NOT NULL DROP VIEW dbo.view_UserCompositeInfo_2025;
GO
create view [dbo].[view_UserCompositeInfo] as

select
 IDA.Id UserId,
 IDA.LastName , IDA.FirstName, (IDA.LastName + IDA.FirstName) FullName, IDA.UserName , isnull(bMonth,'') bMonth, isnull(bDay,'') bDay ,
 isnull([birthDay_PublishYN],'') [birthDay_PublishYN] ,
  IDA.email, isnull(PM.PhoneNumber,'') PhoneNumber ,
 isnull(PM.PhoneNumber2,'') PhoneNumber2 ,
 isnull(PM.HomeAaddress,'') HomeAaddress ,
 isnull(PM.school,'') school ,
 isnull(PM.sex   ,'') sex ,
 isnull([readBibleAllCount],0) [readBibleAllCount],
 isnull(ClassYear, 0)   ClassYear ,
 isnull(ClassBan , 0)   ClassBan  ,
 isnull(PersonId , 0)   PersonId  , 
 isnull(DeptCode , '')   DeptCode , 
 isnull(RoleCode , '')   RoleCode , 
  (select MinorNM  from [dbo].[CodeData] where MajorCD = 'D001' and  MinorCD = DeptCode) DeptNM,
  (select MinorNM  from [dbo].[CodeData] where MajorCD = 'T001' and  MinorCD = RoleCode) RoleNM, 
 ''                     ProfilePicture  
from            [Identity].[User]         IDA
left outer join [dbo].ClassInfo           CI    on IDA.Id = CI.UserId
left outer join [dbo].[view_PersonMaster_2026]      PM    on IDA.Id = PM.UserId
left outer join [dbo].[DeptRoleUserAssoc] DRUA  on IDA.Id = DRUA.UserId
GO

-- ===== view_UserCompositeInfo_2026
IF OBJECT_ID(N'dbo.view_UserCompositeInfo_2026') IS NOT NULL DROP VIEW dbo.view_UserCompositeInfo_2026;
GO
CREATE VIEW [dbo].[view_UserCompositeInfo_2026]  as 

SELECT
    IDA.UserId                         AS UserId,
    LastName,
    FirstName,
    IDA.FullName,
    UserName,
    ISNULL(bMonth,'')                  AS bMonth,
    ISNULL(bDay,'')                    AS bDay,
    ISNULL([birthDay_PublishYN],'')    AS [birthDay_PublishYN],
    email,
    ISNULL(PhoneNumber,'')             AS PhoneNumber,
    ISNULL(PhoneNumber2,'')            AS PhoneNumber2,
    ISNULL(HomeAaddress,'')            AS HomeAaddress,
    ISNULL(school,'')                  AS school,
    ISNULL(sex,'')                     AS sex,
    ISNULL([readBibleAllCount],0)      AS [readBibleAllCount],

    -- ✅ isnull 제거 (원본 값 그대로)
    ClassYear                          AS ClassYear,
    ClassBan                           AS ClassBan,

    ISNULL(IDA.PersonId, 0)            AS PersonId,
 isnull(DeptCode , '')   DeptCode , 
 isnull(RoleCode , '')   RoleCode , 
  (select MinorNM  from [dbo].[CodeData] where MajorCD = 'D001' and  MinorCD = DeptCode) DeptNM,
  (select MinorNM  from [dbo].[CodeData] where MajorCD = 'T001' and  MinorCD = RoleCode) RoleNM, 

    ''                                 AS ProfilePicture
FROM            [dbo].[view_PersonMaster_2026] IDA
LEFT OUTER join [dbo].[view_All_Person_2026] DRUA  on IDA.PersonId = DRUA.PersonId
LEFT OUTER JOIN [dbo].[ClassInfo_2026]      CI   ON IDA.PersonId = CI.PersonId
GO

-- ===== view_UserCompositeInfo_2026_N
IF OBJECT_ID(N'dbo.view_UserCompositeInfo_2026_N') IS NOT NULL DROP VIEW dbo.view_UserCompositeInfo_2026_N;
GO
CREATE VIEW [dbo].[view_UserCompositeInfo_2026] AS
SELECT
    IDA.UserId                         AS UserId,
    LastName,
    FirstName,
    FullName,
    UserName,
    ISNULL(bMonth,'')                  AS bMonth,
    ISNULL(bDay,'')                    AS bDay,
    ISNULL([birthDay_PublishYN],'')    AS [birthDay_PublishYN],
    email,
    ISNULL(PhoneNumber,'')             AS PhoneNumber,
    ISNULL(PhoneNumber2,'')            AS PhoneNumber2,
    ISNULL(HomeAaddress,'')            AS HomeAaddress,
    ISNULL(school,'')                  AS school,
    ISNULL(sex,'')                     AS sex,
    ISNULL([readBibleAllCount],0)      AS [readBibleAllCount],

    -- ✅ isnull 제거 (원본 값 그대로)
    ClassYear                          AS ClassYear,
    ClassBan                           AS ClassBan,

    ISNULL(IDA.PersonId, 0)            AS PersonId,
    ''               AS DeptCode,
    ''             AS RoleCode,

    -- ✅ ClassYear 기준 DeptNM 로직
    CASE
        WHEN ClassYear BETWEEN 1 AND 3 THEN N'유년부'
        WHEN ClassYear BETWEEN 4 AND 5 THEN N'초등부'
        ELSE N''
    END                                AS DeptNM,

    -- ✅ ClassYear 기준 RoleNM 로직
    CASE
        WHEN ClassYear BETWEEN 1 AND 6 THEN N'학생'
        ELSE N''
    END                                AS RoleNM,

    ''                                 AS ProfilePicture
FROM [dbo].[view_PersonMaster_2026] IDA
LEFT OUTER JOIN [dbo].[ClassInfo_2026]      CI   ON IDA.PersonId = CI.PersonId
GO

-- ===== view_UserCompositeInfo_2026old
IF OBJECT_ID(N'dbo.view_UserCompositeInfo_2026old') IS NOT NULL DROP VIEW dbo.view_UserCompositeInfo_2026old;
GO
CREATE VIEW [dbo].[view_UserCompositeInfo_2026] AS
SELECT
    IDA.UserId                         AS UserId,
    LastName,
    FirstName,
    FullName,
    UserName,
    ISNULL(bMonth,'')                  AS bMonth,
    ISNULL(bDay,'')                    AS bDay,
    ISNULL([birthDay_PublishYN],'')    AS [birthDay_PublishYN],
    email,
    ISNULL(PhoneNumber,'')             AS PhoneNumber,
    ISNULL(PhoneNumber2,'')            AS PhoneNumber2,
    ISNULL(HomeAaddress,'')            AS HomeAaddress,
    ISNULL(school,'')                  AS school,
    ISNULL(sex,'')                     AS sex,
    ISNULL([readBibleAllCount],0)      AS [readBibleAllCount],

    -- ✅ isnull 제거 (원본 값 그대로)
    ClassYear                          AS ClassYear,
    ClassBan                           AS ClassBan,

    ISNULL(IDA.PersonId, 0)            AS PersonId,
    ''               AS DeptCode,
    ''             AS RoleCode,

    -- ✅ ClassYear 기준 DeptNM 로직
    CASE
        WHEN ClassYear BETWEEN 1 AND 3 THEN N'유년부'
        WHEN ClassYear BETWEEN 4 AND 5 THEN N'초등부'
        ELSE N''
    END                                AS DeptNM,

    -- ✅ ClassYear 기준 RoleNM 로직
    CASE
        WHEN ClassYear BETWEEN 1 AND 6 THEN N'학생'
        ELSE N''
    END                                AS RoleNM,

    ''                                 AS ProfilePicture
FROM [dbo].[view_PersonMaster_2026] IDA
LEFT OUTER JOIN [dbo].[ClassInfo_2026]      CI   ON IDA.PersonId = CI.PersonId
GO

-- ===== GetThisWeekRewarding2__ (depends on GetThisWeekRewarding____, restore that first)
IF OBJECT_ID(N'dbo.GetThisWeekRewarding2__') IS NOT NULL DROP FUNCTION dbo.GetThisWeekRewarding2__;
GO
CREATE FUNCTION [dbo].[GetThisWeekRewarding2]()
RETURNS TABLE
AS
RETURN
(
   select   ROW_NUMBER() OVER(ORDER BY DC desc) sequenceNo, *  
   from   
   (  
   select   (DC - isnull(AD.AwardLevel, 0)) DC ,  DC DC_Total, MA.userName  , MA.[LastName], MA.[FirstName] , MA.ClassYear, MA.ClassBan  ,  [minclickDate], [maxclickDate] ,isnull(AD.AwardLevel, 0) AwardLevel  
      from [dbo].view_Ranking_By_Person  MA   
      left outer join [dbo].[awardData] AD on MA.userName = AD.userName  
      where 1 = 1   
   and ClassYear between 1 and 3 
    ) data   

	where  AwardLevel in (100, 300, 500, 700)
	and   DC between 100 and 199

UNION ALL

select  ROW_NUMBER() OVER(ORDER BY DC desc) sequenceNo, *  
   from   
   (  
   select   (DC - isnull(AD.AwardLevel, 0)) DC ,  DC DC_Total, MA.userName  , MA.[LastName], MA.[FirstName] , MA.ClassYear, MA.ClassBan  ,  [minclickDate], [maxclickDate] ,isnull(AD.AwardLevel, 0) AwardLevel  
      from [dbo].view_Ranking_By_Person  MA   
      left outer join [dbo].[awardData] AD on MA.userName = AD.userName  
      where 1 = 1   
   and ClassYear between 1 and 3 
    ) data   

	where  AwardLevel = 0
	and   DC between 100 and 199
);
GO
