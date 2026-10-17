using System.Collections.Generic;
using System.Threading.Tasks;
using BulletinBoard.DataAccessLayer.Birthday.Models;

namespace BulletinBoard.DataAccessLayer.Birthday
{
    public interface IBirthdayRepository
    {
        Task<List<ViewPersonMaster2026Row>> GetListAsync(int? month);
        Task<ViewPersonMaster2026Row?> GetOneAsync(int personId);
        Task<int> UpdateGiftStateAsync(string userId,  bool isActive, bool isSent);
        Task<List<ViewPersonMaster2026Row>> GetNoBirthdayListAsync();
        Task<int> UpdatePickSmsStateAsync(string userId, bool smsSent);
        // ✅ 메모: BirthdayGiftPickMeMo (PersonID 키)
        Task<int> UpsertMemoAsync(int personId, string? memo);
    }
}
