using BulletinBoard.Common;
using BulletinBoard.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace YunYunBu2024.Services
{
    public interface IAlbumService
    {
        Task<IReadOnlyList<Album>> GetAlbumsAsync();
        Task<Album> GetAlbumAsync(int id);
        Task<Album> CreateAlbumAsync(string name, string description, string UserName);
        Task<int> UploadAsync(int albumId, IEnumerable<IFormFile> files, string userName, bool avoidDuplicates = true);
        Task DeletePhotoAsync(int photoId);
        Task SetCoverAsync(int albumId, int photoId);
        Task<(Album album, IReadOnlyList<Photo> photos)> GetAlbumWithPhotosAsync(int id, int? take = null);
        Task DeleteAlbumAsync(int albumId);   // ✅ 추가
        Task DeletePhotosAsync(int albumId, IEnumerable<int> photoIds); // ✅ 추가

        // ✅ 새 앨범 화면용 — 앨범마다 사진 수 · 커버 썸네일 · 마지막 올린 날
        Task<IReadOnlyList<AlbumSummary>> GetAlbumSummariesAsync();
        // ✅ 앨범 이름 · 설명 고치기
        Task<bool> UpdateAlbumAsync(int albumId, string name, string description);
    }

    public class AlbumSummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string? UserName { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public int PhotoCount { get; set; }
        public System.DateTime? LastUploadedAt { get; set; }
        public string? CoverUrl { get; set; }   // 썸네일 우선, 없으면 원본 (커버가 없으면 첫 사진)
    }
}
