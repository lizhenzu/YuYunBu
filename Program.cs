using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BulletinBoard.Data;
using BulletinBoard.Models;
using BibleReadingThrough.Data;

namespace BulletinBoard
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            // ⚡ 이 앱은 동기 ADO.NET 조회를 Task.Run 으로 병렬 실행하는 곳이 많다.
            //    동기 호출은 DB 응답을 기다리는 동안 워커 스레드를 계속 붙잡고 있는데,
            //    스레드풀 기본 최소 워커 수는 CPU 코어 수라 금방 바닥난다.
            //    부족하면 .NET 이 초당 1~2개씩만 워커를 늘리므로, 병렬로 던진 조회들이
            //    실제로는 계단식으로 순차 실행돼 홈 화면이 1초 이상 느려졌다.
            //    최소 워커를 미리 확보해 이 지연을 없앤다.
            System.Threading.ThreadPool.GetMinThreads(out var minW, out var minIo);
            System.Threading.ThreadPool.SetMinThreads(Math.Max(minW, 64), Math.Max(minIo, 64));

            var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    await ContextSeed.SeedRolesAsync(userManager, roleManager);
                    await ContextSeed.SeedSuperAdminAsync(userManager, roleManager);
                }
                catch (Exception ex)
                {
                    var logger = loggerFactory.CreateLogger<Program>();
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }

                // Bible DB 초기화 — 실패해도 앱은 계속 실행
                try
                {
                    var db = services.GetRequiredService<AppDbContext>();
                    BibleDataSeeder.EnsureReadingProgressTable(db);
                    await BibleDataSeeder.SeedAsync(db);
                }
                catch (Exception ex)
                {
                    var logger = loggerFactory.CreateLogger<Program>();
                    logger.LogError(ex, "Bible DB 초기화 중 오류 발생 — 앱은 계속 실행됩니다.");
                }

                // 방문자 집계 테이블 — 실패해도 앱은 계속 실행
                try
                {
                    BulletinBoard.Services.SiteVisit.SiteVisitCounter.EnsureTable(services.GetRequiredService<AppDbContext>());
                }
                catch (Exception ex)
                {
                    var logger = loggerFactory.CreateLogger<Program>();
                    logger.LogError(ex, "방문자 집계 테이블 준비 중 오류 발생 — 앱은 계속 실행됩니다.");
                }
            }
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
