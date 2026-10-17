using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulletinBoard.Services.Abstract
{
    public interface IDevotionPointsService
    {
        Task<List<ClassOptionVM>> LoadAllClassesAsync();
        Task<MyClassVM?> TryGetMyClassAsync(string userId);

        Task<List<DevotionRowVM>> LoadGridRowsAsync(int classYear, int classBan, string ym);

        Task<int> AdjustMonthlyTotalAsync(string ym, int personId, int delta);
        Task<DevotionDashboardSummaryVM> GetDashboardSummaryAsync(string ym, int classYear, int classBan);
        Task<List<DevotionTrendPointVM>> GetDashboardTrendAsync(int months, int classYear, int classBan);
        // ✅ 전체 개인 랭킹 (월/학년/반 필터)
        Task<List<DevotionTopVM>> GetParticipantRankAllAsync(string ym, int classYear, int classBan);

        Task<int> GetMyTotalPointsAsync(string userId);
        Task<List<DevotionMyMonthlyPointVM>> GetMyMonthlyPointsAsync(string userId, int months);

        // ✅ 추가
        Task<List<DevotionStudentListItemVM>> LoadMyClassStudentsAsync(string userId);
        Task<List<DevotionYearMonthPointVM>> GetStudentYearPointsAsync(int personId, int year);
        Task<int> SetMonthlyTotalAsync(string ym, int personId, int points);

    }
    public class DevotionMyMonthlyPointVM
    {
        public string Ym { get; set; } = "";   // YYYY-MM
        public int Points { get; set; }
    }
    public class DevotionDashboardSummaryVM
    {
        public int StudentCount { get; set; }
        public int TotalPoints { get; set; }
        public double AvgPoints { get; set; }
        public int MaxPoints { get; set; }

        public List<DevotionClassAggVM> ClassAgg { get; set; } = new(); // 전체 모드일 때만 사용
        public List<DevotionTopVM> Top { get; set; } = new();
    }

    public class DevotionClassAggVM
    {
        public int ClassYear { get; set; }
        public int ClassBan { get; set; }
        public int StudentCount { get; set; }
        public int TotalPoints { get; set; }
        public double AvgPoints { get; set; }
    }

    public class DevotionTopVM
    {
        public int ClassYear { get; set; }
        public int ClassBan { get; set; }
        public string FullName { get; set; } = "";
        public int Points { get; set; }
    }

    public class DevotionTrendPointVM
    {
        public string Ym { get; set; } = ""; // YYYY-MM
        public int TotalPoints { get; set; }
    }
    public class ClassOptionVM
    {
        public int ClassYear { get; set; }
        public int ClassBan { get; set; }
        public string ClassName { get; set; } = "";
    }

    public class MyClassVM
    {
        public int ClassYear { get; set; }
        public int ClassBan { get; set; }
    }

    public class DevotionRowVM
    {
        public int PersonId { get; set; }
        public string FullName { get; set; } = "";
        public int Points { get; set; }
    }
    // ✅ 추가 VM
    public class DevotionStudentListItemVM
    {
        public int PersonId { get; set; }
        public string FullName { get; set; } = "";
        public int ClassYear { get; set; }
        public int ClassBan { get; set; }
        public int TotalPoints { get; set; }
    }

    public class DevotionYearMonthPointVM
    {
        public int Month { get; set; }
        public string Ym { get; set; } = "";
        public int Points { get; set; }
    }
}