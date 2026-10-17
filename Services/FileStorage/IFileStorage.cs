using BulletinBoard.Common;
using BulletinBoard.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace YunYunBu2024.Services
{
    public interface IFileStorage
    {
        Task<string> SaveAsync(int albumId, IFormFile file, string fileName);
        // ★ 추가: 스트림으로 저장 (subFolder는 "thumbs" 같은 용도)
        Task<string> SaveAsync(int albumId, Stream stream, string fileName, string subFolder = null);

        Task<Stream> OpenReadAsync(string storagePath);
        void Delete(string storagePath);
        string MapPhysicalPath(string storagePath);
    }
}
