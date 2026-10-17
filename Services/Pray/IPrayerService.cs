using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Enums;
using System;

namespace BulletinBoard.Services.Abstract
{
    public interface IPrayerService
    {
        Task<PrayerArrow?> GetTodayAsync(DateTime? now = null);
        Task<PrayerArrow?> GetByDayIndexAsync(int dayIndex);
        Task<List<PrayerArrow>> SearchAsync(int? dayIndex = null, string? keyword = null, bool? isActive = true, string? orderBy = null, int? page = null, int? pageSize = null);
        Task<PrayerArrow?> GetByIdAsync(int id);
        Task<int> UpdateAsync(PrayerArrow item);
    }
}
