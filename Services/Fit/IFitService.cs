// Services/Fitness/FitService.cs
using BulletinBoard.Models;
using System.Threading.Tasks;
 


namespace BulletinBoard.Services
{
    public interface IFitService
    {
        Task<PagedResult<ExerciseDocListItem>> GetListAsync(int page, int pageSize, string keyword);
        Task IncrementVisitedAsync(int exerciseDocId);
        Task<ExerciseDocDetail> GetDetailAsync(int id); // null 가능
    }

    
}
