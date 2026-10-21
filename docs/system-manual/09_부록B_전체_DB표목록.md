# 09. 부록 B — 전체 DB 표 목록

> 운영 DB 구조(2026-10-21). 행 수 · 기본 키 · 열(형식, `NN` = NOT NULL). 이름에 `_bak`, `_delete`, 날짜가 붙은 표는 백업이다.


## 표 (291)

| 표 | 행 | 기본 키 | 열 |
|---|---:|---|---|
| `dbo.__EFMigrationsHistory` | 16 | MigrationId | MigrationId nvarchar(150) NN; ProductVersion nvarchar(32) NN |
| `dbo._bak_TodayWordToRead_Synopsis_20261017` | 31 |  | Id int IDENTITY NN; ExposeDate datetime NN; Synopsis nvarchar(max) |
| `dbo._bak_TodayWordToRead_Synopsis_202611_20261017` | 30 |  | Id int IDENTITY NN; ExposeDate datetime NN; Synopsis nvarchar(max) |
| `dbo._bak_TodayWordToRead_Synopsis_5s_20261017` | 61 |  | Id int IDENTITY NN; ExposeDate datetime NN; Synopsis nvarchar(max) |
| `dbo.aaa` | 235 |  | UserId nvarchar(450); FullName nvarchar(max); TotalPoints int NN |
| `dbo.abbreviationsBible` | 66 |  | Id int IDENTITY NN; abbrWord nvarchar(20) NN; fullWord nvarchar(20) NN; InsertDT datetime NN; updateDT datetime NN |
| `dbo.AIAskHistory` | 84 |  | Id int IDENTITY NN; WhatAsk nvarchar(450) NN; UserName nvarchar(450) NN; InsertDT datetime NN; responseData nvarchar(max); durationTime float |
| `dbo.AIPromptWord` | 9 |  | Id int IDENTITY NN; PromptText nvarchar(max); InsertDT datetime NN; updateDT datetime NN |
| `dbo.Albums` | 54 | Id | Id int IDENTITY NN; Name nvarchar(200) NN; Description nvarchar(1000); CreatedAt datetime2 NN; CoverPhotoId int; UserName nvarchar(100) |
| `dbo.Answers` | 5 | Id | Id int IDENTITY NN; QuestionId int NN; ParticipantId int NN; RawText nvarchar(max); OptionIds nvarchar(max); ScaleValue int; IsRemoved bit NN; SubmittedAt datetime2 NN |
| `dbo.AnswerTokens` | 5 | Id | Id int IDENTITY NN; AnswerId int NN; QuestionId int NN; NormalizedText nvarchar(100) NN; DisplayText nvarchar(100) NN |
| `dbo.AspNetRoleClaims` | 0 | Id | Id int IDENTITY NN; RoleId nvarchar(450) NN; ClaimType nvarchar(max); ClaimValue nvarchar(max) |
| `dbo.AspNetRoles` | 4 | Id | Id nvarchar(450) NN; Name nvarchar(256); NormalizedName nvarchar(256); ConcurrencyStamp nvarchar(max) |
| `dbo.AspNetUserClaims` | 0 | Id | Id int IDENTITY NN; UserId nvarchar(450) NN; ClaimType nvarchar(max); ClaimValue nvarchar(max) |
| `dbo.AspNetUserLogins` | 0 | LoginProvider,ProviderKey | LoginProvider nvarchar(450) NN; ProviderKey nvarchar(450) NN; ProviderDisplayName nvarchar(max); UserId nvarchar(450) NN |
| `dbo.AspNetUserRoles` | 1 | UserId,RoleId | UserId nvarchar(450) NN; RoleId nvarchar(450) NN |
| `dbo.AspNetUsers` | 1 | Id | Id nvarchar(450) NN; FullName nvarchar(max) NN; ChurchId int; DepartmentId int; IsActive bit NN; CreatedAt datetime2 NN; UserName nvarchar(256); NormalizedUserName nvarchar(256); Email nvarchar(256); NormalizedEmail nvarchar(256); EmailConfirmed bit NN; PasswordHash nvarchar(max); SecurityStamp nvarchar(max); ConcurrencyStamp nvarchar(max); PhoneNumber nvarchar(max); PhoneNumberConfirmed bit NN; TwoFactorEnabled bit NN; LockoutEnd datetimeoffset; LockoutEnabled bit NN; AccessFailedCount int NN |
| `dbo.AspNetUserTokens` | 0 | UserId,LoginProvider,Name | UserId nvarchar(450) NN; LoginProvider nvarchar(450) NN; Name nvarchar(450) NN; Value nvarchar(max) |
| `dbo.AttendanceCheck_2025` | 449 | AttendanceCheckId | AttendanceCheckId bigint IDENTITY NN; AttendDate date NN; UserId nvarchar(450) NN; IsPresent bit NN; UpdatedAt datetime2 NN; UpdatedBy nvarchar(100) |
| `dbo.AttendanceCheck_2026` | 2035 | AttendanceCheckId | AttendanceCheckId bigint IDENTITY NN; AttendDate date NN; PersonId int NN; IsPresent bit NN; UpdatedAt datetime2 NN; UpdatedBy nvarchar(100) |
| `dbo.AttendanceNewComer` | 10 | Id | Id int IDENTITY NN; ChildName nvarchar(100) NN; FirstVisitDate date NN; ParentName nvarchar(100) NN; Phone nvarchar(50) NN; BirthDate date; School nvarchar(200); IsDeleted bit NN; InsertDT datetime NN; UpdateDT datetime NN; PersonId int; PhotoPath nvarchar(500); IsVoluntary bit NN; EvangelistName nvarchar(100) |
| `dbo.AttendanceNewComerVisit` | 95 | Id | Id int IDENTITY NN; NewComerId int NN; VisitDate date NN; InsertDT datetime NN; UpdateDT datetime NN |
| `dbo.AwardBase` | 3 |  | Id int IDENTITY NN; AwardLevel int NN; AmountOfMoney int NN; UpdateDT datetime NN; InsertDT datetime NN |
| `dbo.awardData` | 0 |  | AwardLevel int; userName nvarchar(256); LastName nvarchar(max); FirstName nvarchar(max) |
| `dbo.awardData_` | 37 |  | AwardLevel int; userName nvarchar(256); LastName nvarchar(max); FirstName nvarchar(max) |
| `dbo.awardDataUnit1000` | 0 |  | Award1000UserId nvarchar(450) NN; AwardLevel1000Value int NN; InsertDT datetime NN; updateDT datetime NN |
| `dbo.AwardItem` | 243 | ProductID | ProductID int IDENTITY NN; ProductName nvarchar(100) NN; ProductDescription nvarchar(500); UnitPrice int NN; CreatedDate datetime; ModifiedDate datetime; SourceType nvarchar(100); IsHidden bit NN; ExternalUrl nvarchar(1000) |
| `dbo.AwardItemPick` | 1 | PickID | PickID int IDENTITY NN; ProductID int NN; UserId nvarchar(450) NN; InsertDT datetime; UpdateDT datetime |
| `dbo.AwardUserClaim` | 1 | Id | Id int IDENTITY NN; UserName nvarchar(256) NN; cYear int NN; AwardLevel int NN; AwardBaseId int; Status nvarchar(20) NN; SelectedProductID int; SelectedDT datetime; InsertDT datetime NN; UpdateDT datetime NN |
| `dbo.AwardUserDelivery` | 0 | DeliveryId | DeliveryId bigint IDENTITY NN; UserName nvarchar(100) NN; cYear int NN; AwardLevel int NN; DeliveredYn char(1) NN; DeliveredAt datetime; DeliveredBy nvarchar(100); Memo nvarchar(200); CreatedAt datetime NN |
| `dbo.AwardUserSnooze` | 0 | UserName,cYear | UserName nvarchar(256) NN; cYear int NN; SnoozeUntilDT datetime NN; InsertDT datetime NN; UpdateDT datetime NN |
| `dbo.BandAttendancePostComments` | 0 | Id | Id int IDENTITY NN; BandPostId int NN; WriterName nvarchar(50); Content nvarchar(1000) NN; CreatedAt datetime2 NN |
| `dbo.BandAttendancePostImages` | 5 | Id | Id int IDENTITY NN; BandPostId int NN; ImagePath nvarchar(300) NN |
| `dbo.BandAttendancePosts` | 5 | Id | Id int IDENTITY NN; WriterName nvarchar(50); Content nvarchar(max); LikeCount int NN; CreatedAt datetime2 NN |
| `dbo.BandJuboPostComments` | 0 | Id | Id int IDENTITY NN; BandPostId int NN; WriterName nvarchar(50); Content nvarchar(1000) NN; CreatedAt datetime2 NN |
| `dbo.BandJuboPostImages` | 90 | Id | Id int IDENTITY NN; BandPostId int NN; ImagePath nvarchar(300) NN |
| `dbo.BandJuboPosts` | 46 | Id | Id int IDENTITY NN; WriterName nvarchar(50); Content nvarchar(max); LikeCount int NN; CreatedAt datetime2 NN |
| `dbo.BandPostComments` | 1 | Id | Id int IDENTITY NN; BandPostId int NN; WriterName nvarchar(50); Content nvarchar(1000) NN; CreatedAt datetime2 NN |
| `dbo.BandPostImages` | 0 | Id | Id int IDENTITY NN; BandPostId int NN; ImagePath nvarchar(300) NN |
| `dbo.BandPosts` | 1 | Id | Id int IDENTITY NN; WriterName nvarchar(50); Content nvarchar(max); LikeCount int NN; CreatedAt datetime2 NN |
| `dbo.BannedWords` | 0 | Id | Id int IDENTITY NN; Word nvarchar(100) NN; Severity int NN |
| `dbo.BessingChild` | 32 |  | Id int IDENTITY NN; FromDate nvarchar(10); ToDate nvarchar(10); PrayTitle nvarchar(100); PrayText nvarchar(max); visited int NN; InsertDT datetime NN; updateDT datetime NN; HolyWord nvarchar(max); PrayText_Eng nvarchar(max) |
| `dbo.BessingChild_bak_v1` | 32 |  | Id int IDENTITY NN; FromDate nvarchar(10); ToDate nvarchar(10); PrayTitle nvarchar(100); PrayText nvarchar(max); visited int NN; InsertDT datetime NN; updateDT datetime NN; HolyWord nvarchar(max); PrayText_Eng nvarchar(max) |
| `dbo.BessingChild_bak_v2` | 32 |  | Id int IDENTITY NN; FromDate nvarchar(10); ToDate nvarchar(10); PrayTitle nvarchar(100); PrayText nvarchar(max); visited int NN; InsertDT datetime NN; updateDT datetime NN; HolyWord nvarchar(max); PrayText_Eng nvarchar(max) |
| `dbo.BessingChild_bak_v3` | 32 |  | Id int IDENTITY NN; FromDate nvarchar(10); ToDate nvarchar(10); PrayTitle nvarchar(100); PrayText nvarchar(max); visited int NN; InsertDT datetime NN; updateDT datetime NN; HolyWord nvarchar(max); PrayText_Eng nvarchar(max) |
| `dbo.BessingChild_bak_v4` | 32 |  | Id int IDENTITY NN; FromDate nvarchar(10); ToDate nvarchar(10); PrayTitle nvarchar(100); PrayText nvarchar(max); visited int NN; InsertDT datetime NN; updateDT datetime NN; HolyWord nvarchar(max); PrayText_Eng nvarchar(max) |
| `dbo.BessingChildA` | 32 |  | Id int IDENTITY NN; FromDate nvarchar(10); ToDate nvarchar(10); PrayTitle nvarchar(100); PrayText nvarchar(max); visited int NN; InsertDT datetime NN; updateDT datetime NN; HolyWord nvarchar(max); PrayText_Eng nvarchar(max) |
| `dbo.BessingHusband` | 45 |  | Id int IDENTITY NN; PrayTitle nvarchar(200); PrayText nvarchar(max); PrayHolyWord nvarchar(max); UpdateDT datetime NN; InsertDT datetime NN; visited int; PageNo int; IsVerified bit NN; VerifiedDT datetime; VerifiedBy nvarchar(100) |
| `dbo.BessingHusband_bak_v1` | 40 |  | Id int IDENTITY NN; PrayTitle nvarchar(200); PrayText nvarchar(max); PrayHolyWord nvarchar(max); UpdateDT datetime NN; InsertDT datetime NN; visited int; PageNo int |
| `dbo.BessingHusband_bak_v2` | 40 |  | Id int IDENTITY NN; PrayTitle nvarchar(200); PrayText nvarchar(max); PrayHolyWord nvarchar(max); UpdateDT datetime NN; InsertDT datetime NN; visited int; PageNo int |
| `dbo.BessingHusband_bak_v3` | 40 |  | Id int IDENTITY NN; PrayTitle nvarchar(200); PrayText nvarchar(max); PrayHolyWord nvarchar(max); UpdateDT datetime NN; InsertDT datetime NN; visited int; PageNo int |
| `dbo.BessingWife` | 60 |  | Id int IDENTITY NN; PrayTitle nvarchar(200); PrayText nvarchar(max); PrayHolyWord nvarchar(max); UpdateDT datetime NN; InsertDT datetime NN; visited int; PageNo int; IsVerified bit NN; VerifiedDT datetime; VerifiedBy nvarchar(100) |
| `dbo.BessingWife_bak_20261012` | 60 |  | Id int IDENTITY NN; PrayTitle nvarchar(200); PrayText nvarchar(max); PrayHolyWord nvarchar(max); UpdateDT datetime NN; InsertDT datetime NN; visited int; PageNo int; IsVerified bit NN; VerifiedDT datetime; VerifiedBy nvarchar(100) |
| `dbo.BessingWife_bak_v1` | 60 |  | Id int IDENTITY NN; PrayTitle nvarchar(200); PrayText nvarchar(max); PrayHolyWord nvarchar(max); UpdateDT datetime NN; InsertDT datetime NN; visited int; PageNo int |
| `dbo.BessingWife_bak_v2` | 60 |  | Id int IDENTITY NN; PrayTitle nvarchar(200); PrayText nvarchar(max); PrayHolyWord nvarchar(max); UpdateDT datetime NN; InsertDT datetime NN; visited int; PageNo int |
| `dbo.BessingWife_bak_v3` | 60 |  | Id int IDENTITY NN; PrayTitle nvarchar(200); PrayText nvarchar(max); PrayHolyWord nvarchar(max); UpdateDT datetime NN; InsertDT datetime NN; visited int; PageNo int |
| `dbo.BibleBasicInfo` | 66 |  | Id int IDENTITY NN; BibleText nvarchar(100); BibleText_EN nvarchar(100); BibleJang int NN; kind nvarchar(1); MinID int NN; MaxID int NN; ZulCount int NN; InsertDT datetime NN; updateDT datetime NN; OverView nvarchar(max); WhenWritten nvarchar(100); Author nvarchar(100); Purpose nvarchar(max); Kind2 nvarchar(2); kind2Desc nvarchar(50); SummaryURL1 nvarchar(200); SummaryURL2 nvarchar(200) |
| `dbo.BibleBookOverview_Detail` | 66 | Id | Id int IDENTITY NN; BookName nvarchar(50); Author nvarchar(200); AuthorDescription nvarchar(max); WrittenDateFrom nvarchar(20); WrittenDateTo nvarchar(20); AlternativeDate nvarchar(100); WrittenPlace nvarchar(200); WrittenTarget nvarchar(max); HistoricalPeriod nvarchar(100); HistoricalBackground nvarchar(max); Keywords nvarchar(300); ContentSummary nvarchar(max); SummaryURL1 nvarchar(200); SummaryURL2 nvarchar(200) |
| `dbo.BibleCompletionLog` | 10 | Id | Id int IDENTITY NN; UserId nvarchar(450) NN; UserName nvarchar(256); RoundNo int NN; CompletedAt date NN; CreatedAt datetime2 NN; IsSeed bit NN |
| `dbo.BibleGeomorphologicalWeeklyWordToReadAssoc` | 0 |  | BibleGeomorphologicalWeeklyWordToReadAssocID int IDENTITY NN; bibleGeomorphologicalID nvarchar(100); WeeklyWordToReadID nvarchar(100); InsertDT datetime NN; updateDT datetime NN |
| `dbo.BibleGiftMilestoneConfig` | 10 | Id | Id int IDENTITY NN; cYear int NN; RoundNo int NN; Title nvarchar(100) NN; SubTitle nvarchar(200); MaxUnitPrice int NN; IsActive bit NN; SortOrder int NN; CreatedAt datetime NN |
| `dbo.BibleGiftOrder` | 6 | OrderId | OrderId int IDENTITY NN; UserId nvarchar(450) NN; UserName nvarchar(256); RoundNo int NN; ConfigId int; ProductID int NN; UnitPriceSnapshot int NN; Status nvarchar(20) NN; IsCurrentDraft bit NN; SelectedAt datetime; PurchasedAt datetime; AdminId nvarchar(100); Memo nvarchar(200); CreatedAt datetime NN; UpdatedAt datetime NN; DeliveredAt datetime |
| `dbo.BibleGiftOrderLog` | 45 | LogId | LogId bigint IDENTITY NN; OrderId int; UserId nvarchar(450) NN; UserName nvarchar(256); RoundNo int NN; Action nvarchar(20) NN; ProductID int; UnitPrice int; ActorId nvarchar(256); ActorRole nvarchar(20); CreatedAt datetime NN |
| `dbo.BibleHubClickLog` | 67 | Id | Id int IDENTITY NN; Version varchar(10) NN; UserId nvarchar(450); UserName nvarchar(256); ClickedAt datetime2 NN |
| `dbo.BibleSearchLog` | 1499 | Id | Id int IDENTITY NN; Query nvarchar(200) NN; SearchedAt datetime2 NN; UserId nvarchar(450) |
| `dbo.BirthdayGiftPickMeMo` | 4 |  | PersonID int; Memo nvarchar(500) |
| `dbo.BirthdayGiftPickSms` | 24 | UserId | UserId nvarchar(450) NN; SmsSent bit NN; SmsSentAt datetime2; UpdatedAt datetime2 NN |
| `dbo.BirthdayGiftSkip` | 23 | PersonID | PersonID int NN; IsSkipped bit NN; SkippedAt datetime2; UpdatedBy nvarchar(100) |
| `dbo.BirthdayRecords` | 1 | Id | Id int IDENTITY NN; BulletinId int NN; MemberName nvarchar(100) NN; BirthDate date NN; DepartmentId int; Note nvarchar(max) |
| `dbo.BoardAnonymous` | 10 | Id | Id int IDENTITY NN; Title nvarchar(255) NN; Content nvarchar(max) NN; CreatedDate datetime NN; visited int; UserName nvarchar(256) |
| `dbo.BoardHelpDesk` | 3 | Id | Id int IDENTITY NN; Title nvarchar(255) NN; Content nvarchar(max) NN; CreatedDate datetime NN; visited int; UserName nvarchar(256) |
| `dbo.BoardRegistry` | 2 | Id | Id int IDENTITY NN; BoardName nvarchar(100) NN; PostTable sysname NN; ReplyTable sysname NN; IsEnabled bit NN; CreatedAt datetime2 NN |
| `dbo.Bookmark` | 3 | Id | Id int IDENTITY NN; BibleText nvarchar(100) NN; Jang int NN; Zul int NN; VerseText nvarchar(max); CreatedAt datetime NN |
| `dbo.Bulletins` | 3 | Id | Id int IDENTITY NN; ChurchId int NN; DepartmentId int; Title nvarchar(300) NN; BannerImageUrl nvarchar(max); BulletinDate date NN; IsPublished bit NN; CreatedAt datetime2 NN; PublishedAt datetime2; Benediction nvarchar(max); PrayerGroup nvarchar(max); SermonLeader nvarchar(max); SermonTitle nvarchar(max); BirthdayNames nvarchar(max); FreeContent1 nvarchar(max); FreeContent2 nvarchar(max); FreeContent3 nvarchar(max); FreeContent4 nvarchar(max); FreeTitle1 nvarchar(max); FreeTitle2 nvarchar(max); FreeTitle3 nvarchar(max); FreeTitle4 nvarchar(max); MissionBookWeek nvarchar(max); NextPrayerGroup nvarchar(max); StreetEvangelism nvarchar(max); WorshipInfo nvarchar(max); BibleReading nvarchar(max); NextWeekEvangelism nvarchar(max); NextWeekPrayer nvarchar(max) |
| `dbo.CalendarList` | 52 |  | Id int IDENTITY NN; DateSerial datetime NN; InsertDT datetime NN; updateDT datetime NN |
| `dbo.CalendarSerialList` | 730 |  | Id int IDENTITY NN; DayByDay datetime NN; WeekDayName nvarchar(10); DatePartNo int; InsertDT datetime NN; updateDT datetime NN |
| `dbo.CampPray` | 25 |  | PrayID int IDENTITY NN; PrayTEXTData nvarchar(max); ExposeDate datetime NN; InsertDT datetime NN; updateDT datetime NN; Subject nvarchar(200); BibleText nvarchar(100); BibleTextData nvarchar(500); publishYN nvarchar(1) |
| `dbo.ChurchDepartments` | 1 | Id | Id int IDENTITY NN; ChurchId int NN; Name nvarchar(100) NN; Description nvarchar(max); SortOrder int NN; IsActive bit NN; CreatedAt datetime2 NN; DeptCode nvarchar(50) |
| `dbo.Churches` | 1 | Id | Id int IDENTITY NN; Name nvarchar(200) NN; Address nvarchar(max); Website nvarchar(500); PhoneNumber nvarchar(50); LogoImageUrl nvarchar(max); IsActive bit NN; CreatedAt datetime2 NN |
| `dbo.ClassInfo_0000` | 129 |  | Id int IDENTITY NN; ClassYear int; ClassBan int; UserId nvarchar(450); ts nvarchar(1); InsertDT datetime NN; updateDT datetime NN; FullName nvarchar(100) |
| `dbo.ClassInfo_2026` | 82 |  | Id int IDENTITY NN; ClassYear int; ClassBan int; PersonID int; FullName_A nvarchar(100); updateDT datetime NN; InsertDT datetime NN; ts nvarchar(1); visibleOrder int |
| `dbo.ClassMaster` | 13 | ClassId | ClassId int IDENTITY NN; ClassYear int NN; ClassBan int NN; ClassName nvarchar(50); IsActive bit NN; SortOrder int NN; InsertDT datetime NN; UpdateDT datetime NN |
| `dbo.ClassTeacherMap_2026` | 13 | Id | Id int IDENTITY NN; ClassYear int NN; ClassBan int NN; TeacherPersonId int NN; IsActive bit NN; Memo nvarchar(200); InsertDT datetime NN; UpdateDT datetime NN; SubTeacherPersonId int |
| `dbo.ClickHistoryForEnglishBibleQuiz` | 0 |  | Id int IDENTITY NN; QuestionId int NN; UserName nvarchar(256); ClickDate datetime NN; AnswerQ nvarchar(200); TryCnt int NN; InsertDT datetime NN |
| `dbo.ClickHistoryForPoint` | 0 |  | Id int IDENTITY NN; Point int NN; UserName nvarchar(256); ClickDate datetime NN; InsertDT datetime NN |
| `dbo.ClickHistoryForPrayPhrase` | 4506 |  | Id int IDENTITY NN; PrayPhraseId int NN; ToMe nvarchar(1) NN; ToParent nvarchar(1) NN; ToFriend nvarchar(1) NN; UserName nvarchar(256); ClickDate datetime NN; InsertDT datetime NN; cYear int |
| `dbo.ClickHistoryForPrayPhrase_2025` | 5244 |  | Id int IDENTITY NN; PrayPhraseId int NN; ToMe nvarchar(1) NN; ToParent nvarchar(1) NN; ToFriend nvarchar(1) NN; UserName nvarchar(256); ClickDate datetime NN; InsertDT datetime NN; cYear int |
| `dbo.ClickHistoryForPrayPhrase_20260831` | 3789 |  | Id int IDENTITY NN; PrayPhraseId int NN; ToMe nvarchar(1) NN; ToParent nvarchar(1) NN; ToFriend nvarchar(1) NN; UserName nvarchar(256); ClickDate datetime NN; InsertDT datetime NN; cYear int |
| `dbo.ClickHistoryForQuestion` | 5406 |  | Id int IDENTITY NN; QuestionId int NN; UserName nvarchar(256); AnswerQ nvarchar(200); CorrectAnswer nvarchar(200); TryCnt int NN; ClickDate datetime NN; InsertDT datetime NN; cYear int |
| `dbo.ClickHistoryForQuestion_2025` | 5223 |  | Id int IDENTITY NN; QuestionId int NN; UserName nvarchar(256); AnswerQ nvarchar(200); CorrectAnswer nvarchar(200); TryCnt int NN; ClickDate datetime NN; InsertDT datetime NN; cYear int |
| `dbo.ClickHistoryForTodayWordToRead` | 4894 |  | Id int IDENTITY NN; TodayWordToReadId int NN; UserName nvarchar(256); ToMe nvarchar(1) NN; ToParent nvarchar(1) NN; ClickDate datetime NN; InsertDT datetime NN; updateDT datetime NN; cYear int |
| `dbo.ClickHistoryForTodayWordToRead_2025` | 5262 |  | Id int IDENTITY NN; TodayWordToReadId int NN; UserName nvarchar(256); ToMe nvarchar(1) NN; ToParent nvarchar(1) NN; ClickDate datetime NN; InsertDT datetime NN; updateDT datetime NN; cYear int |
| `dbo.ClickHistoryForWeeklyWordToRead` | 810 |  | Id int IDENTITY NN; WeeklyWordToReadId int NN; UserName nvarchar(256); ClickDate datetime NN; InsertDT datetime NN; cYear int |
| `dbo.ClickHistoryForWeeklyWordToRead_2025` | 792 |  | Id int IDENTITY NN; WeeklyWordToReadId int NN; UserName nvarchar(256); ClickDate datetime NN; InsertDT datetime NN; cYear int |
| `dbo.ClickHistorySummaryByYear` | 0 |  | DC_Total int; userName nvarchar(256); ByYear int NN |
| `dbo.ClickHistorySummaryByYear_` | 137 |  | DC_Total int; userName nvarchar(256); ByYear int NN |
| `dbo.CodeData` | 21 |  | Id int IDENTITY NN; MajorCD nvarchar(50); MinorCD nvarchar(50); MinorNM nvarchar(250); InsertDT datetime NN; updateDT datetime NN; displayOrder int |
| `dbo.CommentAnonymous` | 0 | Id | Id int IDENTITY NN; BoardId int NN; ParentCommentId int; Content nvarchar(max) NN; CreatedDate datetime NN; UserName nvarchar(256) |
| `dbo.CommentHelpDesk` | 2 | Id | Id int IDENTITY NN; BoardId int NN; ParentCommentId int; Content nvarchar(max) NN; CreatedDate datetime NN; UserName nvarchar(256) |
| `dbo.CommonMetaData` | 2 |  | Id int IDENTITY NN; Kind nvarchar(100); KindDesc nvarchar(200); InsertDT datetime NN; updateDT datetime NN |
| `dbo.DailyEnglish` | 365 | Id | Id int IDENTITY NN; EnglishText nvarchar(200) NN; KoreanText nvarchar(200); Pronunciation nvarchar(200); Description nvarchar(500); Example nvarchar(500); IsRead int NN; CreatedAt datetime NN; ExampleKorean nvarchar(600); VocabNote nvarchar(600) |
| `dbo.DailyEnglishVideo` | 3 | Id | Id int IDENTITY NN; YouTubeId nvarchar(30) NN; Title nvarchar(200) NN; CreatedAt datetime NN |
| `dbo.DeptGroupInfo` | 16 |  | Id int IDENTITY NN; ParentDeptCode nvarchar(250); DeptCode nvarchar(250); DeptName nvarchar(250); InsertDT datetime NN; updateDT datetime NN |
| `dbo.DeptGroupInfo2` | 10 |  | Id int IDENTITY NN; DeptCode int; DeptName nvarchar(250); InsertDT datetime NN; updateDT datetime NN; DeptCodeSub int |
| `dbo.DeptInfo` | 0 |  | Id int IDENTITY NN; DeptCode int; UserId nvarchar(450); InsertDT datetime NN; updateDT datetime NN |
| `dbo.DeptRoleUserAssoc` | 207 |  | Id int IDENTITY NN; DeptCode nvarchar(50); RoleCode nvarchar(50); UserId nvarchar(450); InsertDT datetime NN; updateDT datetime NN |
| `dbo.DeptTeacherMap_2026` | 13 |  | Id int IDENTITY NN; PersonId int NN; DeptCode nvarchar(50); RoleCode nvarchar(50); IsActive bit NN; InsertDT datetime NN; UpdateDT datetime NN; juboPrint bit |
| `dbo.DM2_FamilyMember` | 33 | Id | Id int IDENTITY NN; FamilyTeamId int NN; UserName nvarchar(100) NN; RoleCode tinyint NN; SortNo int NN; IsActive bit NN; InsertDT datetime NN; UpdateDT datetime NN; RelationCode tinyint NN; RelationLabel nvarchar(20) |
| `dbo.DM2_FamilyMilestoneConfig` | 5 | Id | Id int IDENTITY NN; Year int NN; PointThreshold int NN; MilestoneType nvarchar(20) NN; DisplayTitle nvarchar(200) NN; DisplayEmoji nvarchar(10); IsActive bit NN; SortOrder int NN; CreatedAt datetime2 NN |
| `dbo.DM2_FamilyMilestoneHidden` | 3 | Id | Id int IDENTITY NN; MilestoneConfigId int NN; FamilyTeamId int NN; HiddenAt datetime2 NN |
| `dbo.DM2_FamilyTeam` | 13 | FamilyTeamId | FamilyTeamId int IDENTITY NN; TeamName nvarchar(50) NN; IsActive bit NN; Memo nvarchar(200); InsertDT datetime NN; UpdateDT datetime NN |
| `dbo.DM2HubClickLog` | 208 | Id | Id int IDENTITY NN; Version varchar(10) NN; UserId nvarchar(450); UserName nvarchar(256); ClickedAt datetime2 NN |
| `dbo.DrawingVote` | 47 | Id | Id int IDENTITY NN; ImageNumber int NN; ImageUrl nvarchar(300) NN; LikeCount int NN; CreatedAt datetime2 NN; UpdatedAt datetime2 NN; Who nvarchar(300) |
| `dbo.DrawingVoteLog` | 196 | Id | Id int IDENTITY NN; DrawingVoteId int NN; VoterIp nvarchar(64) NN; CreatedAt datetime2 NN |
| `dbo.eBible` | 31102 | Id | Id int IDENTITY NN; BibleText nvarchar(100); Jang int NN; zul int NN; BibleData nvarchar(max); InsertDT datetime NN; updateDT datetime NN; JangZul int |
| `dbo.eBible_20250518` | 31102 |  | Id int IDENTITY NN; BibleText nvarchar(100); Jang int NN; zul int NN; BibleData nvarchar(max); InsertDT datetime NN; updateDT datetime NN |
| `dbo.eBibleEN` | 31102 | Id | Id int IDENTITY NN; BibleText nvarchar(100); BibleTextEN nvarchar(100); Jang int NN; zul int NN; BibleData nvarchar(max); InsertDT datetime NN; updateDT datetime NN |
| `dbo.eBibleEN_KJV` | 1308 | Id | Id int IDENTITY NN; BibleText nvarchar(100); BibleTextEN nvarchar(100); Jang int NN; zul int NN; BibleData nvarchar(max); InsertDT datetime NN; updateDT datetime NN |
| `dbo.eBibleEN_NIV` | 31102 | Id | Id int IDENTITY NN; BibleText nvarchar(100); BibleTextEN nvarchar(100); Jang int NN; zul int NN; BibleData nvarchar(max); InsertDT datetime NN; updateDT datetime NN |
| `dbo.eBibleHD` | 31102 | Id | Id int IDENTITY NN; BibleText nvarchar(100); Jang int NN; zul int NN; BibleData nvarchar(max); InsertDT datetime NN; updateDT datetime NN; JangZul int |
| `dbo.eBibleHD_` | 31102 | Id | Id int IDENTITY NN; BibleText nvarchar(100); Jang int NN; zul int NN; BibleData nvarchar(max); InsertDT datetime NN; updateDT datetime NN; JangZul int |
| `dbo.eBibleSummary` | 5516 |  | Id int IDENTITY NN; BibleText nvarchar(100); BibleKeyWord nvarchar(max); BibleSummary nvarchar(max); FromJang int NN; FromZul int NN; ToJang int NN; ToZul int NN; InsertDT datetime NN; updateDT datetime NN |
| `dbo.eBibleYouTuBu` | 1189 |  | Id int IDENTITY NN; BibleText nvarchar(100) NN; Jang int NN; URL nvarchar(200) NN; TimeMM int NN; TimeSS int NN; InsertDT datetime NN; updateDT datetime NN |
| `dbo.eBibleYouTuBuHit` | 0 |  | Id int IDENTITY NN; videoId nvarchar(200) NN; UserId nvarchar(450) NN; InsertDT datetime NN; updateDT datetime NN |
| `dbo.EnglishBibleQuiz` | 50 |  | Id int IDENTITY NN; ExposeDate datetime NN; QuestionType nvarchar(1); QuestionQ nvarchar(max); Question1 nvarchar(max); Question2 nvarchar(max); Question3 nvarchar(max); Question4 nvarchar(max); QuestionQ_KR nvarchar(max); Question1_KR nvarchar(max); Question2_KR nvarchar(max); Question3_KR nvarchar(max); Question4_KR nvarchar(max); AnswerQ nvarchar(100); CorrectAnswer nvarchar(100); UserName nvarchar(256); InsertDT datetime NN; updateDT datetime NN; AnswerGuide nvarchar(256); KindData nvarchar(20); publishYn nvarchar(1) |
| `dbo.EnglishBibleQuiz_1234` | 150 |  | Id int IDENTITY NN; ExposeDate datetime NN; QuestionType nvarchar(1); QuestionQ nvarchar(max); Question1 nvarchar(max); Question2 nvarchar(max); Question3 nvarchar(max); Question4 nvarchar(max); QuestionQ_KR nvarchar(max); Question1_KR nvarchar(max); Question2_KR nvarchar(max); Question3_KR nvarchar(max); Question4_KR nvarchar(max); AnswerQ nvarchar(100); CorrectAnswer nvarchar(100); UserName nvarchar(256); InsertDT datetime NN; updateDT datetime NN; AnswerGuide nvarchar(256); KindData nvarchar(20); publishYn nvarchar(1) |
| `dbo.EntranceExamPray` | 62 | PrayId | PrayId int IDENTITY NN; SortOrder int NN; Title nvarchar(200) NN; PrayText nvarchar(max) NN; BibleRef nvarchar(100); BibleText nvarchar(1000); ClickCount int NN; LikeCount int NN; UseYN char(1) NN; CreatedAt datetime NN; UpdatedAt datetime; RotateYN char(1) NN |
| `dbo.EntranceExamPray_bak_v1` | 29 |  | PrayId int IDENTITY NN; SortOrder int NN; Title nvarchar(200) NN; PrayText nvarchar(max) NN; BibleRef nvarchar(100); BibleText nvarchar(1000); ClickCount int NN; LikeCount int NN; UseYN char(1) NN; CreatedAt datetime NN; UpdatedAt datetime |
| `dbo.EntranceExamPray_bak_v2` | 40 |  | PrayId int IDENTITY NN; SortOrder int NN; Title nvarchar(200) NN; PrayText nvarchar(max) NN; BibleRef nvarchar(100); BibleText nvarchar(1000); ClickCount int NN; LikeCount int NN; UseYN char(1) NN; CreatedAt datetime NN; UpdatedAt datetime; RotateYN char(1) NN |
| `dbo.EntranceExamPray_bak_v3` | 39 |  | PrayId int IDENTITY NN; SortOrder int NN; Title nvarchar(200) NN; PrayText nvarchar(max) NN; BibleRef nvarchar(100); BibleText nvarchar(1000); ClickCount int NN; LikeCount int NN; UseYN char(1) NN; CreatedAt datetime NN; UpdatedAt datetime; RotateYN char(1) NN |
| `dbo.EntranceExamPray_bak_v4` | 39 |  | PrayId int IDENTITY NN; SortOrder int NN; Title nvarchar(200) NN; PrayText nvarchar(max) NN; BibleRef nvarchar(100); BibleText nvarchar(1000); ClickCount int NN; LikeCount int NN; UseYN char(1) NN; CreatedAt datetime NN; UpdatedAt datetime; RotateYN char(1) NN |
| `dbo.EntranceExamPray_bak_v5` | 39 |  | PrayId int IDENTITY NN; SortOrder int NN; Title nvarchar(200) NN; PrayText nvarchar(max) NN; BibleRef nvarchar(100); BibleText nvarchar(1000); ClickCount int NN; LikeCount int NN; UseYN char(1) NN; CreatedAt datetime NN; UpdatedAt datetime; RotateYN char(1) NN |
| `dbo.ErrorLogs` | 5 | Id | Id int IDENTITY NN; Message nvarchar(max) NN; CreatedAt datetime2 NN |
| `dbo.EventApplyInfo` | 34 |  | Id int IDENTITY NN; UserId nvarchar(450) NN; EventId int NN; insertDT datetime; updateDT datetime |
| `dbo.EventMater` | 24 |  | EventId int IDENTITY NN; EventTitle nvarchar(200); EventDesc nvarchar(max); EventPlace nvarchar(max); EventDayFrom datetime; EventDayTo datetime; EventTimeFrom time; EventCountLimitStudent int; EventCountLimitTeacher int; EventPublishYN nvarchar(1) NN; insertDT datetime; updateDT datetime; processStatus nvarchar(1); applyYN nvarchar(1); URL nvarchar(200); URLDesc nvarchar(200); EventTimeTo time; ForWhom nvarchar(200) |
| `dbo.EventMater1` | 6 |  | EventTitle nvarchar(200); EventDesc nvarchar(max); EventDayFrom datetime; EventDayTo datetime; EventCountLimitStudent int; EventCountLimitTeacher int; EventPublishYN nvarchar(1) NN |
| `dbo.everyBible` | 2127 |  | Id int IDENTITY NN; ExposeDate nvarchar(10); BibleText nvarchar(100); chapter int NN; startVerse int NN; endVerse int NN; InsertDT datetime NN; updateDT datetime NN |
| `dbo.exercise_Audience` | 60 | AudienceId | AudienceId int IDENTITY NN; ExerciseId int NN; Audience nvarchar(20) NN; Explanation nvarchar(1000) NN |
| `dbo.exercise_Category` | 12 | CategoryId | CategoryId int IDENTITY NN; CategoryName nvarchar(50) NN |
| `dbo.exercise_Category_Map` | 84 | ExerciseId,CategoryId | ExerciseId int NN; CategoryId int NN |
| `dbo.exercise_Effect` | 57 | EffectId | EffectId int IDENTITY NN; EffectText nvarchar(200) NN |
| `dbo.exercise_Effect_Map` | 120 | ExerciseId,EffectId | ExerciseId int NN; EffectId int NN |
| `dbo.exercise_Equipment` | 25 | EquipmentId | EquipmentId int IDENTITY NN; EquipmentName nvarchar(50) NN |
| `dbo.exercise_Equipment_Map` | 69 | ExerciseId,EquipmentId | ExerciseId int NN; EquipmentId int NN; Notes nvarchar(200) |
| `dbo.exercise_Exercise` | 25 | ExerciseId | ExerciseId int IDENTITY NN; Name nvarchar(100) NN; NameEn nvarchar(100); IsAnaerobic bit NN; Difficulty nvarchar(20) NN; Description nvarchar(1000); CreatedAt datetime2 NN; UpdatedAt datetime2 NN |
| `dbo.exercise_Muscle` | 29 | MuscleId | MuscleId int IDENTITY NN; MuscleName nvarchar(50) NN; Region nvarchar(50) |
| `dbo.exercise_Muscle_Map` | 162 | ExerciseId,MuscleId | ExerciseId int NN; MuscleId int NN; Role nvarchar(10) NN; SortOrder int |
| `dbo.exercise_Note` | 103 | NoteId | NoteId int IDENTITY NN; ExerciseId int NN; NoteType nvarchar(20) NN; NoteText nvarchar(500) NN |
| `dbo.exercise_Prescription` | 25 | ExerciseId | ExerciseId int NN; RepsMin int; RepsMax int; SetsMin int; SetsMax int; RestSecMin int; RestSecMax int; Tempo nvarchar(20); Notes nvarchar(200) |
| `dbo.exercise_Step` | 125 | StepId | StepId int IDENTITY NN; ExerciseId int NN; StepNo int NN; Phase nvarchar(20) NN; Title nvarchar(100); Instruction nvarchar(1000) NN |
| `dbo.exercise_Variation` | 121 | VariationId | VariationId int IDENTITY NN; ExerciseId int NN; VariationName nvarchar(100) NN; Description nvarchar(500) |
| `dbo.FamilyPhotoContest` | 1 | ContestId | ContestId int NN; IsPublished bit NN; PublishedAt datetime2; PublishedBy nvarchar(100) |
| `dbo.FamilyPhotoEntry` | 8 | EntryId | EntryId int IDENTITY NN; FamilyName nvarchar(40) NN; FamilyNameKey nvarchar(40) NN; PinHash nvarchar(128) NN; PinSalt nvarchar(32) NN; IsHidden bit NN; HiddenBy nvarchar(100); HiddenAt datetime2; CreatedIp nvarchar(64); CreatedAt datetime2 NN; UpdatedAt datetime2 NN |
| `dbo.FamilyPhotoItem` | 21 | PhotoId | PhotoId int IDENTITY NN; EntryId int NN; Slot int NN; Caption nvarchar(80); FilePath nvarchar(300) NN; ThumbPath nvarchar(300) NN; Width int NN; Height int NN; IsHidden bit NN; HiddenBy nvarchar(100); HiddenAt datetime2; CreatedAt datetime2 NN; UpdatedAt datetime2 NN |
| `dbo.FamilyPhotoLike` | 150 | LikeId | LikeId bigint IDENTITY NN; PhotoId int NN; DeviceKey nvarchar(64) NN; Ip nvarchar(64); CreatedAt datetime2 NN |
| `dbo.GiftPopupActionLog` | 82 | Id | Id bigint IDENTITY NN; UserName nvarchar(256) NN; Milestone int NN; ActionType nvarchar(20) NN; CreatedAtUtc datetime2 NN |
| `dbo.GiftPopupState` | 19 | UserName | UserName nvarchar(256) NN; LastSeenMilestone int NN; BlockUntilUtc datetime2; UpdatedAtUtc datetime2 NN |
| `dbo.GlobalClientCookie` | 219 | CookieSeq | CookieSeq bigint IDENTITY NN; CookieKey nvarchar(200) NN; ClientKey nvarchar(100) NN; HideUntil datetime2; CreatedAt datetime2 NN; UpdatedAt datetime2 |
| `dbo.HealingAndRestorationPray` | 32 | PrayId | PrayId int IDENTITY NN; SortOrder int NN; Title nvarchar(200) NN; PrayText nvarchar(max) NN; BibleRef nvarchar(100); BibleText nvarchar(1000); ClickCount int NN; UseYN char(1) NN; CreatedAt datetime NN; UpdatedAt datetime; LikeCount int NN; IsVerified bit NN; VerifiedDT datetime; VerifiedBy nvarchar(100) |
| `dbo.HealingAndRestorationPray_bak_20261011` | 31 |  | PrayId int IDENTITY NN; SortOrder int NN; Title nvarchar(200) NN; PrayText nvarchar(max) NN; BibleRef nvarchar(100); BibleText nvarchar(1000); ClickCount int NN; UseYN char(1) NN; CreatedAt datetime NN; UpdatedAt datetime; LikeCount int NN |
| `dbo.HealingAndRestorationPray_bak_v4` | 31 |  | PrayId int IDENTITY NN; SortOrder int NN; Title nvarchar(200) NN; PrayText nvarchar(max) NN; BibleRef nvarchar(100); BibleText nvarchar(1000); ClickCount int NN; UseYN char(1) NN; CreatedAt datetime NN; UpdatedAt datetime; LikeCount int NN |
| `dbo.HealingAndRestorationPray_bak_v5` | 31 |  | PrayId int IDENTITY NN; SortOrder int NN; Title nvarchar(200) NN; PrayText nvarchar(max) NN; BibleRef nvarchar(100); BibleText nvarchar(1000); ClickCount int NN; UseYN char(1) NN; CreatedAt datetime NN; UpdatedAt datetime; LikeCount int NN |
| `dbo.HealingAndRestorationPray_bak_v6` | 31 |  | PrayId int IDENTITY NN; SortOrder int NN; Title nvarchar(200) NN; PrayText nvarchar(max) NN; BibleRef nvarchar(100); BibleText nvarchar(1000); ClickCount int NN; UseYN char(1) NN; CreatedAt datetime NN; UpdatedAt datetime; LikeCount int NN |
| `dbo.HelpDesckPost` | 0 | PostId | PostId int IDENTITY NN; Title nvarchar(255) NN; Content nvarchar(max) NN; CreatedDate datetime NN |
| `dbo.JobCategories` | 0 | JobCategoryId | JobCategoryId nvarchar(450) NN; Name nvarchar(max) |
| `dbo.LiveEnglish` | 1 |  | Id int IDENTITY NN; OneLineText text; Hit int NN; InsertDT datetime NN; updateDT datetime NN |
| `dbo.loginHistory` | 161 |  | Id int IDENTITY NN; ControllerName nvarchar(450) NN; ActionName nvarchar(450) NN; UserName nvarchar(450) NN; requestIP nvarchar(100); InsertDT datetime NN; updateDT datetime NN |
| `dbo.loginHistoryN` | 272 |  | Id int IDENTITY NN; ControllerName nvarchar(450) NN; ActionName nvarchar(450) NN; UserName nvarchar(450) NN; requestIP nvarchar(100); InsertDT datetime NN; updateDT datetime NN; 구분 varchar(6) NN |
| `dbo.Logs` | 1 | Id | Id int IDENTITY NN; Level nvarchar(20) NN; Message nvarchar(max) NN; ClassName nvarchar(200); MethodName nvarchar(200); CreatedAt datetime2 NN |
| `dbo.LostAndFoundPostComments` | 2 | Id | Id int IDENTITY NN; BandPostId int NN; WriterName nvarchar(50); Content nvarchar(1000); CreatedAt datetime2 NN |
| `dbo.LostAndFoundPostImages` | 2 | Id | Id int IDENTITY NN; BandPostId int NN; ImagePath nvarchar(300) |
| `dbo.LostAndFoundPosts` | 2 | Id | Id int IDENTITY NN; WriterName nvarchar(50); Content nvarchar(max); LikeCount int NN; CreatedAt datetime2 NN |
| `dbo.LunarSolarCalendar` | 347 | LunarYear,LunarMonth,LunarDay,IsLeapMonth | LunarYear int NN; LunarMonth int NN; LunarDay int NN; IsLeapMonth bit NN; SolarDate date NN |
| `dbo.MemoryVerses` | 2 | Id | Id int IDENTITY NN; BulletinId int NN; Scripture nvarchar(max) NN; Reference nvarchar(200) |
| `dbo.MissionMenuConfig` | 24 |  | Id int IDENTITY NN; Role nvarchar(20) NN; MissionText nvarchar(200) NN; MissionMessage nvarchar(200); MissionURL1 nvarchar(300); MissionURL1_Text nvarchar(200); MissionURL2 nvarchar(300); MissionURL2_Text nvarchar(200); MissionURL3 nvarchar(300); MissionURL3_Text nvarchar(200); MissionURL4 nvarchar(300); MissionURL4_Text nvarchar(200); MissionURL5 nvarchar(300); MissionURL5_Text nvarchar(200); BackGround nvarchar(300); ButtonStyle nvarchar(100); DisplayOrder int NN; publishYN nvarchar(1); expireDate datetime; Header nvarchar(100); MissionURL6 nvarchar(200); MissionURL6_Text nvarchar(200) |
| `dbo.MyPhotoPick` | 18 |  | ExtratD int IDENTITY NN; UserName nvarchar(256) NN; InsertDT datetime; UpdateDT datetime |
| `dbo.Notice` | 26 | Id | Id int IDENTITY NN; Title nvarchar(200) NN; Content nvarchar(max) NN; AuthorName nvarchar(50); IsPinned bit NN; ViewCount int NN; IsDeleted bit NN; CreatedAt datetime2 NN; UpdatedAt datetime2; IsPopup bit NN; PopupUntil datetime2 |
| `dbo.NoticeAttachment` | 1 | Id | Id bigint IDENTITY NN; NoticeId int NN; OriginalName nvarchar(255) NN; StoredName nvarchar(255) NN; ContentType nvarchar(100); FileSize bigint NN; UploadedAt datetime2 NN; IsDeleted bit NN |
| `dbo.NoticeBoard` | 16 | Id | Id int IDENTITY NN; Title nvarchar(200) NN; Content nvarchar(max) NN; Writer nvarchar(60) NN; PasswordHash varbinary; ViewCount int NN; CreatedAt datetime2 NN; UpdatedAt datetime2; IsPinned bit NN; PopupUntil datetime2 |
| `dbo.NoticeBoard_File` | 0 | Id | Id int IDENTITY NN; PostId int NN; OriginalFileName nvarchar(260) NN; StoredFileName nvarchar(80) NN; ContentType nvarchar(100) NN; FileSize bigint NN; RelativePath nvarchar(500) NN; CreatedAt datetime2 NN |
| `dbo.NoticeBoard_Reply` | 88 | Id | Id int IDENTITY NN; PostId int NN; Content nvarchar(1000) NN; Writer nvarchar(60) NN; PasswordHash varbinary; CreatedAt datetime2 NN |
| `dbo.Offerings` | 3 | Id | Id int IDENTITY NN; BulletinId int NN; OfferingType nvarchar(100) NN; DonorName nvarchar(100) NN; Note nvarchar(max) |
| `dbo.OneLineFamousWord` | 127 |  | Id int IDENTITY NN; OneLineText nvarchar(max); InsertDT datetime NN; updateDT datetime NN |
| `dbo.Opic` | 151 |  | Id int IDENTITY NN; PageNo int NN; Question nvarchar(400); KeyWord1 nvarchar(100); KeyWord1Desc nvarchar(max); KeyWord2 nvarchar(100); KeyWord2Desc nvarchar(max); KeyWord3 nvarchar(100); KeyWord3Desc nvarchar(max); KeyWord4 nvarchar(max); KeyWord4Desc nvarchar(max); BodyContents nvarchar(max); Visited int; Vocabulary nvarchar(max); Memo nvarchar(max); WorkVisited int; UpdateDT datetime NN; InsertDT datetime NN |
| `dbo.Participants` | 5 | Id | Id int IDENTITY NN; SessionId int NN; Token nvarchar(64) NN; Nickname nvarchar(30); JoinedAt datetime2 NN; LastSeenAt datetime2; IsBlocked bit NN; IpAddress nvarchar(45) |
| `dbo.PatientPray_1` | 35 |  | PrayId int IDENTITY NN; SortOrder int NN; Title nvarchar(200) NN; PrayText nvarchar(max) NN; BibleRef nvarchar(100); BibleText nvarchar(1000); ClickCount int NN; UseYN char(1) NN; CreatedAt datetime NN; UpdatedAt datetime |
| `dbo.PatientPray_backup_20260827` | 35 |  | PrayId int IDENTITY NN; SortOrder int NN; Title nvarchar(200) NN; PrayText nvarchar(max) NN; BibleRef nvarchar(100); BibleText nvarchar(1000); ClickCount int NN; UseYN char(1) NN; CreatedAt datetime NN; UpdatedAt datetime |
| `dbo.PatientPray_bak_v2` | 35 |  | PrayId int IDENTITY NN; SortOrder int NN; Title nvarchar(200) NN; PrayText nvarchar(max) NN; BibleRef nvarchar(100); BibleText nvarchar(1000); ClickCount int NN; UseYN char(1) NN; CreatedAt datetime NN; UpdatedAt datetime |
| `dbo.PatientPray_bak_v3` | 35 |  | PrayId int IDENTITY NN; SortOrder int NN; Title nvarchar(200) NN; PrayText nvarchar(max) NN; BibleRef nvarchar(100); BibleText nvarchar(1000); ClickCount int NN; UseYN char(1) NN; CreatedAt datetime NN; UpdatedAt datetime |
| `dbo.PersonMaster_2025` | 212 |  | PersonId int IDENTITY NN; UserId nvarchar(450) NN; PhoneNumber nvarchar(max); bMonth int; bDay int; insertDT datetime; updateDT datetime; fullName nvarchar(100); sex nvarchar(1); PhoneNumber2 nvarchar(20); HomeAaddress nvarchar(200); school nvarchar(200); readBibleAllCount int; plusMins nvarchar(1); bYear int; birthDay_PublishYN nvarchar(1) |
| `dbo.PersonMaster_2026` | 227 | PersonId | PersonId int IDENTITY NN; bYear int; bMonth int; bDay int; plusMins nvarchar(1); birthDay_PublishYN nvarchar(1); fullName nvarchar(100); sex nvarchar(1); PhoneNumber nvarchar(max); PhoneNumber2 nvarchar(20); HomeAaddress nvarchar(200); school nvarchar(200); readBibleAllCount int; insertDT datetime; updateDT datetime; UserName nvarchar(256) |
| `dbo.PersonMaster_2026_2026_0309` | 215 |  | PersonId int IDENTITY NN; bYear int; bMonth int; bDay int; plusMins nvarchar(1); birthDay_PublishYN nvarchar(1); fullName nvarchar(100); sex nvarchar(1); PhoneNumber nvarchar(max); PhoneNumber2 nvarchar(20); HomeAaddress nvarchar(200); school nvarchar(200); readBibleAllCount int; insertDT datetime; updateDT datetime; UserName nvarchar(256) |
| `dbo.PhotoContest` | 1 |  | Id int IDENTITY NN; SummaryText nvarchar(200); FullText nvarchar(max); URLData nvarchar(300); visited int; InsertDT datetime NN; updateDT datetime NN |
| `dbo.Photos` | 3122 | Id | Id int IDENTITY NN; AlbumId int NN; FileName nvarchar(260) NN; OriginalName nvarchar(260) NN; ContentType nvarchar(100) NN; FileSizeBytes bigint NN; Width int; Height int; HashSha256 varbinary; StoragePath nvarchar(500) NN; UploadedAt datetime2 NN; ThumbnailPath nvarchar(500); ThumbnailStoragePath nvarchar(500); UserName nvarchar(100) |
| `dbo.PlaceItem` | 12 | PlaceId | PlaceId int IDENTITY NN; Category nvarchar(20) NN; Name nvarchar(200) NN; ExternalUrl nvarchar(500) NN; Address nvarchar(300); Tags nvarchar(200); ThumbnailUrl nvarchar(500); IsActive bit NN; InsertDT datetime2 NN; UpdateDT datetime2 |
| `dbo.Point_DevotionMonthly` | 115 | Ym,PersonId | Ym char(7) NN; PersonId int NN; Points int NN; InsertDT datetime NN; UpdateDT datetime NN |
| `dbo.PopMessage` | 409 |  | Id int IDENTITY NN; MessageText nvarchar(max); readYN nvarchar(1); ToWhom nvarchar(256); FromWhom nvarchar(256); Visited int NN; UpdateDT datetime NN; InsertDT datetime NN |
| `dbo.PraiseContents` | 103 |  | Id int IDENTITY NN; contentsURL nvarchar(200); contentsText nvarchar(200); kind nvarchar(20); visited int NN; InsertDT datetime NN; updateDT datetime; singer nvarchar(200) |
| `dbo.Prayer365` | 77 | MonthDay | MonthDay char(4) NN; Title nvarchar(200) NN; VerseRef nvarchar(100); VerseText nvarchar(1000); Body nvarchar(max) NN; Fixes nvarchar(max); IsReviewed bit NN; UpdatedAt datetime2 NN; UpdatedBy nvarchar(100); ViewCount int NN; PrayCount int NN |
| `dbo.Prayer365PrayLog` | 11 | LogId | LogId bigint IDENTITY NN; MonthDay char(4) NN; PrayDate date NN; UserName nvarchar(256) NN; UserId nvarchar(450); PrayedAt datetime2 NN |
| `dbo.Prayer365ViewDay` | 2 | ViewDate | ViewDate date NN; ViewCount int NN |
| `dbo.PrayerArrow` | 31 | PrayerId | PrayerId int IDENTITY NN; DayIndex int NN; Title nvarchar(100) NN; Line1 nvarchar(400) NN; Line2 nvarchar(400) NN; Line3 nvarchar(400) NN; IsActive bit NN; InsertDT datetime2 NN; UpdateDT datetime2 |
| `dbo.PrayFreind` | 27 |  | PrayId int IDENTITY NN; UserId nvarchar(450); PrayText nvarchar(max); InsertDT datetime NN; updateDT datetime NN; visited int; visitDT datetime; WriteUserName nvarchar(256); WriteUserId nvarchar(450) |
| `dbo.PrayHistory` | 120 |  | PrayId int IDENTITY NN; PersonPraying nvarchar(450); PrayerRecipient nvarchar(450); WhenPray datetime NN; InsertDT datetime NN |
| `dbo.PrayPhrase` | 729 |  | Id int IDENTITY NN; ExposeDate datetime NN; PrayTitle nvarchar(200); PraryBeginMesssage nvarchar(200); BibleTextToPray nvarchar(200); BibleTextToPrayFromJang int NN; BibleTextToPrayFromzul int NN; BibleTextToPrayToJang int NN; BibleTextToPrayTozul int NN; PrayContent1 nvarchar(200); PrayContent2 nvarchar(200); PrayContent3 nvarchar(200); PraryEndMesssage nvarchar(200); InsertDT datetime NN; updateDT datetime NN; PrayWrite nvarchar(200) |
| `dbo.PrayPhrase_111` | 729 |  | Id int IDENTITY NN; ExposeDate datetime NN; PrayTitle nvarchar(200); PraryBeginMesssage nvarchar(200); BibleTextToPray nvarchar(200); BibleTextToPrayFromJang int NN; BibleTextToPrayFromzul int NN; BibleTextToPrayToJang int NN; BibleTextToPrayTozul int NN; PrayContent1 nvarchar(200); PrayContent2 nvarchar(200); PrayContent3 nvarchar(200); PraryEndMesssage nvarchar(200); InsertDT datetime NN; updateDT datetime NN; PrayWrite nvarchar(200) |
| `dbo.PrayPhrase_bak_v1` | 729 |  | Id int IDENTITY NN; ExposeDate datetime NN; PrayTitle nvarchar(200); PraryBeginMesssage nvarchar(200); BibleTextToPray nvarchar(200); BibleTextToPrayFromJang int NN; BibleTextToPrayFromzul int NN; BibleTextToPrayToJang int NN; BibleTextToPrayTozul int NN; PrayContent1 nvarchar(200); PrayContent2 nvarchar(200); PrayContent3 nvarchar(200); PraryEndMesssage nvarchar(200); InsertDT datetime NN; updateDT datetime NN; PrayWrite nvarchar(200) |
| `dbo.PrayPhrase_bak_v2` | 729 |  | Id int IDENTITY NN; ExposeDate datetime NN; PrayTitle nvarchar(200); PraryBeginMesssage nvarchar(200); BibleTextToPray nvarchar(200); BibleTextToPrayFromJang int NN; BibleTextToPrayFromzul int NN; BibleTextToPrayToJang int NN; BibleTextToPrayTozul int NN; PrayContent1 nvarchar(200); PrayContent2 nvarchar(200); PrayContent3 nvarchar(200); PraryEndMesssage nvarchar(200); InsertDT datetime NN; updateDT datetime NN; PrayWrite nvarchar(200) |
| `dbo.PrayPhrase_bak_v3` | 729 |  | Id int IDENTITY NN; ExposeDate datetime NN; PrayTitle nvarchar(200); PraryBeginMesssage nvarchar(200); BibleTextToPray nvarchar(200); BibleTextToPrayFromJang int NN; BibleTextToPrayFromzul int NN; BibleTextToPrayToJang int NN; BibleTextToPrayTozul int NN; PrayContent1 nvarchar(200); PrayContent2 nvarchar(200); PrayContent3 nvarchar(200); PraryEndMesssage nvarchar(200); InsertDT datetime NN; updateDT datetime NN; PrayWrite nvarchar(200) |
| `dbo.PrayPhrase3` | 365 |  | Id int IDENTITY NN; Id3 int; ExposeDate datetime NN; PrayTitle nvarchar(200); PraryBeginMesssage nvarchar(200); BibleTextToPray nvarchar(200); BibleTextToPrayFromJang int NN; BibleTextToPrayFromzul int NN; BibleTextToPrayToJang int NN; BibleTextToPrayTozul int NN; PrayContent1 nvarchar(200); PrayContent2 nvarchar(200); PrayContent3 nvarchar(200); PraryEndMesssage nvarchar(200); InsertDT datetime NN; updateDT datetime NN; PrayWrite nvarchar(200) |
| `dbo.PrayPhraseTemplate` | 31 |  | Id int IDENTITY NN; DayNo int NN; PrayTitle nvarchar(200); PraryBeginMesssage nvarchar(200); BibleTextToPray nvarchar(200); BibleTextToPrayFromJang int NN; BibleTextToPrayFromzul int NN; BibleTextToPrayToJang int NN; BibleTextToPrayTozul int NN; PrayContent1 nvarchar(200); PrayContent2 nvarchar(200); PrayContent3 nvarchar(200); PraryEndMesssage nvarchar(200); InsertDT datetime NN; updateDT datetime NN; PrayWrite nvarchar(200) |
| `dbo.PrayPhraseTemplate_20260831` | 31 |  | Id int IDENTITY NN; DayNo int NN; PrayTitle nvarchar(200); PraryBeginMesssage nvarchar(200); BibleTextToPray nvarchar(200); BibleTextToPrayFromJang int NN; BibleTextToPrayFromzul int NN; BibleTextToPrayToJang int NN; BibleTextToPrayTozul int NN; PrayContent1 nvarchar(200); PrayContent2 nvarchar(200); PrayContent3 nvarchar(200); PraryEndMesssage nvarchar(200); InsertDT datetime NN; updateDT datetime NN; PrayWrite nvarchar(200) |
| `dbo.PrayPhraseTemplate_bak_v1` | 31 |  | Id int IDENTITY NN; DayNo int NN; PrayTitle nvarchar(200); PraryBeginMesssage nvarchar(200); BibleTextToPray nvarchar(200); BibleTextToPrayFromJang int NN; BibleTextToPrayFromzul int NN; BibleTextToPrayToJang int NN; BibleTextToPrayTozul int NN; PrayContent1 nvarchar(200); PrayContent2 nvarchar(200); PrayContent3 nvarchar(200); PraryEndMesssage nvarchar(200); InsertDT datetime NN; updateDT datetime NN; PrayWrite nvarchar(200) |
| `dbo.PrayPhraseTemplate_bak_v2` | 31 |  | Id int IDENTITY NN; DayNo int NN; PrayTitle nvarchar(200); PraryBeginMesssage nvarchar(200); BibleTextToPray nvarchar(200); BibleTextToPrayFromJang int NN; BibleTextToPrayFromzul int NN; BibleTextToPrayToJang int NN; BibleTextToPrayTozul int NN; PrayContent1 nvarchar(200); PrayContent2 nvarchar(200); PrayContent3 nvarchar(200); PraryEndMesssage nvarchar(200); InsertDT datetime NN; updateDT datetime NN; PrayWrite nvarchar(200) |
| `dbo.PrayPhraseTemplate_bak_v3` | 31 |  | Id int IDENTITY NN; DayNo int NN; PrayTitle nvarchar(200); PraryBeginMesssage nvarchar(200); BibleTextToPray nvarchar(200); BibleTextToPrayFromJang int NN; BibleTextToPrayFromzul int NN; BibleTextToPrayToJang int NN; BibleTextToPrayTozul int NN; PrayContent1 nvarchar(200); PrayContent2 nvarchar(200); PrayContent3 nvarchar(200); PraryEndMesssage nvarchar(200); InsertDT datetime NN; updateDT datetime NN; PrayWrite nvarchar(200) |
| `dbo.PresentItemPick` | 55 | PickId | PickId int IDENTITY NN; UserId nvarchar(50) NN; ProductId int NN; ProductName nvarchar(200) NN; ProductDescription nvarchar(500); PickedAt datetime NN; IsActive bit NN; IsSent bit NN; SentAt datetime2; sourceType nvarchar(100); IsPurchased bit NN; PurchasedAt datetime2 |
| `dbo.QuestionBankData` | 2483 |  | Id int IDENTITY NN; QuestionType nvarchar(1); QuestionQ nvarchar(max); Question1 nvarchar(max); Question2 nvarchar(max); Question3 nvarchar(max); Question4 nvarchar(max); CorrectAnswer nvarchar(100); BibleText nvarchar(100); Jang int NN; zul int NN; UserName nvarchar(256); InsertDT datetime NN; updateDT datetime NN; publishYN nvarchar(1); zul1 int; seqNo int; example1 nvarchar(200); example2 nvarchar(200); example3 nvarchar(200); example4 nvarchar(200); queryCode nvarchar(200); AnswerGuide nvarchar(max) |
| `dbo.QuestionData` | 276 |  | Id int IDENTITY NN; ExposeDate datetime NN; QuestionType nvarchar(1); QuestionQ nvarchar(max); Question1 nvarchar(max); Question2 nvarchar(max); Question3 nvarchar(max); Question4 nvarchar(max); AnswerQ nvarchar(100); CorrectAnswer nvarchar(100); UserName nvarchar(256); InsertDT datetime NN; updateDT datetime NN; AnswerGuide nvarchar(500); KindData nvarchar(20); appendedComments text; QuestionBackground nvarchar(max); cYear int |
| `dbo.QuestionData_2025` | 535 |  | Id int IDENTITY NN; ExposeDate datetime NN; QuestionType nvarchar(1); QuestionQ nvarchar(max); Question1 nvarchar(max); Question2 nvarchar(max); Question3 nvarchar(max); Question4 nvarchar(max); AnswerQ nvarchar(100); CorrectAnswer nvarchar(100); UserName nvarchar(256); InsertDT datetime NN; updateDT datetime NN; AnswerGuide nvarchar(500); KindData nvarchar(20); appendedComments text; QuestionBackground nvarchar(max) |
| `dbo.QuestionOptions` | 0 | Id | Id int IDENTITY NN; QuestionId int NN; Order int NN; Text nvarchar(200) NN; IsCorrect bit NN; ImageUrl nvarchar(max) |
| `dbo.Questions` | 1 | Id | Id int IDENTITY NN; SessionId int NN; Order int NN; Type int NN; Title nvarchar(2000) NN; ConfigJson nvarchar(max); Status int NN; OpenedAt datetime2; ClosedAt datetime2 |
| `dbo.ReadingProgress` | 1189 | Id | Id int IDENTITY NN; BibleText nvarchar(100) NN; Jang int NN; IsRead bit NN; ReadDate datetime |
| `dbo.RepairHomePage` | 0 |  | Id int IDENTITY NN; RequestContents nvarchar(300); UserName nvarchar(256); InsertDT datetime NN; updateDT datetime NN; AnswerContents nvarchar(300) |
| `dbo.ReplayBoardPostLike` | 0 | PostTable,PostId,UserId | PostTable nvarchar(128) NN; PostId int NN; UserId nvarchar(450) NN; CreatedAt datetime2 NN |
| `dbo.ReplayBoardPostMeta` | 19 | PostTable,PostId | PostTable nvarchar(128) NN; PostId int NN; AuthorUserId nvarchar(450); WriterSnapshot nvarchar(60); IsNotice bit NN; LikeCount int NN; CreatedAt datetime2 NN |
| `dbo.RGBColorGradientTable` | 15 |  | Id int IDENTITY NN; ColorName nvarchar(200); BackGroud1 nvarchar(300); BackGroud2 nvarchar(300); BackGroud3 nvarchar(300); FontColor nvarchar(300) |
| `dbo.RGBColorTable` | 199 |  | Id int IDENTITY NN; ColorName nvarchar(200); BackGroud nvarchar(300); FontColor nvarchar(300); ColorType nvarchar(200) |
| `dbo.RhapsodyOfRealities` | 493 |  | Id int IDENTITY NN; ExposeDate nvarchar(10); WordTitle nvarchar(max); WordPray nvarchar(max); HolyWord nvarchar(max); WordPhrase nvarchar(max); visited int NN; InsertDT datetime NN; updateDT datetime NN; YouTuBuURL nvarchar(200); EndMinute int; EndSecond int |
| `dbo.sequenceTable` | 152 |  | SeqNo int IDENTITY NN; Dummy int |
| `dbo.SerialNo` | 109 |  | Id int IDENTITY NN; Dummy int |
| `dbo.Sessions` | 1 | Id | Id int IDENTITY NN; Code nvarchar(3) NN; Title nvarchar(100) NN; OwnerUserId nvarchar(max); Status int NN; ActiveQuestionId int; CreatedAt datetime2 NN; ClosedAt datetime2 |
| `dbo.ShortVideo` | 174 | ShortId | ShortId int IDENTITY NN; YoutubeId nvarchar(16) NN; Title nvarchar(200); IsActive bit NN; InsertDT datetime2 NN; UpdateDT datetime2; VideoKind nvarchar(50) |
| `dbo.SiteDailyVisit` | 17 | VisitDate | VisitDate date NN; VisitCount int NN |
| `dbo.StockCompanies` | 0 | Id | Id int IDENTITY NN; StockCode varchar(6) NN; CorpCode varchar(8) NN; CorpName nvarchar(200) NN; CorpEngName nvarchar(200); Market varchar(20) NN; ModifyDate varchar(8); CreatedAt datetime2 NN; UpdatedAt datetime2 NN |
| `dbo.StoryElementary123` | 16 |  | Id int IDENTITY NN; contentsURL nvarchar(200); contentsText nvarchar(200); kind nvarchar(20); visited int NN; InsertDT datetime NN; updateDT datetime |
| `dbo.StudentPointGiftOrder` | 29 | OrderId | OrderId int IDENTITY NN; UserName nvarchar(256) NN; ProductID int NN; CreditCostSnapshot int NN; Status nvarchar(30) NN; IsCurrentDraft bit NN; CreatedAt datetime NN; PurchasedAt datetime; AdminId nvarchar(50); MileStone int; Memo nvarchar(200); DeliveredAt datetime |
| `dbo.StudentPointGiftPopupState` | 0 | UserName | UserName nvarchar(256) NN; LastSeenMilestone int; DeferredUntil datetime; UpdatedAt datetime NN |
| `dbo.StudentPointMilestoneCredit` | 20 | MilestonePoint | MilestonePoint int NN; Credit int NN; IsActive bit NN; CreatedAt datetime NN; ModifiedAt datetime NN |
| `dbo.SundaySchoolJubo` | 1 | Id | Id int IDENTITY NN; JuboDate date NN; Season nvarchar(100); Title nvarchar(300) NN; TemplateType nvarchar(50) NN; IsPublished bit NN; CreatedBy nvarchar(200); CreatedAt datetime NN; LastModifiedBy nvarchar(200); LastModifiedAt datetime |
| `dbo.SundaySchoolJuboBlock` | 8 | Id | Id int IDENTITY NN; JuboId int NN; BlockType nvarchar(100) NN; BlockTitle nvarchar(300); BlockContent nvarchar(max); SortOrder int NN; IsVisible bit NN; LastModifiedBy nvarchar(200); LastModifiedAt datetime |
| `dbo.SundaySchoolJuboSnapshot` | 3 | Id | Id int IDENTITY NN; JuboId int NN; SnapshotJson nvarchar(max); CreatedAt datetime NN; CreatedBy nvarchar(200) |
| `dbo.SurveyData` | 5 |  | Id int IDENTITY NN; SurveyText nvarchar(200); question11 nvarchar(max); question21 nvarchar(max); question22 nvarchar(max); question31 nvarchar(max); question41 nvarchar(max); question42 nvarchar(max); question51 nvarchar(max); InsertDT datetime NN |
| `dbo.TeacherBook` | 26 | Id | Id int IDENTITY NN; LessonKey nvarchar(50) NN; LessonNumber int; LessonLabel nvarchar(100) NN; LessonTitle nvarchar(max); UnitNumber int; UnitTitle nvarchar(200); BibleTextRef nvarchar(500); MemoryVerseRef nvarchar(200); MemoryVerseText nvarchar(max); LearningObjective nvarchar(max); ApplicationText nvarchar(max); TeacherStudyTitle nvarchar(200); TeacherStudyContent nvarchar(max); UnitIntroContent nvarchar(max); LearnContent nvarchar(max); WordGlassesText nvarchar(500); LiveContent nvarchar(max); TeacherTip nvarchar(max); ClosingContent nvarchar(max); PrayerText nvarchar(max); Page1ImagePath nvarchar(500); Page2ImagePath nvarchar(500); Page3ImagePath nvarchar(500); Page4ImagePath nvarchar(500); AcademicYear int NN; Semester int NN; WeeklyWordToReadId int; PublishYn nchar(1) NN; CreatedAt datetime NN |
| `dbo.TeacherCandidate_2026` | 13 | PersonId | PersonId int NN; IsActive bit NN; Memo nvarchar(200); InsertDT datetime NN; UpdateDT datetime NN |
| `dbo.TodayWordToRead` | 364 |  | Id int IDENTITY NN; ExposeDate datetime NN; BibleSubject nvarchar(100); BibleText nvarchar(100); FromJang int; FromZul int; ToJang int; ToZul int; InsertDT datetime; updateDT datetime; publishYn nvarchar(1); note1Keyword nvarchar(200); note1 nvarchar(max); note1Question1 nvarchar(100); note1Question2 nvarchar(100); note2Keyword nvarchar(200); note2 nvarchar(max); note2Question1 nvarchar(100); note2Question2 nvarchar(100); todayPrayForPerson nvarchar(100); todayPrayForHome nvarchar(100); todayDescribe1 nvarchar(200); todayDescribe2 nvarchar(200); visited int; todayDescribe1KeyWord nvarchar(100); todayDescribe2KeyWord nvarchar(100); BibleSummary nvarchar(max); YouTuBuStartMM int; YouTuBuStartSS int; YouTuBuEndMM int; YouTuBuEndSS int; synopsis nvarchar(max); YouTuBuStartMM2 int; YouTuBuStartSS2 int; YouTuBuEndSS2 int; YouTuBuEndMM2 int; YouTuBuStartMM3 int; YouTuBuStartSS3 int; YouTuBuEndSS3 int; YouTuBuEndMM3 int; YouTuBuStartMM4 int; YouTuBuStartSS4 int; YouTuBuEndSS4 int; YouTuBuEndMM4 int; YouTuBuStartMM5 int; YouTuBuStartSS5 int; YouTuBuEndSS5 int; YouTuBuEndMM5 int |
| `dbo.TodayWordToRead_2025` | 730 |  | Id int IDENTITY NN; ExposeDate datetime NN; BibleSubject nvarchar(100); BibleText nvarchar(100) NN; FromJang int NN; FromZul int NN; ToJang int NN; ToZul int NN; InsertDT datetime NN; updateDT datetime NN; publishYn nvarchar(1) NN; note1Keyword nvarchar(200); note1 nvarchar(max); note1Question1 nvarchar(100); note1Question2 nvarchar(100); note2Keyword nvarchar(200); note2 nvarchar(max); note2Question1 nvarchar(100); note2Question2 nvarchar(100); todayPrayForPerson nvarchar(100); todayPrayForHome nvarchar(100); todayDescribe1 nvarchar(200); todayDescribe2 nvarchar(200); visited int; todayDescribe1KeyWord nvarchar(100); todayDescribe2KeyWord nvarchar(100); BibleSummary nvarchar(max); YouTuBuStartMM int; YouTuBuStartSS int; YouTuBuEndMM int; YouTuBuEndSS int; synopsis nvarchar(max); YouTuBuStartMM2 int; YouTuBuStartSS2 int; YouTuBuEndSS2 int; YouTuBuEndMM2 int; YouTuBuStartMM3 int; YouTuBuStartSS3 int; YouTuBuEndSS3 int; YouTuBuEndMM3 int; YouTuBuStartMM4 int; YouTuBuStartSS4 int; YouTuBuEndSS4 int; YouTuBuEndMM4 int; YouTuBuStartMM5 int; YouTuBuStartSS5 int; YouTuBuEndSS5 int; YouTuBuEndMM5 int; cYear int |
| `dbo.UiMenuPolicy` | 9 | MenuUrl | MenuUrl nvarchar(300) NN; AllowedTypes varchar(10) NN; Title nvarchar(100); UseYN char(1) NN; UpdateDT datetime NN; InsertDT datetime NN |
| `dbo.UploadData` | 333 |  | UploadID int IDENTITY NN; CategoryData nvarchar(100); TEXTData nvarchar(200); URLData nvarchar(300); InsertDT datetime NN; updateDT datetime NN |
| `dbo.UploadWeeklyWordToReadAssoc` | 941 |  | UploadWeeklyWordToReadAssocID int IDENTITY NN; UploadID nvarchar(100); WeeklyWordToReadID nvarchar(100); InsertDT datetime NN; updateDT datetime NN |
| `dbo.UserAwardOrder` | 0 | OrderID | OrderID int IDENTITY NN; UserId nvarchar(50) NN; ProductID int NN; CreditCostSnapshot int NN; Status nvarchar(30) NN; CreatedAt datetime NN; PurchasedAt datetime; AdminId nvarchar(50) |
| `dbo.UserBibleProgress` | 199153 | UserId,VerseId | UserId nvarchar(450) NN; VerseId int NN; ReadDate datetime NN |
| `dbo.UserBibleProgress_20260725` | 282188 |  | UserId nvarchar(450) NN; VerseId int NN; ReadDate datetime NN |
| `dbo.UserBibleProgress_delete` | 31102 |  | UserId nvarchar(450) NN; VerseId int NN; ReadDate datetime NN |
| `dbo.UserBibleProgressCompleted` | 8 |  | UserName nvarchar(256); readBibleAllCount int; fullName nvarchar(max) |
| `dbo.UserBookMark` | 3094 | UserId,VerseId | UserId nvarchar(450) NN; VerseId int NN; InsertDT datetime NN |
| `dbo.UserSettings` | 101 |  | Id nvarchar(450) NN; LangCD nvarchar(2); PagesForView int; InsertDT datetime NN; updateDT datetime NN; QuestionWorkDate datetime; Notice_1_Collapse nvarchar(1); BibleBookOverview nvarchar(1) |
| `dbo.UserSettings_20250623` | 20 |  | Id nvarchar(450) NN; LangCD nvarchar(2); PaesForView int; InsertDT datetime NN; updateDT datetime NN; QuestionWorkDate datetime |
| `dbo.UserSettings_cookieName` | 0 |  | Id nvarchar(450) NN; cookieName nvarchar(200); cookieValue nvarchar(200); cookieDays int; InsertDT datetime NN; updateDT datetime NN |
| `dbo.UserUiPreference` | 2918 | PrefId | PrefId int IDENTITY NN; UserName nvarchar(100); VisitorId nvarchar(50); IPKey nvarchar(64); UserType char(1) NN; LastSeenDT datetime2 NN; UpdateDT datetime2 NN; InsertDT datetime2 NN; UserNameTrim nvarchar(100); IPKeyTrim nvarchar(64) |
| `dbo.VBSParticipationSurveyAttendance` | 123 | Id | Id bigint IDENTITY NN; UserName nvarchar(100) NN; FullName nvarchar(100); RoleCode nvarchar(20); RetreatDate date NN; Slot char(2) NN; IsAttend bit; UpdatedAt datetime NN; UpdatedByUserName nvarchar(450); RequestIp nvarchar(64); StatusNote nvarchar(50) |
| `dbo.VBSParticipationSurveyGuideUserState` | 0 | Id | Id bigint IDENTITY NN; UserName nvarchar(256) NN; NextShowAt datetime; UpdatedAt datetime NN |
| `dbo.VBSParticipationSurveySurveyFinal` | 7 | UserName | UserName nvarchar(100) NN; FinalMode nvarchar(20) NN; FinalAt datetime NN; RequestIp nvarchar(64); CompletedAt datetime |
| `dbo.VerseProgress` | 8077 | Id | Id int IDENTITY NN; BibleText nvarchar(100) NN; Jang int NN; Zul int NN; IsRead bit NN; ReadDate datetime |
| `dbo.WeeklyWordToRead` | 51 |  | Id int IDENTITY NN; WeekTitle_DW int; WeekTitle_GUA int; BibleTitle nvarchar(200); OnePoint nvarchar(200); LearningObjectives1 nvarchar(300); LearningObjectives2 nvarchar(300); LearningObjectives3 nvarchar(300); BibleTextToRead nvarchar(200); BibleTextToReadFromJang int; BibleTextToReadFromzul int; BibleTextToReadToJang int; BibleTextToReadToZul int; BibleTextToRecite nvarchar(200); BibleTextToReciteFromJang int; BibleTextToReciteFromzul int; BibleTextToReciteToJang int; BibleTextToReciteTozul int; YoJulSongURL nvarchar(200); YoJulSongText nvarchar(200); verseText1 nvarchar(200); verseText2 nvarchar(200); verseText3 nvarchar(200); verseText4 nvarchar(200); verseText5 nvarchar(200); WelcomeA nvarchar(200); WelcomeB nvarchar(200); Adventure nvarchar(200); ValueData1 nvarchar(200); ValueData2 nvarchar(200); ValueData3 nvarchar(200); EncourageStudyPoint1 nvarchar(200); EncourageStudyPoint2 nvarchar(200); ThisWeekMission nvarchar(200); DateFrom datetime; DateTo datetime; InsertDT datetime; updateDT datetime; publishYn nvarchar(1); relativeImagePath1 nvarchar(100); relativeImagePath2 nvarchar(100); relativeImagePath3 nvarchar(100); visited int; WelcomeTeacherGuide nvarchar(max); AdventureTeacherGuide nvarchar(max); ValueTeacherGuide nvarchar(max); EncourageTeacherGuide nvarchar(max); BibleTextToReadFromJang_1 int; BibleTextToReadFromzul_1 int; BibleTextToReadToJang_1 int; BibleTextToReadToZul_1 int; BibleTextToRead_1 nvarchar(200); CharcaterMovingURL nvarchar(200); CharcaterMovingURLText nvarchar(200); Charcatercard1Text nvarchar(200); Charcatercard2Text nvarchar(200); Charcatercard3Text nvarchar(200); ReadAndWriteOneWord nvarchar(200); PrayText nvarchar(max); cYear int |
| `dbo.WeeklyWordToRead_2025` | 70 |  | Id int IDENTITY NN; WeekTitle_DW int; WeekTitle_GUA int; BibleTitle nvarchar(200); OnePoint nvarchar(200); LearningObjectives1 nvarchar(300); LearningObjectives2 nvarchar(300); LearningObjectives3 nvarchar(300); BibleTextToRead nvarchar(200); BibleTextToReadFromJang int; BibleTextToReadFromzul int; BibleTextToReadToJang int; BibleTextToReadToZul int; BibleTextToRecite nvarchar(200); BibleTextToReciteFromJang int; BibleTextToReciteFromzul int; BibleTextToReciteToJang int; BibleTextToReciteTozul int; YoJulSongURL nvarchar(200); YoJulSongText nvarchar(200); verseText1 nvarchar(200); verseText2 nvarchar(200); verseText3 nvarchar(200); verseText4 nvarchar(200); verseText5 nvarchar(200); WelcomeA nvarchar(200); WelcomeB nvarchar(200); Adventure nvarchar(200); ValueData1 nvarchar(200); ValueData2 nvarchar(200); ValueData3 nvarchar(200); EncourageStudyPoint1 nvarchar(200); EncourageStudyPoint2 nvarchar(200); ThisWeekMission nvarchar(200); DateFrom datetime; DateTo datetime; InsertDT datetime; updateDT datetime; publishYn nvarchar(1); relativeImagePath1 nvarchar(100); relativeImagePath2 nvarchar(100); relativeImagePath3 nvarchar(100); visited int; WelcomeTeacherGuide nvarchar(max); AdventureTeacherGuide nvarchar(max); ValueTeacherGuide nvarchar(max); EncourageTeacherGuide nvarchar(max); BibleTextToReadFromJang_1 int; BibleTextToReadFromzul_1 int; BibleTextToReadToJang_1 int; BibleTextToReadToZul_1 int; BibleTextToRead_1 nvarchar(200); CharcaterMovingURL nvarchar(200); CharcaterMovingURLText nvarchar(200); Charcatercard1Text nvarchar(200); Charcatercard2Text nvarchar(200); Charcatercard3Text nvarchar(200); ReadAndWriteOneWord nvarchar(200); PrayText nvarchar(max); cYear int |
| `dbo.WeeklyWordToRead_DanWonTitle` | 12 |  | Id int IDENTITY NN; WeekTitle_DW int; WeekTitle_DW_Title nvarchar(100); InsertDT datetime; updateDT datetime |
| `dbo.WordAliases` | 0 | Id | Id int IDENTITY NN; SessionId int; FromText nvarchar(100) NN; ToText nvarchar(100) NN |
| `dbo.WorldviewActiveCourse` | 1 | Id | Id int NN; ActiveLessonNo int NN; UpdatedDT datetime2 NN; UpdatedBy nvarchar(100) |
| `dbo.WorldviewLessonBlank` | 4 | BlankId | BlankId int IDENTITY NN; LessonNo int NN; BlankKey nvarchar(50) NN; AnswerText nvarchar(20) NN; HintText nvarchar(50); SortNo int NN; IsActive bit NN |
| `dbo.WorldviewLessonBlankChar` | 180 | BlankCharId | BlankCharId int IDENTITY NN; LessonNo int NN; GroupKey nvarchar(50) NN; CharIndex int NN; AnswerChar nchar(1) NN; IsActive bit NN; InitialChar nvarchar(1) |
| `dbo.WorldviewLessonBlankSubmit` | 2 | SubmitId | SubmitId bigint IDENTITY NN; LessonNo int NN; UserId nvarchar(100); SubmitJson nvarchar(max) NN; IsCorrect bit NN; InsertDT datetime2 NN |
| `dbo.WorldviewLessonContent` | 1033 | ContentId | ContentId int IDENTITY NN; LessonNo int NN; SectionKey nvarchar(50) NN; ContentText nvarchar(max); SortNo int NN; IsActive bit NN; UpdateDT datetime2 NN |
| `dbo.WorldviewLessonSubmitHistory` | 353 | SubmitId | SubmitId int IDENTITY NN; LessonNo int NN; UserId nvarchar(100); UserName nvarchar(100); IsAllCorrect bit NN; BasePoint int NN; BonusPoint int NN; TotalPoint int NN; SubmittedAt datetime2 NN; CheckedAt datetime2; FinalSubmittedAt datetime2 |
| `dbo.YoutubeShorts` | 15 |  | Id int IDENTITY NN; contentsURL nvarchar(200); contentsText nvarchar(200); InsertDT datetime NN; updateDT datetime |
| `fit.ExerciseChips` | 96 | ChipId | ChipId int IDENTITY NN; ExerciseDocId int NN; ChipText nvarchar(200) NN; SortOrder int NN |
| `fit.ExerciseDoc` | 30 | ExerciseDocId | ExerciseDocId int IDENTITY NN; Slug nvarchar(120); TitleKo nvarchar(200) NN; TitleEn nvarchar(200); Subtitle nvarchar(200); Audience nvarchar(100); LevelText nvarchar(100); Keywords nvarchar(300); Description nvarchar(600); CreatedAt datetime2 NN; UpdatedAt datetime2 NN; Visited int |
| `fit.Illustrations` | 32 | IllustrationId | IllustrationId int IDENTITY NN; ExerciseDocId int NN; Url nvarchar(600) NN; AltText nvarchar(300); SortOrder int NN |
| `fit.NavigationLinks` | 64 | NavLinkId | NavLinkId int IDENTITY NN; ExerciseDocId int NN; Label nvarchar(120) NN; Href nvarchar(600) NN; StyleHint nvarchar(120); SortOrder int NN |
| `fit.PrescriptionRows` | 160 | PrescriptionRowId | PrescriptionRowId int IDENTITY NN; PrescriptionId int NN; LabelText nvarchar(80) NN; ValueText nvarchar(400) NN; SortOrder int NN |
| `fit.Prescriptions` | 32 | PrescriptionId | PrescriptionId int IDENTITY NN; SectionId int NN; Note nvarchar(400) |
| `fit.SectionListItems` | 502 | ListItemId | ListItemId int IDENTITY NN; SectionId int NN; ItemType nvarchar(20) NN; Content nvarchar(max) NN; SortOrder int NN |
| `fit.Sections` | 287 | SectionId | SectionId int IDENTITY NN; ExerciseDocId int NN; SectionType nvarchar(30) NN; Eyebrow nvarchar(60); Title nvarchar(200); Subtitle nvarchar(200); Body nvarchar(max); SortOrder int NN |
| `fit.SeoMeta` | 32 | SeoMetaId | SeoMetaId int IDENTITY NN; ExerciseDocId int NN; HtmlTitle nvarchar(200) NN; MetaDescription nvarchar(500) |
| `Identity.Role` | 4 | Id | Id nvarchar(450) NN; Name nvarchar(256); NormalizedName nvarchar(256); ConcurrencyStamp nvarchar(max) |
| `Identity.RoleClaims` | 0 | Id | Id int IDENTITY NN; RoleId nvarchar(450) NN; ClaimType nvarchar(max); ClaimValue nvarchar(max) |
| `Identity.User` | 206 | Id | Id nvarchar(450) NN; UserName nvarchar(256); NormalizedUserName nvarchar(256); Email nvarchar(256); NormalizedEmail nvarchar(256); EmailConfirmed bit NN; PasswordHash nvarchar(max); SecurityStamp nvarchar(max); ConcurrencyStamp nvarchar(max); PhoneNumber nvarchar(max); PhoneNumberConfirmed bit NN; TwoFactorEnabled bit NN; LockoutEnd datetimeoffset; LockoutEnabled bit NN; AccessFailedCount int NN; FirstName nvarchar(max); LastName nvarchar(max); ProfilePicture varbinary; UsernameChangeLimit int NN; insertDT datetime NN |
| `Identity.User_delete` | 3 |  | Id nvarchar(450) NN; UserName nvarchar(256); NormalizedUserName nvarchar(256); Email nvarchar(256); NormalizedEmail nvarchar(256); EmailConfirmed bit NN; PasswordHash nvarchar(max); SecurityStamp nvarchar(max); ConcurrencyStamp nvarchar(max); PhoneNumber nvarchar(max); PhoneNumberConfirmed bit NN; TwoFactorEnabled bit NN; LockoutEnd datetimeoffset; LockoutEnabled bit NN; AccessFailedCount int NN; FirstName nvarchar(max); LastName nvarchar(max); ProfilePicture varbinary; UsernameChangeLimit int NN; insertDT datetime NN |
| `Identity.UserClaims` | 0 | Id | Id int IDENTITY NN; UserId nvarchar(450) NN; ClaimType nvarchar(max); ClaimValue nvarchar(max) |
| `Identity.UserLogins` | 0 | LoginProvider,ProviderKey | LoginProvider nvarchar(450) NN; ProviderKey nvarchar(450) NN; ProviderDisplayName nvarchar(max); UserId nvarchar(450) NN |
| `Identity.UserRoles` | 233 | UserId,RoleId | UserId nvarchar(450) NN; RoleId nvarchar(450) NN |
| `Identity.UserTokens` | 0 | UserId,LoginProvider,Name | UserId nvarchar(450) NN; LoginProvider nvarchar(450) NN; Name nvarchar(450) NN; Value nvarchar(max) |

