/* Old definition of view_UserCompositeInfo_2026 (before RoleSingleSource_20261109.sql) - for rollback */
ALTER VIEW [dbo].[view_UserCompositeInfo_2026] AS
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
    ClassYear                          AS ClassYear,
    ClassBan                           AS ClassBan,
    ISNULL(IDA.PersonId, 0)            AS PersonId,
    ISNULL(RoleCode, '')               AS RoleCode,
    (SELECT MinorNM FROM [dbo].[CodeData] WHERE MajorCD = 'T001' AND MinorCD = RoleCode) AS RoleNM,
    ''                                 AS ProfilePicture
FROM            [dbo].[view_PersonMaster_2026] IDA
LEFT OUTER JOIN [dbo].[view_All_Person_2026]  DRUA ON IDA.PersonId = DRUA.PersonId
LEFT OUTER JOIN [dbo].[ClassInfo_2026]        CI   ON IDA.PersonId = CI.PersonId
GO
