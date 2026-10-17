using BulletinBoard.Common;
using BulletinBoard.Models;
using Microsoft.AspNetCore.Http;

namespace BulletinBoard.Services.Abstract
{
    public interface IAwardItemService
    {
        Task<List<AwardItem>> GetAllAsync(ProductDisplayOpton option, bool includeHidden);
        Task<AwardItem?> GetByIdAsync(int productId, bool includeHidden);
        Task<int> CreateAsync(AwardItem item);
        Task UpdateAsync(AwardItem item);

        // ✅ 값 지정(기존 유지)
        Task SetHiddenAsync(int productId, bool isHidden);

        // ✅ 서버에서 직접 토글
        Task ToggleHiddenAsync(int productId);

        Task DeleteAsync(int productId);

        Task SaveImageJpgAsync(int productId, IFormFile imageFile, string webRootPath);
        Task DeleteImageAsync(int productId, string webRootPath);
    }
}