## 뷰 (30)

- `dbo.AllClickTables`
- `dbo.bibleMissing`
- `dbo.vierw_AttendanceCheck_2026`
- `dbo.view_All_Person_2026`
- `dbo.view_all_point_info`
- `dbo.view_AttendanceNewComer`
- `dbo.view_AwardPayouts_WithTotal`
- `dbo.view_BibleText_KO_EN`
- `dbo.view_click_data_all_person`
- `dbo.view_EnglishBibleQuiz_Rank`
- `dbo.view_everyBible`
- `dbo.view_KO_EN_SU`
- `dbo.view_loginHistory_groupBy`
- `dbo.view_PersonMaster`
- `dbo.view_PersonMaster_2026`
- `dbo.view_Ranking_By_Person`
- `dbo.view_Ranking_By_Person_ext`
- `dbo.view_Read_bible_Details`
- `dbo.view_TodayWordToRead_eBibleYouTuBu`
- `dbo.view_TodayWordToRead_eBibleYouTuBu2`
- `dbo.view_TodayWordToRead_eBibleYouTuBu2_BACKUP`
- `dbo.view_TodayWordToRead_Ext`
- `dbo.view_Total_Summary_Point`
- `dbo.view_UserBibleProgress`
- `dbo.view_UserCompositeInfo_2025`
- `dbo.view_UserCompositeInfo_2026`
- `dbo.view_UserCompositeInfo_2026_N`
- `dbo.view_UserCompositeInfo_2026old`
- `dbo.vw_DM2_ActiveUsers`
- `dbo.vw_exercise_list`

