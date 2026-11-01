using BibleReadingThrough.Data;
using BibleReadingThrough.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BulletinBoard.Helpers
{
    /// <summary>
    /// 성경통독 하루 읽기 분량 제한 (/Bible, /Bible2 공통)
    /// - 대상: 로그인 계정이 명확히 학생(RoleCode = T1001)일 때만. 그 외(교사·관리자·미등록)는 제한 없음
    /// - 오늘 읽은 장 = 오늘(KST) 읽음 표시한 절이 하나라도 있는 장
    /// - 읽음 취소는 언제나 가능
    /// </summary>
    public static class BibleDailyLimitHelper
    {
        public const int MaxChaptersPerDay = 20;
        private const string StudentRoleCode = "T1001";   // 학생

        /// <summary>
        /// targetVerses 를 읽음으로 표시해도 되는지 확인.
        /// 막아야 하면 안내 문구, 괜찮으면 null.
        /// </summary>
        public static async Task<string?> CheckAsync(AppDbContext db, string userId, string? userName, IQueryable<EBible> targetVerses)
        {
            if (!await IsStudentAsync(db, userId, userName)) return null;

            var today    = DateHelper.GetKoreaNow().Date;
            var tomorrow = today.AddDays(1);

            var todayChapters = await db.UserBibleProgresses
                .Where(p => p.UserId == userId && p.ReadDate >= today && p.ReadDate < tomorrow)
                .Join(db.EBibles, p => p.VerseId, e => e.Id, (p, e) => new { e.BibleText, e.Jang })
                .Distinct()
                .ToListAsync();

            // 이번에 새로 읽음 표시될 절이 있는 장
            var targetChapters = await targetVerses
                .Where(e => !db.UserBibleProgresses.Any(p => p.UserId == userId && p.VerseId == e.Id))
                .Select(e => new { e.BibleText, e.Jang })
                .Distinct()
                .ToListAsync();

            int newChapters = targetChapters.Count(t => !todayChapters.Contains(t));
            if (newChapters == 0) return null;

            int remain = MaxChaptersPerDay - todayChapters.Count;
            if (newChapters <= remain) return null;

            return remain <= 0
                ? $"오늘은 {MaxChaptersPerDay}장을 다 읽었어요! 내일 또 만나요 😊"
                : $"하루에 {MaxChaptersPerDay}장까지 읽을 수 있어요. 오늘은 {remain}장 남았어요.";
        }

        /// <summary>사람 정보 뷰에서 찾은 계정의 역할이 모두 학생일 때만 true</summary>
        private static async Task<bool> IsStudentAsync(AppDbContext db, string userId, string? userName)
        {
            try
            {
                var roles = await db.UserComposites.AsNoTracking()
                    .Where(u => u.UserId == userId || (userName != null && u.UserName == userName))
                    .Select(u => u.RoleCode)
                    .ToListAsync();

                return roles.Count > 0
                    && roles.All(r => string.Equals(r, StudentRoleCode, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;   // 확인 못 하면 제한하지 않음
            }
        }
    }
}
