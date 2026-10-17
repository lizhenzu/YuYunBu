using BulletinBoard.Common;
using BulletinBoard.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace YunYunBu2024.Services
{
    public interface IZipService
    {
        Task<MemoryStream> CreateZipAsync(IEnumerable<(string EntryName, Stream Source)> items);
    }
}
