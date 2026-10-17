using BulletinBoard.Common;
using BulletinBoard.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using YunYunBu2024.Services;

 

namespace YunYunBu2024.Services
{
    public class ZipService : IZipService
    {
        public async Task<MemoryStream> CreateZipAsync(IEnumerable<(string EntryName, Stream Source)> items)
        {
            var ms = new MemoryStream();
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                foreach (var (entryName, source) in items)
                {
                    var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                    using var entryStream = entry.Open();
                    source.Position = 0;
                    await source.CopyToAsync(entryStream);
                }
            }
            ms.Position = 0;
            return ms;
        }
    }
}