## 저장 프로시저 (50)

- `dbo.BackupClickHistoryTables`
- `dbo.DeleteBoardHelpDeskWithComments`
- `dbo.DeleteBoardWithComments`
- `dbo.DeleteExpiredUserSettingsCookies`
- `dbo.InsertCalendarData`
- `dbo.InsertPopMessagesForAllUsers`
- `dbo.InsertSilmulPrayMonthlyDates`
- `dbo.InsertTodayWordToReadMonthlyDates`
- `dbo.sp_InsertPrayPhrase`
- `dbo.sp_Upsert_ClickHistories`
- `dbo.Sync_ClickHistory_Prays_ByDate`
- `dbo.TransferData`
- `dbo.Update_eBibleEN_From_NIV`
- `dbo.UpdateBessingHusbandPrayTitle`
- `dbo.UpdateBessingWifePrayTitle`
- `dbo.UpdateDayN`
- `dbo.UpdateNextRecordUpdateDT`
- `dbo.UpdatePrayPhraseTemplateByExposeDate`
- `dbo.UpdatePreviousRecordUpdateDT`
- `dbo.UpdateQuestionBankData`
- `dbo.UpdateSeqqValues`
- `dbo.UpdateUserBibleProgressByTodayWord`
- `dbo.usp_ClearTable`
- `dbo.usp_Compare_ClickHistoryForPrayPhrase`
- `dbo.usp_Compare_ClickHistoryForQuestion`
- `dbo.usp_Compare_ClickHistoryForTodayWordToRead`
- `dbo.usp_Compare_ClickHistoryForWeeklyWordToRead`
- `dbo.usp_copy_AllClickHistories_fromAtoB`
- `dbo.usp_copy_ClickHistoryForPrayPhrase_fromAtoB`
- `dbo.usp_copy_ClickHistoryForQuestion_fromAtoB`
- `dbo.usp_copy_ClickHistoryForTodayWordToRead_fromAtoB`
- `dbo.usp_copy_ClickHistoryForWeeklyWordToRead_fromAtoB`
- `dbo.usp_DM2_GetAwardDashboard`
- `dbo.usp_DM2_GetAwardDashboard2`
- `dbo.usp_DM2_GetAwardDashboardTargets`
- `dbo.usp_DM2_GetMyAwards`
- `dbo.usp_DM2_MarkAwardDelivered`
- `dbo.usp_DM2_SyncAwardUserClaim`
- `dbo.usp_missions_InsertClickHistoryForPrayPhrase`
- `dbo.usp_missions_InsertClickHistoryForQuestion`
- `dbo.usp_missions_InsertClickHistoryForTodayWordToRead`
- `dbo.usp_missions_InsertClickHistoryForWeeklyWordToRead`
- `dbo.usp_OneLineFamousWord_updateDT`
- `dbo.usp_SyncPersonMaster2026_UserName_FromIdentityUser`
- `dbo.usp_SyncUserPhoneNumber_FromPersonMaster2026`
- `dbo.usp_UpsertUserBibleProgressByNameAndBibleText`
- `dbo.usp_user_click_history`
- `dbo.usp_user_click_history_byDate`
- `dbo.Worldview_GetActiveLessonNo`
- `dbo.Worldview_SetActiveLessonNo`

