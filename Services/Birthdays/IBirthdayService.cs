using System.Threading.Tasks;
using BulletinBoard.ViewModels.Birthday;

namespace BulletinBoard.Services.Abstract
{
    public interface IBirthdayService
    {
        Task<BirthdayListPageVM> GetMonthAsync(int month);
        Task<BirthdayListPageVM> GetYearAsync();
        Task<int> UpdateGiftStateAsync(
    string userId,
    bool isActive,
    bool isSent
);
        Task<int> UpdatePickSmsStateAsync(string userId, bool smsSent);
        // ✅ 메모: BirthdayGiftPickMeMo (PersonID 키)
        Task<int> UpsertMemoAsync(int personId, string? memo);
        Task<BirthdayEditVM?> GetEditAsync(int personId);
        Task<BirthdayListPageVM> GetNoBirthdayAsync();
    }
}
