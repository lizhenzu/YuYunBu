// Services/IPlaceService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using BulletinBoard.Models;

namespace BulletinBoard.Services
{
    public interface IPlaceService
    {
        Task<IReadOnlyList<PlaceItem>> GetListAsync(string category, string? q = null);
        Task<PlaceItem?> GetAsync(int placeId);
        Task<int> CreateAsync(PlaceItemInput input);
        Task<int> UpdateAsync(PlaceItemInput input);
        Task<int> DeleteAsync(int placeId);
        Task<Dictionary<string, int>> GetCountsAsync();
    }
}
