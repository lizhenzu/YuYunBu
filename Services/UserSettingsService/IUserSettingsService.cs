using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Enums;
namespace BulletinBoard.Services
{
    public interface IUserSettingsService
    {
        Task<string> GetLangAsync(string userId);
        Task SaveLangAsync(string userId, string lang);
        Task<bool> IsCookieSuppressedAsync(string userId, string cookieName);
        Task UpsertCookieSuppressionAsync(string userId, string cookieName, string cookieValue, int cookieDays);
    }
}
