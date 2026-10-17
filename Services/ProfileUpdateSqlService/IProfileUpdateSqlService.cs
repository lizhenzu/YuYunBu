using System.Threading.Tasks;
using BulletinBoard.ViewModels.Profile;

namespace BulletinBoard.Services.Abstract
{
    public interface IProfileUpdateSqlService
    {
        Task<ProfileEditVM> GetEditAsync(string userId);
        Task<(bool ok, string message, bool personUpdated)> SaveAsync(ProfileEditVM vm, string loginUserId);
    }
}
