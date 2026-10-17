using BulletinBoard.Common;
using BulletinBoard.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YunYunBu2024.Services.Shorts
{
    public interface IShortsRepository
    {
        Task<List<ShortVideo>> GetListAsync(string? q, VideoKindOption? param);
        Task<ShortVideo?> FindAsync(int id);
        Task<bool> ExistsByYoutubeIdAsync(string youtubeId, int? excludeId = null);
        Task<int> CreateAsync(ShortVideoInput input);
        Task<bool> UpdateAsync(ShortVideoInput input);
        Task<bool> DeleteAsync(int id);
        Task<Dictionary<VideoKindOption, int>> GetCountsByVideoKindAsync(bool onlyActive = true);
    }
}
