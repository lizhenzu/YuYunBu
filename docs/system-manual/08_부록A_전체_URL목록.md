# 08. 부록 A — 전체 주소(URL) 목록

> 컨트롤러 소스에서 뽑은 액션 전체다. 주소는 기본 경로 `/{컨트롤러}/{액션}/{id?}` 를 따른다(별도 `Route` 가 있으면 표시).
> **권한** 칸: 컨트롤러 전체에 붙은 것(클래스) / 액션에 붙은 것. `AllowAnonymous` = 로그인 없이 가능. `LogAction` = 접속 기록 필터.
> 같은 이름이 두 번 나오면 GET(화면)과 POST(저장) 한 쌍이다.


## Account
클래스: `[Authorize]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Account/Login` | HttpGet,AllowAnonymous | string returnUrl = null |
| `/Account/Login` | HttpPost,AllowAnonymous,ValidateAntiForgeryToken | LoginViewModel model, string returnUrl = null |
| `/Account/Register` | HttpGet,AllowAnonymous | string returnUrl = null |
| `/Account/Register` |  | RegisterViewModel model, string returnUrl = null |
| `/Account/Logout` | HttpPost,ValidateAntiForgeryToken |  |

## AddressBook
클래스: `[Authorize]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/AddressBook/Index` |  |  |
| `/AddressBook/Branch` |  |  |
| `/AddressBook/Search` | HttpGet | string q, int take = 30 |

## Admin
클래스: `[Authorize]  [LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Admin/Index` |  |  |
| `/Admin/LifeGuide` |  |  |
| `/Admin/SendBirthMessage` | AllowAnonymous,Route("Admin/SendBirthMessage"),Route("Admin/SendBirthMessage.html") |  |
| `/Admin/evangelism_reward_guide_v1` |  |  |
| `/Admin/IndexPoint` |  |  |
| `/Admin/TeacherOnly` | Authorize(Roles = "Admin,SuperAdmin,Moderator") |  |
| `/Admin/PrintManage` | Authorize(Roles = "Admin,SuperAdmin,Moderator") |  |
| `/Admin/PrintByGrade` | Authorize(Roles = "Admin,SuperAdmin,Moderator") | int grade |
| `/Admin/_DebugRoles` | Authorize |  |

## AIExperience
클래스: `[Authorize]  [LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/AIExperience/Index` |  |  |
| `/AIExperience/ChatWithAI` |  | string meth |
| `/AIExperience/AIAskHistory` |  |  |
| `/AIExperience/WhatAI` | AllowAnonymous |  |

## Albums
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Albums/Index` |  |  |
| `/Albums/Branch` |  |  |
| `/Albums/Create` | Authorize |  |
| `/Albums/Create` | HttpPost,ValidateAntiForgeryToken,Authorize | string name, string description |
| `/Albums/UpdateAlbum` | HttpPost,ValidateAntiForgeryToken,Authorize | int id, string name, string description |
| `/Albums/DeletePhotosApi` | HttpPost,ValidateAntiForgeryToken,Authorize | int albumId, int[] photoIds |
| `/Albums/SetCoverApi` | HttpPost,ValidateAntiForgeryToken,Authorize | int albumId, int photoId |
| `/Albums/Details` |  | int id |
| `/Albums/MyPhotoPick` |  |  |
| `/Albums/Upload` | HttpPost,ValidateAntiForgeryToken,Authorize | int id, List<Microsoft.AspNetCore.Http.IFormFile> files |
| `/Albums/DeletePhoto` | HttpPost,ValidateAntiForgeryToken,Authorize | int photoId, int albumId |
| `/Albums/Delete` | HttpPost,ValidateAntiForgeryToken,Authorize | int id |
| `/Albums/SetCover` | HttpPost,ValidateAntiForgeryToken,Authorize | int albumId, int photoId |
| `/Albums/DownloadAlbum` |  | int id |
| `/Albums/DeleteSelected` | HttpPost,Route("Albums/DeleteSelected"),ValidateAntiForgeryToken,Authorize | int albumId, int[] photoIds |
| `/Albums/DownloadSelected` | HttpPost,ValidateAntiForgeryToken | int albumId, int[] photoIds |
| `/Albums/UploadAjax` | HttpPost,ValidateAntiForgeryToken,Authorize | int id, Microsoft.AspNetCore.Http.IFormFile file |

## ApplyEvent
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/ApplyEvent/Index` |  |  |
| `/ApplyEvent/Branch` |  |  |
| `/ApplyEvent/ApplyList` |  | int EventId |
| `/ApplyEvent/EventList` |  |  |
| `/ApplyEvent/CancelEnroll` | Authorize | string UserId, int EventId |
| `/ApplyEvent/EnrollEventApplyInfo` | Authorize | int EventId |

## Attendance
클래스: `[Authorize]  [LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Attendance/Index` | HttpGet | DateTime? date |
| `/Attendance/More` | HttpGet |  |
| `/Attendance/LongAbsent` | HttpGet,Authorize(Roles = nameof(BulletinBoard.Enums.Roles.Admin) + "," + nameof(BulletinBoard.Enums.Roles.SuperAdmin)) | [FromQuery] int absentDays = 90 |
| `/Attendance/UploadNewComerPhoto` | HttpPost,ValidateAntiForgeryToken | int newComerId, string? croppedImageData |
| `/Attendance/AdminStudentMatrixWeeks` | HttpGet | DateTime? date, int weeks = 2 |
| `/Attendance/StatisticsDetail` | HttpGet | int? year, int? month |
| `/Attendance/AdminStudentRank3Weeks` | HttpGet | DateTime? date |
| `/Attendance/Statistics` | HttpGet | int? year, int? month |
| `/Attendance/Check` | HttpGet | DateTime? date, int classYear = 0, int classBan = 0 |
| `/Attendance/TeacherAssign` | HttpGet,Authorize |  |
| `/Attendance/TeacherAssign` | HttpPost,Authorize,ValidateAntiForgeryToken | TeacherAssignPost req |
| `/Attendance/SavePresent` | HttpPost,Authorize | [FromBody] SavePresentRequest req |
| `/Attendance/Dashboard` | HttpGet | DateTime? date, int classYear = 0, int classBan = 0 |
| `/Attendance/DashboardAll` | HttpGet | DateTime? date |
| `/Attendance/AdminTrendSundayDetail` | HttpGet | string sunday |
| `/Attendance/AdminTrend3Weeks` | HttpGet | DateTime? date |
| `/Attendance/SundayStudentsPC` | HttpGet | string sunday |
| `/Attendance/SundayStudentsMobile` | HttpGet | string sunday |
| `/Attendance/NewComer` | HttpGet | string? keyword = "" |
| `/Attendance/NewComerDetail` | HttpGet | int id |
| `/Attendance/SaveNewComer` | HttpPost,ValidateAntiForgeryToken | NewComerSaveModel model |
| `/Attendance/AddNewComerVisitToday` | HttpPost,ValidateAntiForgeryToken | int newComerId |
| `/Attendance/CancelNewComerVisitToday` | HttpPost,ValidateAntiForgeryToken | int newComerId |
| `/Attendance/AddNewComerVisitByDate` | HttpPost,ValidateAntiForgeryToken | NewComerVisitSaveModel model |
| `/Attendance/DeleteNewComer` | HttpPost,ValidateAntiForgeryToken | int id |
| `/Attendance/UpdateNewComerInfo` | HttpPost,ValidateAntiForgeryToken | NewComerInfoEditModel model |
| `/Attendance/DeleteNewComerVisit` | HttpPost,ValidateAntiForgeryToken | int id, int newComerId |

## BandBoard
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/BandBoard/LoadMore` |  | string boardTableName, string boardName, int page |
| `/BandBoard/Create` | HttpGet,Authorize | string boardTableName = "BandPosts", string boardName = "게시판" |
| `/BandBoard/Like` | HttpPost | string boardTableName, int id |
| `/BandBoard/AddComment` | HttpPost | string boardTableName, int postId, string writerName, string content |
| `/BandBoard/Delete` | HttpPost,ValidateAntiForgeryToken,Authorize | string boardTableName, string boardName, int id |

## Base

| 주소 | 속성 | 매개변수 |
|---|---|---|

## BessingACouple
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/BessingACouple/Index` |  |  |
| `/BessingACouple/DetailWords` |  | string Kind, int Id, string ExposeDate |
| `/BessingACouple/DeleteView` | Authorize(Roles = nameof(Roles.SuperAdmin)) | string Kind, int Id |
| `/BessingACouple/DecreasetHitACouple` | Authorize(Roles = nameof(Roles.SuperAdmin)) | string Kind, int Id |
| `/BessingACouple/ModifyView` | Authorize | string Kind,int Id |
| `/BessingACouple/EnrollView` | Authorize | string Kind |
| `/BessingACouple/ModifyProcess` | HttpPost,Authorize,ValidateAntiForgeryToken | string Kind, BessingACoupleData param |
| `/BessingACouple/EnrollProcess` | HttpPost,Authorize,ValidateAntiForgeryToken | string Kind, BessingACoupleData param |
| `/BessingACouple/DetailList` |  | string Kind |
| `/BessingACouple/Verify` | HttpPost,Authorize,ValidateAntiForgeryToken | string Kind, int Id |
| `/BessingACouple/Unverify` | HttpPost,Authorize,ValidateAntiForgeryToken | string Kind, int Id |

