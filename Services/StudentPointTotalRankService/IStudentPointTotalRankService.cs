using BulletinBoard.ViewModels.Dashboard;

namespace BulletinBoard.Services.Abstract
{
    public interface IStudentPointTotalRankService
    {
        Task<StudentPointTotalRankVM> GetTotalRankAsync(int year);
    }
}
