using BulletinBoard.Common;
using BulletinBoard.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BulletinBoard.Services
{
    public interface IPresentPickService
    {
        Task<PresentItemPick?> GetMyBirthPickAsync(string userId);
        Task SaveBirthPickAsync(string userId, AwardItem item);   // overwrite
        Task CancelBirthPickAsync(string userId);                 // active -> inactive
        Task<BirthPickAdminListVM> GetAdminBirthPicksAsync(string? q, bool includeInactive, IWebHostEnvironment _hostingEnvironment);
        Task<UpcomingBirthdayListVM> GetUpcomingBirthdaysAsync(int withinDays, string filter , IWebHostEnvironment hostingEnvironment);
        Task<HashSet<int>> GetPickedProductIdsAsync();
        Task SetBirthPickSentAsync(long pickId, bool isSent);
        Task SetBirthPickPurchasedAsync(long pickId, bool isPurchased);

        /// <summary>생일자 미처리 보류 상태 토글 (BirthdayGiftSkip 테이블 upsert)</summary>
        Task SetBirthdaySkipAsync(int personId, bool isSkipped, string? updatedBy);
    }
}