## BessingChild
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/BessingChild/Index` |  |  |
| `/BessingChild/DetailWords` |  | int Id, string ExposeDate, string forWhom |
| `/BessingChild/DeleteView` |  | int Id |
| `/BessingChild/DecreasetHitWeeklyWordToRead` |  | int Id |
| `/BessingChild/ModifyView` |  | int Id |
| `/BessingChild/EnrollView` |  |  |
| `/BessingChild/ModifyProcess` |  | BessingChildData param |
| `/BessingChild/EnrollProcess` |  | BessingChildData param |
| `/BessingChild/DetailList` |  |  |

## Bible2
클래스: `[Authorize]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Bible2/Welcome` | HttpGet |  |
| `/Bible2/HubClick` | HttpPost,ValidateAntiForgeryToken | string v |
| `/Bible2/Participants` | HttpGet |  |
| `/Bible2/Index` |  |  |
| `/Bible2/List` |  | string testament = "구약" |
| `/Bible2/Read` |  | string bookName, int jang |
| `/Bible2/Search` | AllowAnonymous | string? q |
| `/Bible2/SearchMore` | HttpGet,AllowAnonymous | string q, int offset = 0 |
| `/Bible2/Bookmarks` |  |  |
| `/Bible2/ToggleChapter` | HttpPost | string bookName, int jang |
| `/Bible2/ToggleBook` | HttpPost | string bookName |
| `/Bible2/ToggleVerse` | HttpPost | int verseId |
| `/Bible2/MarkChapterRead` | HttpPost | string bookName, int jang |
| `/Bible2/ToggleBookmark` | HttpPost | int verseId |
| `/Bible2/ResetProgress` | HttpGet | string userId |
| `/Bible2/ResetProgress` | HttpPost,ValidateAntiForgeryToken | string userId, [FromForm] string dummy = "" |
| `/Bible2/Error` |  |  |

## BibleBulk
클래스: `[Authorize(Roles = nameof(Roles.SuperAdmin))]  [LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/BibleBulk/Index` |  |  |
| `/BibleBulk/IndexEN` |  |  |
| `/BibleBulk/IndexEN_NIV` |  | string BibleText_K, int Jang_K |
| `/BibleBulk/EnrollBulkQuiz` |  | string screenName |
| `/BibleBulk/EnrollView` |  | string BibleText, int chapter, string LangCD |
| `/BibleBulk/BibleSheet2` | AllowAnonymous | string BibleText, int Jang, string langCD |
| `/BibleBulk/BibleEnrollList` |  | string screenName |
| `/BibleBulk/BibleBasicList` |  | string screenName |
| `/BibleBulk/BibleDetailList` |  | string BibleText, int Jang |
| `/BibleBulk/DeleteQuestionBank` |  | int Id, string BibleText, int Jang, int MaxJang |
| `/BibleBulk/BibleQuizEnrollList` |  | string BibleText, int Jang, int MaxJang |
| `/BibleBulk/BibleNullList` |  | string BibleText, int Jang |
| `/BibleBulk/BibleMissingList` |  | string screenName |
| `/BibleBulk/BibleMissingListByLang` |  | string screenName, string KO_BibleText |
| `/BibleBulk/BibleMissingListB_KO_EN_SU` |  |  |
| `/BibleBulk/BibleMissingListB_SU` |  | string BibleText |
| `/BibleBulk/SyncENtoNIC` |  |  |
| `/BibleBulk/BibleMissingListB_KJV` |  | string BibleText |
| `/BibleBulk/BibleMissingList_NIV` |  | string BibleText |
| `/BibleBulk/BibleMissingListByGroupByLang` |  | string screenName |
| `/BibleBulk/CreateBulkInputBible` | HttpPost,ValidateAntiForgeryToken | BibleBulk bibleBulk |
| `/BibleBulk/CreateBulkQuiz` | HttpPost,ValidateAntiForgeryToken | BibleBulkQuiz param |

## bibleCenter
클래스: `[Authorize]  [LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/bibleCenter/Index` |  |  |
| `/bibleCenter/DoneReadBibleOneVerse` | Authorize | string LangCD, int TopValue, int CurrntBibleID, int CurrntBibleIDLoop,… |
| `/bibleCenter/MyDashboard` |  | string userId |
| `/bibleCenter/bibleBookMarkDetail` |  | int VerseId |
| `/bibleCenter/bibleBookMark` |  |  |
| `/bibleCenter/DeleteBookMark` |  | string VerseId |
| `/bibleCenter/MyDashboardList` |  | string bilbeProcessOption |
| `/bibleCenter/MyDashboardList2` | AllowAnonymous |  |
| `/bibleCenter/BibleOneList` |  | string BibleText |
| `/bibleCenter/BibleAllList` |  |  |
| `/bibleCenter/ReadingTheBibleOneVerse` | Authorize | int BibleBookSerialId, int BibleId, int Jang, string LangCD, int Pages… |
| `/bibleCenter/IndexByAB` |  | string ab |
| `/bibleCenter/IndexByID` |  | int Id, string BibleText |
| `/bibleCenter/IndexByMap` |  | int Id, string BibleText |
| `/bibleCenter/HitBibleYouTuBuHit` |  | string videoId |
| `/bibleCenter/SaveOrUpdateBibleYouTuBu` | Authorize(Roles = nameof(Roles.SuperAdmin)) | string BibleText, int Jang |
| `/bibleCenter/ProcessSaveOrUpdateBibleYouTuBu` | HttpPost,Authorize(Roles = nameof(Roles.SuperAdmin)) | BibleYouTuBu bibleYouTuBu |

## Bible
클래스: `[Authorize]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Bible/Welcome` | HttpGet |  |
| `/Bible/Participants` | HttpGet |  |
| `/Bible/Index` |  |  |
| `/Bible/List` |  | string testament = "구약" |
| `/Bible/Read` |  | string bookName, int jang |
| `/Bible/Search` | AllowAnonymous | string? q |
| `/Bible/SearchMore` | HttpGet,AllowAnonymous | string q, int offset = 0 |
| `/Bible/Bookmarks` |  |  |
| `/Bible/ToggleChapter` | HttpPost | string bookName, int jang |
| `/Bible/ToggleBook` | HttpPost | string bookName |
| `/Bible/ToggleVerse` | HttpPost | int verseId |
| `/Bible/MarkChapterRead` | HttpPost | string bookName, int jang |
| `/Bible/ToggleBookmark` | HttpPost | int verseId |
| `/Bible/ResetProgress` | HttpGet | string userId |
| `/Bible/ResetProgress` | HttpPost,ValidateAntiForgeryToken | string userId, [FromForm] string dummy = "" |
| `/Bible/Error` |  |  |

## BibleGift
클래스: `[Authorize]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/BibleGift/Select` | AllowAnonymous,HttpGet | int round, bool view = false |
| `/BibleGift/Pick` | HttpPost,ValidateAntiForgeryToken | int round, int productId, string? returnTo = null |
| `/BibleGift/Cancel` | HttpPost,ValidateAntiForgeryToken | int round, string? returnTo = null |
| `/BibleGift/Admin` | HttpGet | string tab = "picked" |
| `/BibleGift/ForceDelete` | HttpPost,ValidateAntiForgeryToken | string userId, int round, string tab = "all" |

## Birthdays
클래스: `[Authorize]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Birthdays/Index` | HttpGet("") |  |
| `/Birthdays/NoBirthday` | HttpGet("NoBirthday") |  |
| `/Birthdays/Month` | HttpGet("Month") | [FromQuery] int month |
| `/Birthdays/UpdateGiftState` | HttpPost,ValidateAntiForgeryToken,Authorize(Roles = nameof(Roles.Admin) + "," + nameof(Roles.SuperAdmin)),HttpPost("UpdateGiftState") | UpdateGiftStateVM vm |
| `/Birthdays/ThisMonth` | HttpGet("ThisMonth") |  |
| `/Birthdays/Year` | HttpGet("Year") |  |
| `/Birthdays/Edit` | HttpGet("Edit/{personId:int}") | int personId |

## Board
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Board/Index` |  |  |
| `/Board/WhatAnonymous` |  |  |
| `/Board/IndexByBoard` |  | string boardName |
| `/Board/DeleteByID` |  | string boardName, int id |
| `/Board/Detail` |  | string boardName, int id |
| `/Board/Create` |  | string boardName |
| `/Board/Create2` |  | string boardName |
| `/Board/Results` |  | string boardName |
| `/Board/Create` | HttpPost | string boardName, [FromForm] Board board |
| `/Board/AddComment` | HttpPost | string boardName,int boardId, string content, string userName, int? pa… |

## ChallengeEnglishQuiz
클래스: `[Authorize]  [LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/ChallengeEnglishQuiz/Index` |  |  |
| `/ChallengeEnglishQuiz/RankList` |  |  |
| `/ChallengeEnglishQuiz/Intro` | AllowAnonymous |  |
| `/ChallengeEnglishQuiz/DetailView` |  |  |
| `/ChallengeEnglishQuiz/AnswerQuestion` |  | int Id,string selectedOption |
| `/ChallengeEnglishQuiz/DeleteView` |  | int Id |
| `/ChallengeEnglishQuiz/EditView` |  | int Id |
| `/ChallengeEnglishQuiz/EditViewProcess` |  | OpicData param |
| `/ChallengeEnglishQuiz/EnrollView` |  |  |
| `/ChallengeEnglishQuiz/EnrollViewProcess` |  | OpicData param |
| `/ChallengeEnglishQuiz/List` |  | string sortBy, string query |
| `/ChallengeEnglishQuiz/MarkView` |  | int Id |
| `/ChallengeEnglishQuiz/SearchQuery` |  | string query |

## CheckPoint
클래스: `[Authorize]  [LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/CheckPoint/Index` |  |  |
| `/CheckPoint/GetGroupByUsernameDataByClass` |  |  |
| `/CheckPoint/ChampionList` |  |  |
| `/CheckPoint/MismatchClickHistoryForQuestion` | Authorize(Roles = nameof(Roles.SuperAdmin)) |  |
| `/CheckPoint/CorrectMismatchClickHistoryForQuestion` | Authorize(Roles = nameof(Roles.SuperAdmin)) | int Id, int QuestionId, string UserName |
| `/CheckPoint/updateAwardPoint` | HttpPost | [FromBody] AwardUpdateData param |
| `/CheckPoint/GetThisWeekRewardingAward` |  | string viewOption |
| `/CheckPoint/AwardItemList` |  | string option, int myPoint, int ProductID = 0 |
| `/CheckPoint/AwardItemPick` |  | string UserId |
| `/CheckPoint/UpdateAwardItem` |  | int ProductID = 0 |
| `/CheckPoint/UpsertAwardItemPick` |  | string userId, int productId |
| `/CheckPoint/MyAwardItemPick` |  |  |
| `/CheckPoint/ProcessUpdateAwardItem` | HttpPost | AwardItem param |
| `/CheckPoint/ManagePoint` |  | string UserId, string rankQueryOption = "TeacherOnly", string orderBy … |
| `/CheckPoint/SetPoint` |  | string userName, int value |
| `/CheckPoint/CheckPointDetailList2` |  | string rankQueryOption = "TeacherOnly", string orderBy = "DC_Total" |
| `/CheckPoint/ChangeCheckPoint` |  | string userName, string optionData, int value=100 |
| `/CheckPoint/SetDept` |  |  |

## DailyEnglish

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/DailyEnglish/Index` |  | int? id |
| `/DailyEnglish/Progress` | HttpGet |  |
| `/DailyEnglish/MarkAsRead` | HttpPost | int id |
| `/DailyEnglish/Reset` | HttpPost |  |
| `/DailyEnglish/AdminList` |  |  |
| `/DailyEnglish/Create` |  |  |
| `/DailyEnglish/Create` | HttpPost,ValidateAntiForgeryToken | DailyEnglish item |
| `/DailyEnglish/Edit` |  | int id |
| `/DailyEnglish/Edit` | HttpPost,ValidateAntiForgeryToken | DailyEnglish item |
| `/DailyEnglish/Delete` | HttpPost,ValidateAntiForgeryToken | int id |
| `/DailyEnglish/Video` |  |  |
| `/DailyEnglish/VideoAdd` | HttpPost,ValidateAntiForgeryToken | string youTubeId, string title |
| `/DailyEnglish/VideoDelete` | HttpPost,ValidateAntiForgeryToken | int id |
| `/DailyEnglish/VideoPlay` |  | int id |

## DailyMission2Award
클래스: `[Authorize]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/DailyMission2Award/PickGifts` | HttpGet | int? year, int? startLevel |
| `/DailyMission2Award/PickGiftsSelect` | HttpPost,ValidateAntiForgeryToken | int cYear, int awardLevel, int productId |
| `/DailyMission2Award/PickGiftsDone` | HttpGet | int? year |
| `/DailyMission2Award/ResetGiftSelection` | HttpPost,ValidateAntiForgeryToken | int cYear, int awardLevel |
| `/DailyMission2Award/Dashboard` | HttpGet | int? year |
| `/DailyMission2Award/MarkDelivered` | HttpPost,ValidateAntiForgeryToken | string userName, int cYear, int awardLevel |
| `/DailyMission2Award/Dashboard2` | HttpGet | int? year |
| `/DailyMission2Award/MyAwards` | HttpGet |  |

## DailyMission2
클래스: `[Authorize]  [LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/DailyMission2/Participants` | HttpGet | int? year |
| `/DailyMission2/ParticipantPersons` | HttpGet | int? year |
| `/DailyMission2/ParticipantTeams` | HttpGet | int? year |
| `/DailyMission2/Welcome` | HttpGet |  |
| `/DailyMission2/HubClick` | HttpPost,ValidateAntiForgeryToken | string v |
| `/DailyMission2/Branch` | HttpGet |  |
| `/DailyMission2/Index` | HttpGet | string date, string lang = "" |
| `/DailyMission2/AwardPopupHide` | HttpPost,ValidateAntiForgeryToken | int days, string returnUrl = "" |
| `/DailyMission2/Word` | HttpGet | string date, string lang = "", int YearTotalPoint = 0 |
| `/DailyMission2/NaviMonth` |  | string UserName |
| `/DailyMission2/NaviHistory` |  | string UserName |
| `/DailyMission2/Quiz` | HttpGet | string date, string lang = "", int YearTotalPoint = 0 |
| `/DailyMission2/AnswerQuestion` | HttpPost,ValidateAntiForgeryToken | string date, int questionId, string answerQ, string lang = "" |
| `/DailyMission2/Pray` | HttpGet | string date, string lang = "", int YearTotalPoint = 0 |
| `/DailyMission2/Recite` | HttpGet | string date, string lang = "", int YearTotalPoint = 0 |
| `/DailyMission2/BibleRead` | HttpGet | string date, string lang = "" |
| `/DailyMission2/MarkVerseRead` | HttpPost,ValidateAntiForgeryToken | string date, string lang, string bibleText, int jang, int zul, int Id |
| `/DailyMission2/MarkVerseUnread` | HttpPost,ValidateAntiForgeryToken | string date, string lang, string bibleText, int jang, int zul, int Id |
| `/DailyMission2/ToggleBookmark` | HttpPost,ValidateAntiForgeryToken | string date, string lang, string bibleText, int jang, int zul, int Id |
| `/DailyMission2/ToggleBibleRange` | HttpPost,ValidateAntiForgeryToken | [FromBody] ToggleBibleRangeReq req |
| `/DailyMission2/GetBibleProgressJson` | HttpGet |  |
| `/DailyMission2/MarkPageRead` | HttpPost,ValidateAntiForgeryToken | string date, string lang, string bibleText, int fromJang, int fromZul,… |
| `/DailyMission2/SaveWord` | HttpPost,ValidateAntiForgeryToken | string date, int todayWordToReadId, bool toMe, bool toParent, string l… |
| `/DailyMission2/SaveRecite` | HttpPost,ValidateAntiForgeryToken | string date, int weeklyWordToReadId, string lang = "ko" |
| `/DailyMission2/UserMonthlyPoints` |  | string userName, int year = 0, int month = 0 |
| `/DailyMission2/UserDailyPoints` |  | string userName |
| `/DailyMission2/DeleteClickHistory` | HttpGet |  |

## DailyMission2FamilyAdmin
클래스: `[Authorize(Roles = nameof(BulletinBoard.Enums.Roles.SuperAdmin))]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/DailyMission2FamilyAdmin/Index` |  | int? teamId |
| `/DailyMission2FamilyAdmin/CreateTeam` | HttpPost | string teamName, string? memo |
| `/DailyMission2FamilyAdmin/UpdateTeam` | HttpPost | int teamId, string teamName, bool isActive, string? memo |
| `/DailyMission2FamilyAdmin/AddMember` | HttpPost | int teamId, string userName, int roleCode, int sortNo, int relationCod… |
| `/DailyMission2FamilyAdmin/UpdateMember` | HttpPost | int teamId, string userName, int roleCode, int sortNo, bool isActive, … |
| `/DailyMission2FamilyAdmin/DeleteMember` | HttpPost | int teamId, string userName |

## DailyMission4
클래스: `[Authorize]  [LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/DailyMission4/Participants` | HttpGet | int? year |
| `/DailyMission4/ParticipantPersons` | HttpGet | int? year |
| `/DailyMission4/ParticipantTeams` | HttpGet | int? year |
| `/DailyMission4/Branch` | HttpGet |  |
| `/DailyMission4/Index` | HttpGet | string date, string lang = "" |
| `/DailyMission4/AwardPopupHide` | HttpPost,ValidateAntiForgeryToken | int days, string returnUrl = "" |
| `/DailyMission4/Word` | HttpGet | string date, string lang = "", int YearTotalPoint = 0 |
| `/DailyMission4/NaviMonth` |  | string UserName |
| `/DailyMission4/NaviHistory` |  | string UserName |
| `/DailyMission4/Quiz` | HttpGet | string date, string lang = "", int YearTotalPoint = 0 |
| `/DailyMission4/AnswerQuestion` | HttpPost,ValidateAntiForgeryToken | string date, int questionId, string answerQ, string lang = "" |
| `/DailyMission4/Pray` | HttpGet | string date, string lang = "", int YearTotalPoint = 0 |
| `/DailyMission4/Recite` | HttpGet | string date, string lang = "", int YearTotalPoint = 0 |
| `/DailyMission4/BibleRead` | HttpGet | string date, string lang = "" |
| `/DailyMission4/MarkVerseRead` | HttpPost,ValidateAntiForgeryToken | string date, string lang, string bibleText, int jang, int zul, int Id |
| `/DailyMission4/MarkVerseUnread` | HttpPost,ValidateAntiForgeryToken | string date, string lang, string bibleText, int jang, int zul, int Id |
| `/DailyMission4/ToggleBookmark` | HttpPost,ValidateAntiForgeryToken | string date, string lang, string bibleText, int jang, int zul, int Id |
| `/DailyMission4/ToggleBibleRange` | HttpPost,ValidateAntiForgeryToken | [FromBody] ToggleBibleRangeReq req |
| `/DailyMission4/GetBibleProgressJson` | HttpGet |  |
| `/DailyMission4/MarkPageRead` | HttpPost,ValidateAntiForgeryToken | string date, string lang, string bibleText, int fromJang, int fromZul,… |
| `/DailyMission4/SaveWord` | HttpPost,ValidateAntiForgeryToken | string date, int todayWordToReadId, bool toMe, bool toParent, string l… |
| `/DailyMission4/SaveRecite` | HttpPost,ValidateAntiForgeryToken | string date, int weeklyWordToReadId, string lang = "ko" |
| `/DailyMission4/UserMonthlyPoints` |  | string userName, int year = 0, int month = 0 |
| `/DailyMission4/UserDailyPoints` |  | string userName |
| `/DailyMission4/DeleteClickHistory` | HttpGet |  |

## DevotionPoints
클래스: `[Authorize]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/DevotionPoints/Home` | HttpGet,Authorize(Roles = StaffRoles) | string? ym |
| `/DevotionPoints/StudentYear` | HttpGet,Authorize(Roles = StaffRoles) | int personId, int year |
| `/DevotionPoints/Adjust` | HttpPost,Authorize(Roles = StaffRoles) | [FromBody] AdjustReq req |
| `/DevotionPoints/SetMonthly` | HttpPost,Authorize(Roles = StaffRoles) | [FromBody] SetMonthlyReq req |
| `/DevotionPoints/GetGrid` | HttpGet,Authorize(Roles = StaffRoles) | int classYear, int classBan, string ym |
| `/DevotionPoints/Status` | HttpGet,Authorize(Roles = StaffRoles) | string? ym, int classYear = 0 |
| `/DevotionPoints/DashboardSummary` | HttpGet,Authorize(Roles = StaffRoles) | string ym, int classYear = 0, int classBan = 0 |
| `/DevotionPoints/DashboardTrend` | HttpGet,Authorize(Roles = StaffRoles) | int months = 12, int classYear = 0, int classBan = 0 |
| `/DevotionPoints/ParticipantRankAll` | HttpGet,Authorize(Roles = StaffRoles) | string ym, int classYear = 0, int classBan = 0 |
| `/DevotionPoints/ClassMonthlyTotal` | HttpGet,Authorize(Roles = StaffRoles) | int? classYear, int? classBan, string? ym |
| `/DevotionPoints/ClassMonthlyMyClass` | HttpGet,Authorize(Roles = StaffRoles) |  |
| `/DevotionPoints/ClassMonthlyMyClassEdit` | HttpGet,Authorize(Roles = StaffRoles) | int personId, string? fullName, int? year |
| `/DevotionPoints/Dashboard` | HttpGet,Authorize(Roles = StaffRoles) | string? ym, int classYear = 0, int classBan = 0 |
| `/DevotionPoints/Participants` | HttpGet,Authorize(Roles = StaffRoles) | string? ym, int classYear = 0, int classBan = 0 |
| `/DevotionPoints/MyMonthly` | HttpGet |  |
| `/DevotionPoints/MyTotal` | HttpGet |  |
| `/DevotionPoints/MyMonthlyData` | HttpGet | int months = 24 |

## EntranceExamPray
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/EntranceExamPray/Index` | AllowAnonymous |  |
| `/EntranceExamPray/Like` | AllowAnonymous,HttpPost,IgnoreAntiforgeryToken | int prayId |
| `/EntranceExamPray/Detail` | AllowAnonymous | int PrayId |
| `/EntranceExamPray/Verify` | HttpPost,ValidateAntiForgeryToken,Authorize | int PrayId |
| `/EntranceExamPray/List` | AllowAnonymous |  |
| `/EntranceExamPray/Create` | Authorize |  |
| `/EntranceExamPray/Edit` | Authorize | int PrayId |
| `/EntranceExamPray/Save` | HttpPost,ValidateAntiForgeryToken,Authorize | EntranceExamPray model |
| `/EntranceExamPray/Delete` | Authorize | int PrayId |
| `/EntranceExamPray/DeleteConfirmed` | HttpPost,ValidateAntiForgeryToken,Authorize | int PrayId |

## Exercise
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Exercise/Index` | HttpGet |  |
| `/Exercise/Detail` | HttpGet | int id |

## FamilyPhoto

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/FamilyPhoto/Welcome` | HttpGet |  |
| `/FamilyPhoto/Index` | HttpGet |  |
| `/FamilyPhoto/Like` | HttpPost,ValidateAntiForgeryToken | int photoId |
| `/FamilyPhoto/Sticker` | HttpPost,ValidateAntiForgeryToken | int photoId, int kind |
| `/FamilyPhoto/Enter` | HttpGet | bool fresh = false |
| `/FamilyPhoto/Enter` | HttpPost,ValidateAntiForgeryToken | string familyName, string pin |
| `/FamilyPhoto/Manage` | HttpGet |  |
| `/FamilyPhoto/Upload` | HttpPost,ValidateAntiForgeryToken | int slot, IFormFile? file, string? caption |
| `/FamilyPhoto/Caption` | HttpPost,ValidateAntiForgeryToken | int slot, string? caption |
| `/FamilyPhoto/Remove` | HttpPost,ValidateAntiForgeryToken | int slot |
| `/FamilyPhoto/DeleteEntry` | HttpPost,ValidateAntiForgeryToken |  |
| `/FamilyPhoto/Leave` | HttpPost,ValidateAntiForgeryToken |  |
| `/FamilyPhoto/Result` | HttpGet | bool preview = false |
| `/FamilyPhoto/Admin` | HttpGet,Authorize(Roles = nameof(Roles.Admin) + "," + nameof(Roles.SuperAdmin)) |  |
| `/FamilyPhoto/Judge` | HttpGet |  |
| `/FamilyPhoto/AdminEntryHidden` | HttpPost,ValidateAntiForgeryToken,Authorize(Roles = nameof(Roles.Admin) + "," + nameof(Roles.SuperAdmin)) | int entryId, bool hidden |
| `/FamilyPhoto/AdminPhotoHidden` | HttpPost,ValidateAntiForgeryToken,Authorize(Roles = nameof(Roles.Admin) + "," + nameof(Roles.SuperAdmin)) | int photoId, bool hidden |
| `/FamilyPhoto/AdminResetPin` | HttpPost,ValidateAntiForgeryToken,Authorize(Roles = nameof(Roles.Admin) + "," + nameof(Roles.SuperAdmin)) | int entryId |
| `/FamilyPhoto/AdminPublish` | HttpPost,ValidateAntiForgeryToken,Authorize(Roles = nameof(Roles.Admin) + "," + nameof(Roles.SuperAdmin)) | bool published |
| `/FamilyPhoto/AdminResetAll` | HttpPost,ValidateAntiForgeryToken,Authorize(Roles = nameof(Roles.SuperAdmin)) | string confirm |

## FileUploadContest
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/FileUploadContest/Upload` | HttpPost | FileUploadViewModel model |
| `/FileUploadContest/CreateUploadData` |  | FileUploadViewModel model |

## FileUpload
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/FileUpload/Upload` | HttpGet | string category, int Id, int YYear, int MMonth,int WWeek, string Weekl… |
| `/FileUpload/Upload` | HttpPost | FileUploadViewModel model, string WeeklyWordToReadID |
| `/FileUpload/CreateUploadData` |  | FileUploadViewModel model |

## GlobalCookie

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/GlobalCookie/ShouldShow` | HttpPost | [FromBody] GlobalCookieCheckRequest request |
| `/GlobalCookie/Hide` | HttpPost | [FromBody] GlobalCookieHideRequest request |

## HealingAndRestorationPray
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/HealingAndRestorationPray/Index` | AllowAnonymous |  |
| `/HealingAndRestorationPray/Like` | AllowAnonymous,HttpPost,IgnoreAntiforgeryToken | int prayId |
| `/HealingAndRestorationPray/Detail` | AllowAnonymous | int PrayId |
| `/HealingAndRestorationPray/Verify` | HttpPost,Authorize,ValidateAntiForgeryToken | int PrayId, string? returnTo = null |
| `/HealingAndRestorationPray/Unverify` | HttpPost,Authorize,ValidateAntiForgeryToken | int PrayId, string? returnTo = null |
| `/HealingAndRestorationPray/List` | Authorize |  |
| `/HealingAndRestorationPray/Create` | Authorize |  |
| `/HealingAndRestorationPray/Edit` | Authorize | int PrayId |
| `/HealingAndRestorationPray/Save` | HttpPost,ValidateAntiForgeryToken,Authorize | HealingAndRestorationPray model |
| `/HealingAndRestorationPray/Delete` | Authorize | int PrayId |
| `/HealingAndRestorationPray/DeleteConfirmed` | HttpPost,ValidateAntiForgeryToken,Authorize | int PrayId |

## HelpDesk
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/HelpDesk/Index` |  |  |
| `/HelpDesk/CreateRepairHomePage` | HttpPost | [FromBody] RepairHomePageData param |
| `/HelpDesk/DeleteRepairHomePage` |  | int Id |
| `/HelpDesk/ModifyRepairHomePage` |  | int Id |
| `/HelpDesk/ModifyRepairHomePageProcess` | HttpPost | RepairHomePageData param |

## Home
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Home/IndexBranch` |  |  |
| `/Home/HowToInstall` |  |  |
| `/Home/HowToInstall_Android` |  |  |
| `/Home/HowToInstall_iPhone` |  |  |
| `/Home/Index` |  |  |
| `/Home/IndexComplex` |  |  |
| `/Home/DeferVBSParticipationSurveyGuide` | Authorize,HttpPost,ValidateAntiForgeryToken | int days |
| `/Home/goToday` |  |  |
| `/Home/goWorkDT` |  | DateTime? workDT |
| `/Home/MissionMainBoard` | Authorize | DateTime? naviDate |
| `/Home/DeletePopMessage` |  | int Id, string MessageText |
| `/Home/NaviMonth` |  | string UserName |
| `/Home/NaviHistory` |  | string UserName |
| `/Home/ResetUserType` | AllowAnonymous,HttpPost,ValidateAntiForgeryToken |  |
| `/Home/ChangeUserType` | AllowAnonymous,HttpPost,ValidateAntiForgeryToken |  |
| `/Home/PickUserType` | AllowAnonymous,HttpGet | bool force = false |
| `/Home/PickUserType` | AllowAnonymous,HttpPost,ValidateAntiForgeryToken | string userType |

## HumanResource
클래스: `[Authorize]  [LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/HumanResource/Index` |  |  |
| `/HumanResource/ViewMainBirthDay` | AllowAnonymous |  |
| `/HumanResource/ConfirmBirth` | HttpPost,ValidateAntiForgeryToken | BirthConfirmVM model |
| `/HumanResource/printStudentCard` |  | string DeptCode, int ClassYear,int ClassBan |
| `/HumanResource/IndexAdressOnly` |  |  |
| `/HumanResource/IndexBan` |  |  |
| `/HumanResource/IndexTeacher` |  |  |
| `/HumanResource/Index2` |  | string HumanResourceCategory |
| `/HumanResource/addressBookMain` |  |  |
| `/HumanResource/addressBookDetail` |  | string DeptCode, string RankQueryOptionData, int ClassYear, int ClassB… |
| `/HumanResource/addressBookDetailBirthDay` | AllowAnonymous | string DeptCode, string RankQueryOptionData, int ClassYear, int ClassB… |
| `/HumanResource/ModifyHRPhoto` |  | string UserId |
| `/HumanResource/ManageUserInfo` |  | string UserId, int PersonId |
| `/HumanResource/ChangeUserInfo` |  | string UserId, int PersonId |
| `/HumanResource/ChangeUserInfoProcess` | HttpPost | UserInfo model |
| `/HumanResource/ChangeUserDeptInfoIntro` |  | string UserId |
| `/HumanResource/ChangeUserDeptInfoMain` |  | string UserId |
| `/HumanResource/ChangeUseRoleMain` |  | string UserId, string DeptCode,string DeptNM |
| `/HumanResource/ChangeUseRoleMainProcess` |  | DeptRoleUserAssoc model |
| `/HumanResource/ChangeUserClassInfo` |  | string UserId |
| `/HumanResource/ChangeUserClassInfoProcess` | HttpPost | DeptRoleUserAssoc model |
| `/HumanResource/GetDetails` |  | int ClassYear, int ClassBan , string BanMode |
| `/HumanResource/GetProfilePicture` | AllowAnonymous,HttpGet | string userId |
| `/HumanResource/UploadProfilePicture` | HttpPost | [FromBody] ImageUploadModel model |

## Intro
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Intro/Index` |  |  |
| `/Intro/Index1` |  |  |
| `/Intro/Index2` |  |  |
| `/Intro/Index3` |  |  |
| `/Intro/Index4` |  |  |
| `/Intro/Index5` |  |  |
| `/Intro/Index6` |  |  |
| `/Intro/Index7` |  |  |
| `/Intro/WhatChallenge2014` |  |  |
| `/Intro/HowtogiveReward` |  |  |
| `/Intro/HowTo1` |  |  |
| `/Intro/HowTo2` |  |  |
| `/Intro/HowTo3` |  |  |
| `/Intro/HowTo4` |  |  |
| `/Intro/HowTo5` |  |  |
| `/Intro/HowTo6` |  |  |
| `/Intro/HowTo7` |  |  |
| `/Intro/HowToPublishExam` |  |  |

## Jubo

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Jubo/Index` | HttpGet | int page = 1 |
| `/Jubo/Create` |  |  |
| `/Jubo/Create` |  | JuboCreateViewModel model |
| `/Jubo/Edit` |  | int id |
| `/Jubo/SaveMeta` |  | int id, string title, string? season, string templateType |
| `/Jubo/SaveBlock` |  | [FromBody] JuboBlockSaveRequest? request |
| `/Jubo/Preview` | HttpGet | int id |
| `/Jubo/Print` | HttpGet | int id |
| `/Jubo/Delete` |  | int id |
| `/Jubo/TogglePublish` |  | int id |
| `/Jubo/Attendance` |  | int id |
| `/Jubo/DeleteAttendanceClass` |  | int id |
| `/Jubo/SaveNewComer` |  | [FromBody] SaveNewComerViewModel? model |
| `/Jubo/DeleteNewComer` |  | int id |
| `/Jubo/CreateSnapshot` |  | int id |
| `/Jubo/InitTables` |  |  |

## JuManNa
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/JuManNa/Index` | Authorize(Roles = nameof(Roles.SuperAdmin)) |  |
| `/JuManNa/IndexGroupByYYYYMM` | Authorize(Roles = nameof(Roles.SuperAdmin)) |  |
| `/JuManNa/IndexQueryByYYYYMM` | Authorize(Roles = nameof(Roles.SuperAdmin)) | string option, string YYYYMM |
| `/JuManNa/MonthlyBulk` | Authorize(Roles = nameof(Roles.SuperAdmin)) | string YYYYMM |
| `/JuManNa/ProcessMonthlyBulk` | HttpPost,ValidateAntiForgeryToken,Authorize(Roles = nameof(Roles.SuperAdmin)) | JuManNaMonthly model |
| `/JuManNa/SegmentEditor` | Authorize(Roles = nameof(Roles.SuperAdmin)) | string YYYYMM |
| `/JuManNa/SaveSegment` | HttpPost,ValidateAntiForgeryToken,Authorize(Roles = nameof(Roles.SuperAdmin)) | int id, int slot, string kind, int sec |
| `/JuManNa/IndexByID` |  | int Id |
| `/JuManNa/ModifyJuManNa` | Authorize(Roles = nameof(Roles.SuperAdmin)) | int Id |
| `/JuManNa/ModifyJuManNabyDate` | Authorize(Roles = nameof(Roles.SuperAdmin)) | DateTime dt |
| `/JuManNa/ProcessModifyJuManNa` | HttpPost,Authorize(Roles = nameof(Roles.SuperAdmin)) | JuManNa juManNa |
| `/JuManNa/GetDetails` |  | string bibleText , int fromJang , int fromZul , int toJang , int toZul… |

## LearningCenter
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/LearningCenter/Index` |  |  |
| `/LearningCenter/DetailView` |  | int Id, string ExposeDate |
| `/LearningCenter/DeleteView` |  | int Id |
| `/LearningCenter/EditView` |  | int Id |
| `/LearningCenter/EditViewProcess` |  | OpicData param |
| `/LearningCenter/EnrollView` |  |  |
| `/LearningCenter/EnrollViewProcess` |  | OpicData param |
| `/LearningCenter/List` |  | string sortBy, string query |
| `/LearningCenter/MarkView` |  | int Id |
| `/LearningCenter/SearchQuery` |  | string query |

## LikeVoting
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/LikeVoting/Index` | HttpGet | string sortBy |
| `/LikeVoting/Like` | HttpPost | [FromForm] int id |

## LoginHistMgmt
클래스: `[Authorize]  [LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/LoginHistMgmt/Index` |  |  |
| `/LoginHistMgmt/DeleteLoginHistory` |  | string UserName |

## Notice
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Notice/Index` |  | int page = 1, string keyword = null |
| `/Notice/Branch` |  |  |
| `/Notice/Details` |  | int id |
| `/Notice/Create` | Authorize |  |
| `/Notice/Create` | HttpPost,ValidateAntiForgeryToken,Authorize | NoticeEditVm vm, List<IFormFile> files |
| `/Notice/Edit` | Authorize | int id |
| `/Notice/DownloadAttachment` |  | long id |
| `/Notice/Delete` | HttpPost,ValidateAntiForgeryToken,Authorize | int id |

## OneLineFamousWord
클래스: `[Authorize(Roles = nameof(Roles.Admin))]  [LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/OneLineFamousWord/Index` |  |  |
| `/OneLineFamousWord/CreateOneLineFamousWord` |  | int Id |
| `/OneLineFamousWord/CreateOneLineFamousWordProcess` | HttpPost | OneLineFamousWords param |
| `/OneLineFamousWord/DeleteOneLineFamousWord` |  | int Id |
| `/OneLineFamousWord/ModifyOneLineFamousWordProcess` | HttpPost | OneLineFamousWords param |
| `/OneLineFamousWord/ModifyOneLineFamousWord` |  | int Id |

## Opic
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Opic/Index` |  |  |
| `/Opic/DetailWords` |  | int Id, string ExposeDate |
| `/Opic/DeleteView` |  | int Id |
| `/Opic/MarkView` |  | int Id |
| `/Opic/ModifyView` |  | int Id |
| `/Opic/EnrollView` |  |  |
| `/Opic/ModifyProcess` |  | OpicData param |
| `/Opic/EnrollProcess` |  | OpicData param |
| `/Opic/SearchQuery` |  | string query |
| `/Opic/DetailList` |  | string sortBy, string query |

## Places
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Places/Main` | HttpGet |  |
| `/Places/List` | HttpGet | string category = "맛집", string? q = null |
| `/Places/Create` | HttpGet | string? category = "맛집" |
| `/Places/Create` | HttpPost,ValidateAntiForgeryToken | PlaceItemInput input |
| `/Places/Edit` | HttpGet | int id |
| `/Places/Edit` | HttpPost,ValidateAntiForgeryToken | PlaceItemInput input |
| `/Places/Delete` | HttpPost,ValidateAntiForgeryToken | int id, string category |

## PopMessage
클래스: `[Authorize]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/PopMessage/Index` | HttpGet |  |
| `/PopMessage/Compose` | HttpGet | string to, string? returnUrl = null |
| `/PopMessage/PrayLetter` | HttpGet | string? returnUrl = null |
| `/PopMessage/Send` | HttpPost,ValidateAntiForgeryToken | PopMessageComposeVM vm |
| `/PopMessage/SendAjax` | HttpPost,ValidateAntiForgeryToken | [FromForm] string ToUserName, [FromForm] string MessageText |
| `/PopMessage/InboxList` | HttpGet | int page = 0, int pageSize = 20 |
| `/PopMessage/SentList` | HttpGet | int page = 0, int pageSize = 20 |
| `/PopMessage/UnreadCount` | HttpGet |  |
| `/PopMessage/MarkRead` | HttpPost,ValidateAntiForgeryToken | [FromForm] string ids |
| `/PopMessage/Delete` | HttpPost,ValidateAntiForgeryToken | [FromForm] int id |
| `/PopMessage/SearchPeople` | HttpGet | string q, int take = 20 |
| `/PopMessage/AdminDeleteSuperAdminMessages` | HttpPost,ValidateAntiForgeryToken,Authorize(Roles = nameof(Roles.SuperAdmin)) |  |
| `/PopMessage/AdminBroadcast` | HttpPost,ValidateAntiForgeryToken | string MessageText |

## PraiseHome
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/PraiseHome/Index` |  |  |
| `/PraiseHome/IndexBranchOld` |  |  |
| `/PraiseHome/IndexBranch` |  |  |
| `/PraiseHome/IndexQuery` |  | string Kind |
| `/PraiseHome/EnrollView` |  | string kind |
| `/PraiseHome/ModifyView` |  | int Id, string Kind |
| `/PraiseHome/ModifyViewProcess` |  | PraiseContent param |
| `/PraiseHome/EnrollViewProcess` |  | PraiseContent param |
| `/PraiseHome/DeleteView` |  | int Id, string Kind |

## PrayBranch
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/PrayBranch/Index` |  |  |

## Prayer365

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Prayer365/Index` | HttpGet | string? md, string? view |
| `/Prayer365/Pray` | HttpPost,IgnoreAntiforgeryToken | string? md |
| `/Prayer365/Participants` | HttpGet,Authorize | string? sort |
| `/Prayer365/Admin` | HttpGet,Authorize | string? f, string? q |
| `/Prayer365/Edit` | HttpGet,Authorize | string? md |
| `/Prayer365/Edit` | HttpPost,Authorize,ValidateAntiForgeryToken | string md, string? title, string? verseRef, string? verseText, string?… |

## Prayer
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Prayer/Index` |  | int? day |
| `/Prayer/Edit` |  | int id |
| `/Prayer/Edit` | HttpPost,ValidateAntiForgeryToken | PrayerArrow input |

## PrayFriends
클래스: `[Authorize]  [LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/PrayFriends/Index` |  |  |
| `/PrayFriends/PrayFriend` | Authorize(Roles = nameof(Roles.Admin)) |  |

## PrayHome
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/PrayHome/Index` |  |  |
| `/PrayHome/Branch` |  |  |
| `/PrayHome/Intercessory` |  |  |
| `/PrayHome/coClass` |  |  |
| `/PrayHome/TodayPray` | AllowAnonymous |  |
| `/PrayHome/TodayPrayList` | AllowAnonymous |  |
| `/PrayHome/ModifyPray` |  | int PrayID |
| `/PrayHome/ModifyPrayProcess` | HttpPost | TodayPray model |
| `/PrayHome/PrayBan` |  |  |
| `/PrayHome/CreatePrayHistory` |  | string personPraying, string prayerRecipient |
| `/PrayHome/WhatTick` | AllowAnonymous |  |

## PrayPhrase
클래스: `[Authorize]  [LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/PrayPhrase/Index` |  |  |
| `/PrayPhrase/EnrollPrayPhrase` |  |  |
| `/PrayPhrase/ModifyPrayPhrase` |  | int Id |
| `/PrayPhrase/CreatePrayPhrase` | HttpPost | PrayPhrase PrayPhrase |
| `/PrayPhrase/UpdatePrayPhrase` | HttpPost | PrayPhrase PrayPhrase |
| `/PrayPhrase/PrayPhraseDetailList` |  |  |
| `/PrayPhrase/DeletePrayPhrase` |  | int Id |
| `/PrayPhrase/RestorPrayPhrase` |  | int YYear, int MMonth |

## Present

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Present/AdminUpcomingBirthdays` | Authorize, HttpGet | int withinDays = 30, string filter = "all" |
| `/Present/PresentForNewFriend` | HttpGet | string message |
| `/Present/PresentForInvitFriend` | HttpGet | string message |
| `/Present/BirthGuide` |  | string message |
| `/Present/Kind` |  |  |
| `/Present/Birth` |  | [FromQuery] string? userId |
| `/Present/PickBirth` |  | int? productId, string? targetUserId, string? returnTo = null |
| `/Present/CancelBirth` |  | string? targetUserId, string? returnTo = null |
| `/Present/ListPresent` |  | string? productDisplayOpton |
| `/Present/Detail` | HttpGet | int productId, string? productDisplayOpton, string? returnUrl |
| `/Present/PresentDetail` | HttpGet | int productId, string? returnUrl, string? productDisplayOpton |
| `/Present/AdminBirthPickSetSent` |  | long pickId, bool sent, string? q, bool includeInactive, string? filte… |
| `/Present/AdminBirthPickSetPurchased` |  | long pickId, bool purchased, string? q, bool includeInactive, string? … |
| `/Present/AdminBirthdaySetSkip` |  | int personId, bool isSkipped, string? filter, int withinDays = 30 |
| `/Present/AwardItemAdmin` |  |  |
| `/Present/AwardItemCountByKind` |  |  |
| `/Present/AwardItemCountByPrice` |  |  |
| `/Present/AwardItemAdminByPrice` |  | int min, int max |
| `/Present/AwardItemAdminByName` |  | string name |
| `/Present/AwardItemCreate` |  |  |
| `/Present/AwardItemCreate` |  | AwardItemEditVM vm |
| `/Present/AwardItemEdit` |  | int id |
| `/Present/AwardItemEdit` |  | AwardItemEditVM vm |
| `/Present/AwardItemToggleHidden` |  | int id |
| `/Present/AwardItemDelete` |  | int id |

## Profile
클래스: `[Authorize]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Profile/Edit` | HttpGet |  |
| `/Profile/Edit` | HttpPost,ValidateAntiForgeryToken | ProfileEditVM vm |

## QTHome
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/QTHome/Index` |  |  |

## QuestionBank
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/QuestionBank/Index` |  |  |
| `/QuestionBank/IndexByCode` |  |  |
| `/QuestionBank/IndexSlide` |  |  |
| `/QuestionBank/IndexSlideView` |  |  |
| `/QuestionBank/IndexAnswer` |  |  |
| `/QuestionBank/DetailQuestionBank` |  | int Id |
| `/QuestionBank/ModifyQuestionBank` |  | int Id |
| `/QuestionBank/ModifyQuestionBankProcess` | HttpPost | QuestionBankData param |
| `/QuestionBank/VerifyQuestionBank` |  | int Id |
| `/QuestionBank/UpdatePrevRecordUpdateDT` |  | int Id |
| `/QuestionBank/UpdateNextRecordUpdateDT` |  | int Id |
| `/QuestionBank/MinifyQuestionBank` |  | int Id |
| `/QuestionBank/DeleteQuestionBank` |  | int Id |

## Question
클래스: `[Authorize(Roles = nameof(Roles.Admin))]  [LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Question/Index11` |  |  |
| `/Question/Index` |  | DateTime? selectedDate, string? KindData |
| `/Question/GetEvents` |  | DateTime date |
| `/Question/createNewQuestion` |  |  |
| `/Question/BranchQuestionType` |  | string WhenDate |
| `/Question/BranchQuestionMethod` |  | string WhenDate, string KindData |
| `/Question/FromTemplate` |  | string WhenDate |
| `/Question/BranchQuestionType2` |  | string QuestionType, DateTime? WhenDate |
| `/Question/CreateQuizByBank` |  | int Id, string WhenDate |
| `/Question/ModifyQuiz` |  | int Id |
| `/Question/ModifyQuizByDate` |  | DateTime dt |
| `/Question/CreateQuizBlank` |  | string QuestionType, string WhenDate |
| `/Question/ModifyQuestionSubject` |  | int Id |
| `/Question/ModifyQuestionObject` |  | int Id |
| `/Question/ModifyQuestionProcess` | HttpPost | QuestionTypeDataObjects questionTypeDataObject |
| `/Question/CreateQuestionProcess` | HttpPost | QuestionTypeDataObjects questionTypeDataObject |
| `/Question/CreateQuestionSubject` | HttpPost | QuestionTypeData questionTypeData |
| `/Question/UpdateQuestionSubject` | HttpPost | QuestionTypeData questionTypeData |
| `/Question/UpdateQuestionObject` | HttpPost | QuestionTypeDataObject questionTypeDataObject |
| `/Question/QuestionDetailList` |  | string KindData |
| `/Question/DeleteQuestion` |  | int Id |
| `/Question/DeleteQuestionBank` |  | int Id, string WhenDate |
| `/Question/CreateQuizById` |  | int Id, string WhenDate |

## ReplayBoard

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/ReplayBoard/Index` |  | string tableName, int page = 1, int pageSize = 15, string? q = null |
| `/ReplayBoard/Details` |  | string tableName, int id |
| `/ReplayBoard/Create` | HttpGet,Authorize | string tableName |
| `/ReplayBoard/Edit` | HttpGet,Authorize | string tableName, int id |
| `/ReplayBoard/Delete` | HttpPost,Authorize,ValidateAntiForgeryToken | string tableName, int id |
| `/ReplayBoard/EditReply` | HttpPost,Authorize,ValidateAntiForgeryToken | string tableName, int postId, int replyId, string replyWriter, string … |
| `/ReplayBoard/DeleteReply` | HttpPost,Authorize,ValidateAntiForgeryToken | string tableName, int postId, int replyId |
| `/ReplayBoard/DownloadFile` |  | string tableName, int fileId |
| `/ReplayBoard/DeleteFile` | HttpPost,Authorize,ValidateAntiForgeryToken | string tableName, int postId, int fileId |
| `/ReplayBoard/ToggleLike` | HttpPost,Authorize,ValidateAntiForgeryToken | string tableName, int postId |
| `/ReplayBoard/ToggleNotice` | HttpPost,Authorize,ValidateAntiForgeryToken | string tableName, int postId, bool isNotice |

## ResourceBox

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/ResourceBox/Index` |  |  |
| `/ResourceBox/worldView2025` |  |  |
| `/ResourceBox/TeacherSeminarfortheFirstHalfof2026` |  |  |

## RhapsodyOfReality
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/RhapsodyOfReality/Index` |  |  |
| `/RhapsodyOfReality/DetailWords` |  | int Id, string ExposeDate, string methodTEXT |
| `/RhapsodyOfReality/DetailWordsP` |  | int Id, string ExposeDate, string methodTEXT |
| `/RhapsodyOfReality/DeleteView` |  | int Id |
| `/RhapsodyOfReality/ModifyView` |  | int Id |
| `/RhapsodyOfReality/EnrollView` |  |  |
| `/RhapsodyOfReality/ModifyProcess` |  | RhapsodyOfReality param |
| `/RhapsodyOfReality/EnrollProcess` |  | RhapsodyOfReality param |
| `/RhapsodyOfReality/DetailList` |  |  |

## Role

| 주소 | 속성 | 매개변수 |
|---|---|---|

## RoleManager
클래스: `[Authorize(Roles = nameof(Roles.SuperAdmin))]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/RoleManager/Index` |  |  |
| `/RoleManager/AddRole` | HttpPost | string roleName |

## Shorts
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Shorts/Main` |  |  |
| `/Shorts/Index` |  | string? q, VideoKindOption? param |
| `/Shorts/Create` |  |  |
| `/Shorts/Create` | HttpPost,ValidateAntiForgeryToken | ShortVideoInput input |
| `/Shorts/Edit` |  | int id |
| `/Shorts/Edit` | HttpPost,ValidateAntiForgeryToken | ShortVideoInput input |
| `/Shorts/Delete` | HttpPost,ValidateAntiForgeryToken | int id |

## SiteVisit
클래스: `[Authorize]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/SiteVisit/Index` |  |  |

## Story
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Story/Index` |  |  |
| `/Story/Manage` | HttpGet,Authorize(Roles = StaffRoles) |  |
| `/Story/Add` | HttpPost,ValidateAntiForgeryToken,Authorize(Roles = StaffRoles) | string url, string title |
| `/Story/Edit` | HttpPost,ValidateAntiForgeryToken,Authorize(Roles = StaffRoles) | int id, string url, string title |
| `/Story/Delete` | HttpPost,ValidateAntiForgeryToken,Authorize(Roles = StaffRoles) | int id, string? title |

## StudentPointDashboard2
클래스: `[Authorize]  [LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/StudentPointDashboard2/Guide` | HttpGet |  |
| `/StudentPointDashboard2/Index` | HttpGet |  |
| `/StudentPointDashboard2/PopupAction` | HttpPost,ValidateAntiForgeryToken | [FromBody] PopupActionRequest req |
| `/StudentPointDashboard2/MyGiftBox` | HttpGet |  |
| `/StudentPointDashboard2/SelectGift` | HttpPost,ValidateAntiForgeryToken | int productId |
| `/StudentPointDashboard2/CancelGift` | HttpPost,ValidateAntiForgeryToken | int orderId |
| `/StudentPointDashboard2/AdminPurchaseList` | Authorize,HttpGet |  |
| `/StudentPointDashboard2/TeamGiftCancel` | Authorize(Roles = "Admin,SuperAdmin"),HttpPost,ValidateAntiForgeryToken | string studentUserName, int milestone |
| `/StudentPointDashboard2/TeamGiftSelect` | Authorize(Roles = "Admin,SuperAdmin"),HttpGet | string studentUserName, string teamName, int milestone = 1000 |
| `/StudentPointDashboard2/TeamGiftGrant` | Authorize(Roles = "Admin,SuperAdmin"),HttpPost,ValidateAntiForgeryToken | string studentUserName, int productId, string teamName, int milestone … |
| `/StudentPointDashboard2/FamilyGiftSelect` | HttpGet | string teamName, string topScorer, int teamTotal, int threshold = 1000 |
| `/StudentPointDashboard2/FamilyGiftSelectPost` | HttpPost,ValidateAntiForgeryToken | int productId, string teamName, string topScorerUserName, int teamTota… |
| `/StudentPointDashboard2/FamilyGiftCancel` | HttpPost,ValidateAntiForgeryToken | string teamName, string topScorerUserName, int threshold = 1000 |
| `/StudentPointDashboard2/FamilyMilestoneHide` | HttpPost,ValidateAntiForgeryToken | int milestoneConfigId, int familyTeamId |
| `/StudentPointDashboard2/TotalRank` | HttpGet | int? year = null |

## StudentPointDashboard3
클래스: `[Authorize]  [LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/StudentPointDashboard3/Guide` | HttpGet |  |
| `/StudentPointDashboard3/Index` | HttpGet |  |
| `/StudentPointDashboard3/PopupAction` | HttpPost,ValidateAntiForgeryToken | [FromBody] PopupActionRequest req |
| `/StudentPointDashboard3/MyGiftBox` | HttpGet |  |
| `/StudentPointDashboard3/MyGiftBox3` | HttpGet |  |
| `/StudentPointDashboard3/GiftSelect` | HttpGet | string type, int round = 0, int threshold = 0, string? returnTo = null |
| `/StudentPointDashboard3/SelectGift` | HttpPost,ValidateAntiForgeryToken | int productId, string? returnTo = null |
| `/StudentPointDashboard3/CancelGift` | HttpPost,ValidateAntiForgeryToken | int orderId, string? returnTo = null |
| `/StudentPointDashboard3/AdminPurchaseList` | Authorize,HttpGet | string? kind = null |
| `/StudentPointDashboard3/TeamGiftCancel` | Authorize(Roles = "Admin,SuperAdmin"),HttpPost,ValidateAntiForgeryToken | string studentUserName, int milestone |
| `/StudentPointDashboard3/TeamGiftSelect` | Authorize(Roles = "Admin,SuperAdmin"),HttpGet | string studentUserName, string teamName, int milestone = 1000 |
| `/StudentPointDashboard3/TeamGiftGrant` | Authorize(Roles = "Admin,SuperAdmin"),HttpPost,ValidateAntiForgeryToken | string studentUserName, int productId, string teamName, int milestone … |
| `/StudentPointDashboard3/FamilyGiftSelect` | HttpGet | string teamName, string topScorer, int teamTotal, int threshold = 1000 |
| `/StudentPointDashboard3/FamilyMilestoneHide` | HttpPost,ValidateAntiForgeryToken | int milestoneConfigId, int familyTeamId |
| `/StudentPointDashboard3/TotalRank` | HttpGet | int? year = null |

## StudentPointDashboard4
클래스: `[Authorize]  [LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/StudentPointDashboard4/Guide` | HttpGet |  |
| `/StudentPointDashboard4/Index` | HttpGet |  |
| `/StudentPointDashboard4/PopupAction` | HttpPost,ValidateAntiForgeryToken | [FromBody] PopupActionRequest req |
| `/StudentPointDashboard4/MyGiftBox` | HttpGet |  |
| `/StudentPointDashboard4/MyGiftBox3` | HttpGet |  |
| `/StudentPointDashboard4/GiftSelect` | HttpGet | string type, int round = 0, int threshold = 0, string? returnTo = null |
| `/StudentPointDashboard4/SelectGift` | HttpPost,ValidateAntiForgeryToken | int productId, string? returnTo = null |
| `/StudentPointDashboard4/CancelGift` | HttpPost,ValidateAntiForgeryToken | int orderId, string? returnTo = null |
| `/StudentPointDashboard4/AdminPurchaseList` | Authorize,HttpGet | string? kind = null |
| `/StudentPointDashboard4/TeamGiftCancel` | Authorize(Roles = "Admin,SuperAdmin"),HttpPost,ValidateAntiForgeryToken | string studentUserName, int milestone |
| `/StudentPointDashboard4/TeamGiftSelect` | Authorize(Roles = "Admin,SuperAdmin"),HttpGet | string studentUserName, string teamName, int milestone = 1000 |
| `/StudentPointDashboard4/TeamGiftGrant` | Authorize(Roles = "Admin,SuperAdmin"),HttpPost,ValidateAntiForgeryToken | string studentUserName, int productId, string teamName, int milestone … |
| `/StudentPointDashboard4/FamilyGiftSelect` | HttpGet | string teamName, string topScorer, int teamTotal, int threshold = 1000 |
| `/StudentPointDashboard4/FamilyMilestoneHide` | HttpPost,ValidateAntiForgeryToken | int milestoneConfigId, int familyTeamId |
| `/StudentPointDashboard4/TotalRank` | HttpGet | int? year = null |

## SummerCamp
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/SummerCamp/Index` |  |  |
| `/SummerCamp/Index2024` |  |  |
| `/SummerCamp/Invitation` |  |  |
| `/SummerCamp/TimeTable` |  |  |
| `/SummerCamp/Study` |  |  |
| `/SummerCamp/intro` |  |  |
| `/SummerCamp/TrainVideo` |  |  |
| `/SummerCamp/gw02` |  |  |
| `/SummerCamp/gw03` |  |  |
| `/SummerCamp/studyslide_00` |  |  |
| `/SummerCamp/studyslide_01` |  |  |
| `/SummerCamp/studyslide_02` |  |  |
| `/SummerCamp/studyslide_03` |  |  |
| `/SummerCamp/books_01` |  |  |
| `/SummerCamp/books_02` |  |  |
| `/SummerCamp/books_03` |  |  |

## SuperAdminAnomaly
클래스: `[Authorize(Roles = nameof(Roles.SuperAdmin))]  [LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/SuperAdminAnomaly/Index` |  |  |

## Survey
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Survey/CreateSurvey` |  |  |
| `/Survey/ProcessSurvey` | HttpPost | SurveyData param |
| `/Survey/ProcessSurveyResult` |  |  |

## TeacherBooks
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/TeacherBooks/Index` |  | string id |
| `/TeacherBooks/List` |  |  |

## UserRoles
클래스: `[Authorize(Roles = nameof(Roles.SuperAdmin))]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/UserRoles/Index` |  |  |
| `/UserRoles/Delete` |  | string userId |
| `/UserRoles/ImpersonateUser` |  | string userId |
| `/UserRoles/Manage` |  | string userId |
| `/UserRoles/Manage` | HttpPost | List<ManageUserRolesViewModel> model, string userId |

## VBSParticipationSurvey
클래스: `[Authorize]  [LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/VBSParticipationSurvey/Index` |  |  |
| `/VBSParticipationSurvey/StudentSurvey` |  |  |
| `/VBSParticipationSurvey/StaffSurvey` |  |  |
| `/VBSParticipationSurvey/Stats` |  | string view = "All" |
| `/VBSParticipationSurvey/Survey` |  | string who = "" |
| `/VBSParticipationSurvey/Admin` |  | string view = "All" |
| `/VBSParticipationSurvey/ViewAll` |  | string view = "All" |
| `/VBSParticipationSurvey/AdminStats` | HttpGet | string view = "All" |
| `/VBSParticipationSurvey/SaveSlot` | HttpPost | [FromBody] SaveSlotReq req |
| `/VBSParticipationSurvey/SetAll` | HttpPost | [FromBody] SetAllReq req |
| `/VBSParticipationSurvey/SetAllNote` | HttpPost | [FromBody] SetAllNoteReq req |
| `/VBSParticipationSurvey/AdminSaveSlot` | HttpPost | [FromBody] AdminSaveSlotReq req |
| `/VBSParticipationSurvey/DeferGuide` | HttpPost | [FromBody] DeferGuideReq req |

## WeeklyReport
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/WeeklyReport/Index` |  |  |
| `/WeeklyReport/IndexByID` |  | int Id |
| `/WeeklyReport/IndexByID_3` |  | int Id |
| `/WeeklyReport/GetDetails` |  | string bibleText , int fromJang , int fromZul , int toJang , int toZul |

## WeeklyWordToRead
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/WeeklyWordToRead/Index` |  |  |
| `/WeeklyWordToRead/Branch` |  |  |
| `/WeeklyWordToRead/EnrollWeeklyWordToRead` |  |  |
| `/WeeklyWordToRead/SaveUploadData` | HttpPost | UploadDataWithID uploadDataWithID, string action |
| `/WeeklyWordToRead/ModifyUploadData` |  | Category category, int WeeklyWordToReadID, string afterProcess |
| `/WeeklyWordToRead/ModifyWeeklyWordToRead` |  | int Id |
| `/WeeklyWordToRead/MakePublishYn` |  | int Id, string publishYn |
| `/WeeklyWordToRead/DecreasetHitWeeklyWordToRead` |  | int Id |
| `/WeeklyWordToRead/CreateWeeklyWordToRead` | HttpPost | WeeklyWordToRead WeeklyWordToRead |
| `/WeeklyWordToRead/UpdateWeeklyWordToRead` | HttpPost | WeeklyWordToRead WeeklyWordToRead |
| `/WeeklyWordToRead/WeeklyWordToReadDetailList` |  |  |
| `/WeeklyWordToRead/DeleteWeeklyWordToRead` |  | int Id |
| `/WeeklyWordToRead/IntroBook` |  |  |

## WorldviewAdmin
클래스: `[Authorize]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/WorldviewAdmin/SubmitHistory` | HttpGet("SubmitHistory") | [FromQuery] int? lessonNo = null |
| `/WorldviewAdmin/Index` | HttpGet(""),HttpGet("Index") |  |
| `/WorldviewAdmin/SubmitRank` | HttpGet("SubmitRank") | [FromQuery] int? lessonNo = null |
| `/WorldviewAdmin/SetActive` | ValidateAntiForgeryToken,HttpPost("SetActive") | int lessonNo |

## Worldview
클래스: `[Authorize]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/Worldview/Index` | HttpGet("Index") |  |
| `/Worldview/IndexSigned` | Authorize,HttpGet("IndexSigned") |  |
| `/Worldview/Intro` | HttpGet("Intro") |  |
| `/Worldview/StartMission` | HttpGet("StartMission") |  |
| `/Worldview/DashBoard` | HttpGet("DashBoard") |  |
| `/Worldview/SubmitLesson` | Authorize,HttpPost("submitlesson") | [FromBody] LessonSubmitRequest req |
| `/Worldview/GoDashboardAuth` | HttpGet("GoDashboardAuth") |  |
| `/Worldview/LessonByRoute` | HttpGet("Lesson/{id:int}") | [FromRoute] int id |
| `/Worldview/Lesson` | HttpGet("Lesson") | [FromQuery] int id = 1 |
| `/Worldview/LessonById` | HttpGet("LessonById/{id:int}") | [FromRoute] int id |
| `/Worldview/CheckBlanks` | HttpPost("checkblanks") | [FromBody] BlankSubmitRequest req |

## YoutubeShorts
클래스: `[LogAction]`

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/YoutubeShorts/Index` |  |  |
| `/YoutubeShorts/IndexByName` |  | string ContentsText |
| `/YoutubeShorts/IndexByGroup` |  |  |
| `/YoutubeShorts/EnrollYoutubeShorts` |  |  |
| `/YoutubeShorts/ModifyYoutubeShorts` |  | string ContentsURL |
| `/YoutubeShorts/DeleteYoutubeShorts` |  | int id |
| `/YoutubeShorts/EnrollProcessYoutubeShorts` | HttpPost | YoutubeShort param |

## ChurchRecord
클래스: `[Authorize(Roles = "Admin,SuperAdmin,Moderator")]` (파일 이름에 공백: `ChurchRecordController .cs`)

| 주소 | 속성 | 매개변수 |
|---|---|---|
| `/ChurchRecord/Index` |  | string? searchName |
| `/ChurchRecord/Class` |  | int year, int ban, string? searchName |
| `/ChurchRecord/GetPerson` | HttpGet | int personId |
| `/ChurchRecord/UpdatePerson` | HttpPost | [FromBody] PersonEditVM vm |

## 특수 경로

| 주소 | 설명 |
|---|---|
| `/viewTeacherBooks/{id}` | `TeacherBooks/Index` 로 연결되는 별도 경로 |
| `/robots.txt`, `/sitemap.xml` | 미들웨어가 직접 응답(주소별 다름) |
| `/hanmadi-hub` | SignalR 허브(한마디 실시간) |
| `/DailyMission3/*`, `/StudentPointDashboard3/*` | 4번 화면으로 자동 이동 |

## Razor Pages (Areas)

| 주소 | 설명 |
|---|---|
| `/Hanmadi/Admin` | 한마디 실시간 투표 |
| `/Hanmadi/Admin/Live` | 한마디 실시간 투표 |
| `/Hanmadi/Admin/Participants` | 한마디 실시간 투표 |
| `/Hanmadi/Admin/Questions` | 한마디 실시간 투표 |
| `/Hanmadi/Admin/Sessions/Create` | 한마디 실시간 투표 |
| `/Hanmadi` | 한마디 실시간 투표 |
| `/Hanmadi/Join` | 한마디 실시간 투표 |
| `/Hanmadi/Play` | 한마디 실시간 투표 |
| `/Hanmadi/Results` | 한마디 실시간 투표 |
| `/Identity/Account/AccessDenied` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/ConfirmEmail` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/ConfirmEmailChange` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/ExternalLogin` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/ForgotPassword` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/ForgotPasswordConfirmation` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/Lockout` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/Login` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/LoginWith2fa` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/LoginWithRecoveryCode` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/Logout` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/Manage/ChangePassword` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/Manage/DeletePersonalData` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/Manage/Disable2fa` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/Manage/DownloadPersonalData` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/Manage/Email` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/Manage/EnableAuthenticator` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/Manage/ExternalLogins` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/Manage/GenerateRecoveryCodes` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/Manage` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/Manage/PersonalData` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/Manage/ResetAuthenticator` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/Manage/SetPassword` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/Manage/ShowRecoveryCodes` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/Manage/TwoFactorAuthentication` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/Register` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/RegisterConfirmation` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/ResendEmailConfirmation` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/ResetPassword` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Account/ResetPasswordConfirmation` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
| `/Identity/Error` | ASP.NET Identity 기본 화면(로그인 · 가입 · 계정 관리) |
