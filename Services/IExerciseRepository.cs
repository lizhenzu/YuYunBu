using System.Collections.Generic;
using System.Threading.Tasks;
using BulletinBoard.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulletinBoard.Services
{

    public interface IExerciseRepository
    {
        Task<(IEnumerable<ExerciseSummary> Items, int Total)> GetExercisesAsync(ExerciseFilter filter);
        Task<ExerciseDetail?> GetExerciseDetailAsync(int id);
    }
}
