using BulletinBoard.Common;
using BulletinBoard.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace YunYunBu2024.Services
{
    public class LocalFileStorage : IFileStorage
    {
        private readonly string _root;

        public LocalFileStorage(IWebHostEnvironment env)
        {
            // 물리 루트: wwwroot/uploads
            _root = Path.Combine(env.WebRootPath, "uploads");
            Directory.CreateDirectory(_root);
        }

        /// <summary>
        /// 원본 파일 저장 (/uploads/{albumId}/{fileName})
        /// </summary>
        public async Task<string> SaveAsync(int albumId, IFormFile file, string fileName)
        {
            // 물리 폴더: wwwroot/uploads/{albumId}
            var albumPath = Path.Combine(_root, albumId.ToString());
            Directory.CreateDirectory(albumPath);

            var fullPath = Path.Combine(albumPath, fileName);
            using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(fs);
            }

            // 가상 경로: /uploads/{albumId}/{fileName}
            var storagePath = $"/uploads/{albumId}/{fileName}";
            return storagePath.Replace("\\", "/");
        }

        /// <summary>
        /// 스트림(썸네일 등) 저장 (/uploads/{albumId}/[subFolder]/{fileName})
        /// </summary>
        public async Task<string> SaveAsync(int albumId, Stream stream, string fileName, string subFolder = null)
        {
            // ❗여기서 더 이상 "uploads" 를 한 번 더 붙이지 않음
            // 물리 폴더: wwwroot/uploads/{albumId}[/subFolder]
            var baseFolder = Path.Combine(_root, albumId.ToString());
            if (!string.IsNullOrEmpty(subFolder))
                baseFolder = Path.Combine(baseFolder, subFolder);

            Directory.CreateDirectory(baseFolder);

            var physicalPath = Path.Combine(baseFolder, fileName);
            using (var fs = new FileStream(physicalPath, FileMode.Create, FileAccess.Write))
            {
                await stream.CopyToAsync(fs);
            }

            // 가상 경로: /uploads/{albumId}[/subFolder]/{fileName}
            var virtualPath = $"/uploads/{albumId}"
                              + (string.IsNullOrEmpty(subFolder) ? "" : $"/{subFolder}")
                              + $"/{fileName}";

            return virtualPath.Replace("\\", "/");
        }

        public async Task<Stream> OpenReadAsync(string storagePath)
        {
            var full = MapPhysicalPath(storagePath);

            var ms = new MemoryStream();
            using (var fs = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                await fs.CopyToAsync(ms);
            }
            ms.Position = 0;
            return ms;
        }

        public void Delete(string storagePath)
        {
            var full = MapPhysicalPath(storagePath);
            if (File.Exists(full))
            {
                File.Delete(full);
            }
        }

        /// <summary>
        /// /uploads/... 형태의 가상 경로를 물리 경로(wwwroot/uploads/...)로 변환
        /// </summary>
        public string MapPhysicalPath(string storagePath)
        {
            if (string.IsNullOrWhiteSpace(storagePath))
                throw new ArgumentNullException(nameof(storagePath));

            var path = storagePath.Replace('\\', '/');

            // 절대 URL이면 여기서 막아줌 (필요 시 조정)
            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Absolute URLs cannot be mapped to a local path.");
            }

            // 앞의 / 제거 -> "uploads/81/abc.jpg"
            if (path.StartsWith("/"))
                path = path.Substring(1);

            // "uploads/" 접두어 제거 후 나머지를 _root 뒤에 붙임
            const string uploadsPrefix = "uploads/";
            if (path.StartsWith(uploadsPrefix, StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(uploadsPrefix.Length); // "81/abc.jpg" 또는 "81/thumbs/abc.jpg"
            }

            // 최종 물리 경로: wwwroot/uploads/81/abc.jpg
            return Path.Combine(_root, path.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
