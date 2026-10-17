using BibleReadingThrough.Data;
using BulletinBoard.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BulletinBoard.Services.SiteVisit
{
    /// <summary>
    /// 사이트(https://www.jamsilkids.com/) 방문자 수 — 하루 단위 · 누적 (2026-10-05 설계 확정, 2026-10-11 홈 외 화면 직접 진입도 포함)
    ///  · 방문자 = 그날 사이트에 처음 들어온 "브라우저" 하나 (어느 화면이든).  같은 브라우저가 하루에 몇 번 들어와도 1.
    ///  · 누가 들어왔는지는 저장하지 않는다 (날짜 + 숫자만).  중복 방지는 한국 시간 자정에 지워지는 익명 쿠키 jk_v 하나.
    ///  · 누적 = 하루 방문자의 합 (따로 저장하지 않고 계산)
    ///  · 세지 않는 것: 검색봇 · 미리보기 수집기 · prefetch · ajax · 운영 주소(dtempurl) · 로컬 · 대기 화면 뒤의 ?_full=1
    ///  · 집계가 실패해도 홈 화면은 그대로 열려야 하므로 모든 오류는 삼킨다
    /// </summary>
    public static class SiteVisitCounter
    {
        public const string PublicHost = "www.jamsilkids.com";
        private const string CookieName = "jk_v";

        // ── 테이블 자동 생성 (앱 시작 때 한 번) ──────────────────────
        public static void EnsureTable(AppDbContext db)
        {
            db.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (
                    SELECT * FROM sys.objects
                    WHERE object_id = OBJECT_ID(N'[dbo].[SiteDailyVisit]') AND type = 'U'
                )
                BEGIN
                    CREATE TABLE [dbo].[SiteDailyVisit] (
                        [VisitDate]  DATE NOT NULL,
                        [VisitCount] INT  NOT NULL CONSTRAINT DF_SiteDailyVisit_Count DEFAULT 0,
                        CONSTRAINT [PK_SiteDailyVisit] PRIMARY KEY CLUSTERED ([VisitDate] ASC)
                    )
                END
            ");
        }

        // ── 홈 진입 한 번 세기 ───────────────────────────────────────
        public static void CountHomeEntry(HttpContext ctx)
        {
            try
            {
                if (!ShouldCount(ctx.Request)) return;

                var today = DateHelper.GetKoreaToday();
                var todayKey = today.ToString("yyyyMMdd");

                // 오늘 이미 셌으면 끝
                if (ctx.Request.Cookies.TryGetValue(CookieName, out var seen) && seen == todayKey) return;

                // 한국 시간 자정에 만료
                var expires = new DateTimeOffset(today.AddDays(1), TimeSpan.FromHours(9));
                ctx.Response.Cookies.Append(CookieName, todayKey, new CookieOptions
                {
                    Expires = expires,
                    HttpOnly = true,
                    Secure = ctx.Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    IsEssential = true
                });

                // 응답을 기다리게 하지 않고 뒤에서 처리 (요청이 끝나면 DbContext 가 사라지므로 새 scope 를 쓴다)
                var factory = ctx.RequestServices.GetRequiredService<IServiceScopeFactory>();
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = factory.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        await db.Database.ExecuteSqlInterpolatedAsync($@"
                            MERGE [dbo].[SiteDailyVisit] WITH (HOLDLOCK) AS t
                            USING (SELECT CAST({today} AS DATE) AS VisitDate) AS s ON t.VisitDate = s.VisitDate
                            WHEN MATCHED THEN UPDATE SET VisitCount = t.VisitCount + 1
                            WHEN NOT MATCHED THEN INSERT (VisitDate, VisitCount) VALUES (s.VisitDate, 1);");
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            var logger = factory.CreateScope().ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("SiteVisit");
                            logger?.LogWarning(ex, "방문자 집계 저장 실패 — 홈 화면에는 영향 없음");
                        }
                        catch { }
                    }
                });
            }
            catch { /* 집계 때문에 홈이 깨지면 안 된다 */ }
        }

        // 세어도 되는 요청인가: 공개 주소 · 브라우저 · 화면 요청 · 대기 화면 뒤의 두 번째 요청이 아님
        private static bool ShouldCount(HttpRequest req)
        {
            if (!HttpMethods.IsGet(req.Method)) return false;
            if (!string.Equals(req.Host.Host, PublicHost, StringComparison.OrdinalIgnoreCase)) return false;
            if (req.Query.ContainsKey("_full")) return false;
            var accept = req.Headers["Accept"].ToString();
            if (string.IsNullOrEmpty(accept) || accept.IndexOf("text/html", StringComparison.OrdinalIgnoreCase) < 0) return false;

            if (string.Equals(req.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)) return false;
            var purpose = req.Headers["Sec-Purpose"].ToString() + req.Headers["Purpose"].ToString();
            if (purpose.IndexOf("prefetch", StringComparison.OrdinalIgnoreCase) >= 0) return false;

            var ua = req.Headers["User-Agent"].ToString();
            if (string.IsNullOrEmpty(ua)) return false;
            string[] bots = { "bot", "spider", "crawl", "kakaotalk-scrap", "facebookexternalhit", "slurp", "yeti", "daum", "preview", "whatsapp", "telegram", "line/" };
            foreach (var b in bots)
                if (ua.IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0) return false;

            return true;
        }

        // ── 관리자 화면용 집계 ───────────────────────────────────────
        public static async Task<SiteVisitStats> GetStatsAsync(AppDbContext db)
        {
            var rows = new List<(DateTime Date, int Count)>();

            var conn = db.Database.GetDbConnection();
            var wasClosed = conn.State != ConnectionState.Open;
            if (wasClosed) await conn.OpenAsync();
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT VisitDate, VisitCount FROM [dbo].[SiteDailyVisit] ORDER BY VisitDate";
                using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                    rows.Add((rd.GetDateTime(0).Date, rd.GetInt32(1)));
            }
            finally
            {
                if (wasClosed) await conn.CloseAsync();
            }

            var today = DateHelper.GetKoreaToday();
            var map = rows.ToDictionary(r => r.Date, r => r.Count);

            var s = new SiteVisitStats
            {
                Today = map.TryGetValue(today, out var t) ? t : 0,
                Yesterday = map.TryGetValue(today.AddDays(-1), out var y) ? y : 0,
                Total = rows.Sum(r => r.Count),
                FirstDate = rows.Count > 0 ? rows[0].Date : (DateTime?)null,
                DaysCounted = rows.Count
            };

            // 최근 30일 (오늘 포함, 최신이 위)
            for (int i = 0; i < 30; i++)
            {
                var d = today.AddDays(-i);
                s.Recent.Add(new SiteVisitDay { Date = d, Count = map.TryGetValue(d, out var c) ? c : 0 });
            }

            // 월별 합계 (최신이 위)
            s.Months = rows.GroupBy(r => new DateTime(r.Date.Year, r.Date.Month, 1))
                           .OrderByDescending(g => g.Key)
                           .Select(g => new SiteVisitMonth { Month = g.Key, Count = g.Sum(x => x.Count), Days = g.Count() })
                           .ToList();
            return s;
        }
    }

    public class SiteVisitStats
    {
        public int Today { get; set; }
        public int Yesterday { get; set; }
        public int Total { get; set; }
        public DateTime? FirstDate { get; set; }
        public int DaysCounted { get; set; }
        public List<SiteVisitDay> Recent { get; set; } = new List<SiteVisitDay>();
        public List<SiteVisitMonth> Months { get; set; } = new List<SiteVisitMonth>();
    }

    public class SiteVisitDay { public DateTime Date { get; set; } public int Count { get; set; } }
    public class SiteVisitMonth { public DateTime Month { get; set; } public int Count { get; set; } public int Days { get; set; } }
}
