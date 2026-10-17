// Data/Fitness/FitRepository.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Configuration; // .NET Framework일 때
using Microsoft.Extensions.Configuration; // .NET Core일 때 주석 해제

using BulletinBoard.Models;

namespace BulletinBoard.Services
{
    public interface IFitRepository
    {
        Task<PagedResult<ExerciseDocListItem>> GetExerciseDocsAsync(int page, int pageSize, string keyword);
        Task IncrementVisitedAsync(int exerciseDocId);
        Task<ExerciseDocDetail> GetExerciseDocDetailAsync(int exerciseDocId); // null 반환 가능
    }

   
}