## 외래 키 (65)

- `dbo.Answers.QuestionId -> dbo.Questions.Id`
- `dbo.Answers.ParticipantId -> dbo.Participants.Id`
- `dbo.AnswerTokens.AnswerId -> dbo.Answers.Id`
- `dbo.AspNetRoleClaims.RoleId -> dbo.AspNetRoles.Id`
- `dbo.AspNetUserClaims.UserId -> dbo.AspNetUsers.Id`
- `dbo.AspNetUserLogins.UserId -> dbo.AspNetUsers.Id`
- `dbo.AspNetUserRoles.UserId -> dbo.AspNetUsers.Id`
- `dbo.AspNetUserRoles.RoleId -> dbo.AspNetRoles.Id`
- `dbo.AspNetUserTokens.UserId -> dbo.AspNetUsers.Id`
- `dbo.AttendanceNewComerVisit.NewComerId -> dbo.AttendanceNewComer.Id`
- `dbo.BandAttendancePostComments.BandPostId -> dbo.BandPosts.Id`
- `dbo.BandJuboPostComments.BandPostId -> dbo.BandPosts.Id`
- `dbo.BandJuboPostImages.BandPostId -> dbo.BandJuboPosts.Id`
- `dbo.BandPostComments.BandPostId -> dbo.BandPosts.Id`
- `dbo.BandPostImages.BandPostId -> dbo.BandPosts.Id`
- `dbo.BirthdayRecords.BulletinId -> dbo.Bulletins.Id`
- `dbo.BirthdayRecords.DepartmentId -> dbo.ChurchDepartments.Id`
- `dbo.Bulletins.ChurchId -> dbo.Churches.Id`
- `dbo.Bulletins.DepartmentId -> dbo.ChurchDepartments.Id`
- `dbo.ChurchDepartments.ChurchId -> dbo.Churches.Id`
- `dbo.CommentAnonymous.BoardId -> dbo.BoardAnonymous.Id`
- `dbo.CommentAnonymous.ParentCommentId -> dbo.CommentAnonymous.Id`
- `dbo.CommentHelpDesk.BoardId -> dbo.BoardHelpDesk.Id`
- `dbo.CommentHelpDesk.ParentCommentId -> dbo.CommentHelpDesk.Id`
- `dbo.DM2_FamilyMember.FamilyTeamId -> dbo.DM2_FamilyTeam.FamilyTeamId`
- `dbo.DM2_FamilyMilestoneHidden.MilestoneConfigId -> dbo.DM2_FamilyMilestoneConfig.Id`
- `dbo.exercise_Audience.ExerciseId -> dbo.exercise_Exercise.ExerciseId`
- `dbo.exercise_Category_Map.ExerciseId -> dbo.exercise_Exercise.ExerciseId`
- `dbo.exercise_Category_Map.CategoryId -> dbo.exercise_Category.CategoryId`
- `dbo.exercise_Effect_Map.ExerciseId -> dbo.exercise_Exercise.ExerciseId`
- `dbo.exercise_Effect_Map.EffectId -> dbo.exercise_Effect.EffectId`
- `dbo.exercise_Equipment_Map.ExerciseId -> dbo.exercise_Exercise.ExerciseId`
- `dbo.exercise_Equipment_Map.EquipmentId -> dbo.exercise_Equipment.EquipmentId`
- `dbo.exercise_Muscle_Map.ExerciseId -> dbo.exercise_Exercise.ExerciseId`
- `dbo.exercise_Muscle_Map.MuscleId -> dbo.exercise_Muscle.MuscleId`
- `dbo.exercise_Note.ExerciseId -> dbo.exercise_Exercise.ExerciseId`
- `dbo.exercise_Prescription.ExerciseId -> dbo.exercise_Exercise.ExerciseId`
- `dbo.exercise_Step.ExerciseId -> dbo.exercise_Exercise.ExerciseId`
- `dbo.exercise_Variation.ExerciseId -> dbo.exercise_Exercise.ExerciseId`
- `dbo.LostAndFoundPostComments.BandPostId -> dbo.LostAndFoundPosts.Id`
- `dbo.LostAndFoundPostImages.BandPostId -> dbo.LostAndFoundPosts.Id`
- `dbo.MemoryVerses.BulletinId -> dbo.Bulletins.Id`
- `dbo.NoticeAttachment.NoticeId -> dbo.Notice.Id`
- `dbo.Offerings.BulletinId -> dbo.Bulletins.Id`
- `dbo.Participants.SessionId -> dbo.Sessions.Id`
- `dbo.Photos.AlbumId -> dbo.Albums.Id`
- `dbo.QuestionOptions.QuestionId -> dbo.Questions.Id`
- `dbo.Questions.SessionId -> dbo.Sessions.Id`
- `dbo.StudentPointGiftOrder.ProductID -> dbo.AwardItem.ProductID`
- `dbo.SundaySchoolJuboBlock.JuboId -> dbo.SundaySchoolJubo.Id`
- `dbo.UserAwardOrder.ProductID -> dbo.AwardItem.ProductID`
- `fit.ExerciseChips.ExerciseDocId -> fit.ExerciseDoc.ExerciseDocId`
- `fit.Illustrations.ExerciseDocId -> fit.ExerciseDoc.ExerciseDocId`
- `fit.NavigationLinks.ExerciseDocId -> fit.ExerciseDoc.ExerciseDocId`
- `fit.PrescriptionRows.PrescriptionId -> fit.Prescriptions.PrescriptionId`
- `fit.Prescriptions.SectionId -> fit.Sections.SectionId`
- `fit.SectionListItems.SectionId -> fit.Sections.SectionId`
- `fit.Sections.ExerciseDocId -> fit.ExerciseDoc.ExerciseDocId`
- `fit.SeoMeta.ExerciseDocId -> fit.ExerciseDoc.ExerciseDocId`
- `Identity.RoleClaims.RoleId -> Identity.Role.Id`
- `Identity.UserClaims.UserId -> Identity.User.Id`
- `Identity.UserLogins.UserId -> Identity.User.Id`
- `Identity.UserRoles.UserId -> Identity.User.Id`
- `Identity.UserRoles.RoleId -> Identity.Role.Id`
- `Identity.UserTokens.UserId -> Identity.User.Id`
