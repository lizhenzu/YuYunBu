// Services/Fitness/FitService.cs
using BulletinBoard.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing.Printing;
using System.Threading.Tasks; 


namespace BulletinBoard.Services
{ 
    public class FitService : IFitService
    {
        private readonly IFitRepository _repo;
        public FitService(IFitRepository repo)
        {
            _repo = repo;
        }
        public Task IncrementVisitedAsync(int exerciseDocId)
        {
            return _repo.IncrementVisitedAsync(exerciseDocId);
        }
        public Task<PagedResult<ExerciseDocListItem>> GetListAsync(int page, int pageSize, string keyword)
        {
            return _repo.GetExerciseDocsAsync(page, pageSize, keyword);
        }

        public Task<ExerciseDocDetail> GetDetailAsync(int id)
        {
            return _repo.GetExerciseDocDetailAsync(id);
        }
    }
}
