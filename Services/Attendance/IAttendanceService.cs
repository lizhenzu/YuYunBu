using BulletinBoard.Common;
using BulletinBoard.Models;
using BulletinBoard.ViewModels.Attendance;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BulletinBoard.Services
{
   
    public interface IAttendanceService
    {
        //Task<bool> SaveTeacherAssignAsync(int classYear, int classBan, int teacherPersonId, string? updatedBy = null);
        //Task<AttendanceAllClassesDashboardVM> GetAllClassesDashboardAsync(DateTime attendSunday, string? userId); 
        Task<int> GetMyAttendanceRateYearToDatePercentAsync(string userId);
        Task<int> GetMyAttendanceCountYearToDateAsync(string userId);
        Task<int> GetMyAttendanceWeeksYearToDateAsync(string userId);
        //Task<TeacherAssignVM> GetTeacherAssignPageAsync();
        //Task<MyClassInfo?> GetMyClassByUserIdAsync(string userId);
        //Task<AttendanceCheckVM> GetCheckPageAsync(DateTime date, int classYear, int classBan);
        //Task<SavePresentResult> SavePresentAsync(DateTime date, int PersonId, bool isPresent, string? updatedBy);
        //Task<AttendanceDashboardVM> GetDashboardAsync(DateTime date, int classYear, int classBan);
        //Task<AttendanceAdminTrendVM> GetAdminTrend3WeeksAsync(DateTime baseDate);
        Task<AttendanceAdminStudentRankVM> GetAdminStudentRankYearToDateAsync(DateTime latestSunday);
        Task<AttendanceSundayStudentsVM> GetSundayStudentsDetailAsync(DateTime sunday);
        Task<AttendanceSundayClassDetailVM> GetSundayClassDetailAsync(DateTime sunday);
        Task<AttendanceAdminStudentRankVM> GetAdminStudentRank3WeeksAsync(DateTime baseDate);
        Task<bool> SaveTeacherAssignAsync(int classYear, int classBan, int teacherPersonId, string? updatedBy = null);
        Task<AttendanceAdminStudentMatrixVM> GetAdminStudentMatrixAsync(DateTime latestSunday, int weeks);
        Task<AttendanceAllClassesDashboardVM> GetAllClassesDashboardAsync(DateTime attendSunday, string? userId);
        Task<TeacherAssignVM> GetTeacherAssignPageAsync();
        Task<MyClassInfo?> GetMyClassByUserIdAsync(string userId);

        Task<AttendanceCheckVM> GetCheckPageAsync(DateTime date, int classYear, int classBan);

        // ✅ PersonId 기반 저장 (출석만 저장 / 해제는 Delete)
        Task<SavePresentResult> SavePresentAsync(DateTime date, int personId, bool isPresent, string? updatedBy);

        Task<AttendanceDashboardVM> GetDashboardAsync(DateTime date, int classYear, int classBan);

        // (나중에 삭제 예정이면 남겨둬도 됨)
        Task<AttendanceAdminTrendVM> GetAdminTrend3WeeksAsync(DateTime baseDate);

        // ✅ Q40 통계(관리자): 월별(반별 합산) + 주별(전체 합산)
        Task<AttendanceMonthlyStatsVM> GetMonthlyStatsAsync(int year, int month);

        // =========================
        // 새신자 관리
        // =========================
        Task<NewComerIndexVM> GetNewComerIndexAsync(string? keyword);
        Task<NewComerDetailVM?> GetNewComerDetailAsync(int id);
        Task<int> SaveNewComerAsync(NewComerSaveModel model, string? userName);
        Task<bool> DeleteNewComerAsync(int id, string? userName);
        Task<bool> AddNewComerVisitTodayAsync(int newComerId, string? userName);
        Task<bool> AddNewComerVisitByDateAsync(int newComerId, DateTime visitDate, string? userName);
        Task<bool> DeleteNewComerVisitAsync(int visitId, string? userName);
        Task<string?> GetNewComerPhotoPathAsync(int newComerId);
        Task<bool> SaveNewComerPhotoPathAsync(int newComerId, string photoPath, string? userName);

        /// <summary>상세 페이지에서 생일·학년반·학교·출석유형 수정</summary>
        Task<bool> UpdateNewComerInfoAsync(NewComerInfoEditModel model, string? userName);

        /// <summary>PersonId 목록 기준으로 각 학생의 마지막 출석일 조회</summary>
        Task<Dictionary<int, DateTime>> GetLastAttendDateMapAsync(IEnumerable<int> personIds);

        /// <summary>장기 결석자 목록 (absentDays일 이상 미출석)</summary>
        Task<LongAbsentListVM> GetLongAbsentStudentsAsync(IWebHostEnvironment env, int absentDays = 90);

        /// <summary>전도 횟수: AttendanceNewComer.EvangelistName = view_PersonMaster_2026.fullName(userId 기준)</summary>
        Task<int> GetEvangelismCountByUserIdAsync(string userId);
    }
}
