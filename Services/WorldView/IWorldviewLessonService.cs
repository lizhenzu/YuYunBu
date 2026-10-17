using BulletinBoard.ViewModels.Worldview;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulletinBoard.Services.Abstract
{
    public interface IWorldviewLessonService
    {
        Task<WorldviewCheckResult> CheckBlanksAsync(
            int lessonNo,
            Dictionary<string, string[]> answers,
            string? userId = null,
            string? userName = null);

        Task<WorldviewLessonVM> GetLessonAsync(int lessonNo);

        Task<LessonSubmitResult> SubmitLessonAsync(
            int lessonNo,
            Dictionary<string, string[]> answers,
            string? userId,
            string? userName);

        // ✅ 대시보드 상태 맵 (LessonDashStatusVM으로 통일)
        Task<Dictionary<int, LessonDashStatusVM>> GetDashboardStatusMapAsync(
            string? userId,
            string? userName);
    }
}
