using BibleReadingThrough.Data;
using BulletinBoard.Areas.Hanmadi.Data;
using BulletinBoard.Data;
using BulletinBoard.DataAccessLayer;
using BulletinBoard.DataAccessLayer.Birthday;
using BulletinBoard.DataAccessLayer.Worldview;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services;
using BulletinBoard.Services.Abstract;
using BulletinBoard.Services.DailyMission2;
using BulletinBoard.Services.Jubo;
using BulletinBoard.Services.ReplayBoard;
using BulletinBoard.Services.Repos;
using BulletinBoard.Services.WorldView.Admin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StockBulletinBoard.Services;
using System;
using System.IO;
using YourApp.Services;
using YunYunBu2024.Services;
using YunYunBu2024.Services.Shorts;

namespace BulletinBoard
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // 서비스 등록
        public void ConfigureServices(IServiceCollection services)
        {
            // DB 로그 설정
            DbLogger.Configure(Configuration);

            // DbContext + SQL Server
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Bible DbContext (AppDbContext — BibleReadingThrough)
            // 운영 DB 호환 수준이 130 미만이라 EF 9 의 OPENJSON(IN 목록) 번역을 쓰면 오류가 난다 → 120 으로 맞춘다
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                                     sql => sql.UseCompatibilityLevel(120)));

            // 한마디(Hanmadi) DbContext
            services.AddDbContext<HanmadiDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // SignalR (한마디 실시간 통신 — M2에서 Hub 연결)
            services.AddSignalR();

            // Identity 설정
            services
                .AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    // 비밀번호 규칙 (기존과 동일하게 아주 단순하게)
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 1;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // ✅ Identity 쿠키 설정 (권한 없을 때 AccessDenied로 이동)
            services.ConfigureApplicationCookie(options =>
            {
                // 로그인 안 한 경우
                options.LoginPath = "/Identity/Account/Login";

                // 로그인 했지만 권한(roles) 없는 경우
                // -> Areas/Identity/Pages/Account/AccessDenied.cshtml
                options.AccessDeniedPath = "/Identity/Account/AccessDenied"; // ✅ 여기 수정

                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;
                // 브라우저 닫아도 쿠키 유지 (모든 로그인에 일괄 적용)
                options.Events = new Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationEvents
                {
                    OnSigningIn = ctx =>
                    {
                        ctx.Properties.IsPersistent = true;
                        ctx.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30);
                        return Task.CompletedTask;
                    }
                };
            });

            // 메모리 캐시
            services.AddDistributedMemoryCache();
            services.AddMemoryCache();

            // DataProtection 키 저장
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(@"./DataProtectionKeys"))
                .SetApplicationName("MyApp");

            // 세션
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = "MyApp.Auth";
            });

            // 운동/헬스 관련 서비스
            services.AddScoped<IFitRepository, FitRepository>();
            services.AddScoped<IFitService, FitService>();
            services.AddScoped<IExerciseRepository, ExerciseRepository>();
            services.AddScoped<IShortsRepository, ShortsRepository>();

            // 앨범/파일/공지/밴드
            services.AddScoped<IAlbumService, AlbumService>();
            services.AddScoped<IFileStorage, LocalFileStorage>();
            services.AddScoped<IZipService, ZipService>();
            services.AddScoped<INoticeService, NoticeService>();
            services.AddScoped<IBandBoardService, BandBoardService>();
            services.AddScoped<IGlobalCookieService, GlobalCookieService>();
            services.AddScoped<IPresentPickService, PresentPickService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<IChurchRecordService, ChurchRecordService>();
            services.AddScoped<IPopMessageService, PopMessageService>();
            services.AddScoped<IAddressBookService, AddressBookService>();
            services.AddScoped<IProfileUpdateSqlService, ProfileUpdateSqlService>();
            services.AddScoped<IDailyEnglishService, DailyEnglishService>();

            // 선물 품목 조회 단일 창구 (가격 규칙은 GiftPriceRule)
            services.AddScoped<BulletinBoard.Services.GiftCatalog.IGiftCatalogService,
                               BulletinBoard.Services.GiftCatalog.GiftCatalogService>();

            // 성경통독 완독 선물
            services.AddScoped<BibleReadingThrough.Services.BibleGift.IBibleGiftService,
                               BibleReadingThrough.Services.BibleGift.BibleGiftService>();

            // 추석맞이 우리 가족 사진 콘테스트
            services.AddScoped<BulletinBoard.Services.FamilyPhoto.IFamilyPhotoService,
                               BulletinBoard.Services.FamilyPhoto.FamilyPhotoService>();

            // 자녀를 위한 365일 축복 기도문 (날짜별 기도문)
            services.AddScoped<BulletinBoard.Services.Prayer365.IPrayer365Service,
                               BulletinBoard.Services.Prayer365.Prayer365Service>();

            // 선물 관리 통합 창구 (도토리 · 가족팀 · 가족합산 · 성경통독)
            services.AddScoped<BulletinBoard.Services.GiftAdmin.IGiftAdminFacade,
                               BulletinBoard.Services.GiftAdmin.GiftAdminFacade>();

            

            // 투표/장소/기도
            services.AddScoped<IDrawingVoteRepository, DrawingVoteRepository>();
            services.AddScoped<IVBSParticipationSurveyService, VBSParticipationSurveyService>();
            services.AddScoped<UiMenuPolicyRepository>();
            services.AddScoped<IUserUiPreferenceService, UserUiPreferenceService>();

            services.AddScoped<WorldviewAdminRepository>();
            services.AddScoped<IWorldviewAdminService, WorldviewAdminService>();
            services.AddScoped<WorldviewLessonRepository>();
            services.AddScoped<IWorldviewLessonService, WorldviewLessonService>();

            services.AddScoped<BulletinBoard.Services.Abstract.IAwardItemService, BulletinBoard.Services.AwardItemService>();
            services.AddScoped<IDevotionPointsService, DevotionPointsService>();

            services.AddScoped<IStudentPointGiftService, BulletinBoard.Services.StudentPointGift.StudentPointGiftService>();
            services.AddScoped<IGiftPopupStateRepo, GiftPopupStateRepo>();

            services.AddScoped<IBirthdayRepository, BirthdayRepository>();
            services.AddScoped<IBirthdayService, BirthdayService>();
            services.AddScoped<IUserSettingsService, UserSettingsService>();
            services.AddScoped<DailyMission2FamilyAdminService>();
            services.AddScoped<IStudentPointTotalRankService, StudentPointTotalRankService>();

            services.AddScoped<IPlaceService, PlaceService>();
            services.AddScoped<IPrayerService, PrayerService>();
            services.AddScoped<IAwardDashboardService, AwardDashboardService>();
            services.AddScoped<IDailyMission2Service, DailyMission2Service>();
            services.AddScoped<ReplayBoardRepository>();

            // 주일학교 주보 (JuboBoard)
            services.AddScoped<IJuboService, JuboService>();

            // CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

            // AuthService
            services.AddTransient<IAuthService, AuthService>();

            // MVC + 글로벌 필터(LogActionFilter)
            services
                .AddControllersWithViews(options =>
                {
                    options.Filters.Add(new LogActionFilter());

                    // ✅ non-nullable 참조타입을 자동 Required로 보지 않도록
                    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                })
                .AddControllersAsServices();

            // Razor Pages (Identity Area 포함)
            services.AddRazorPages();
        }

        // HTTP 파이프라인 구성
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // 검색 엔진 설정 (2026-10-05)
            //  · 공개 주소  = https://www.jamsilkids.com  → 홈("/")만 검색 허용, 나머지는 noindex
            //  · 운영 주소  = *.dtempurl.com · localhost 등 → 전부 검색 제외 (같은 사이트가 둘로 잡히는 중복 방지)
            //  · jamsilkids.com(www 없음) → www 로 영구 이동
            //  · robots.txt · sitemap.xml 은 파일이 아니라 접속 주소에 따라 다르게 내려준다
            //  · UseStaticFiles 보다 앞에 둬야 이미지 · css 응답에도 X-Robots-Tag 가 붙는다
            app.Use(async (context, next) =>
            {
                const string canonicalHost = "www.jamsilkids.com";
                var host = context.Request.Host.Host;
                bool isCanonical = string.Equals(host, canonicalHost, StringComparison.OrdinalIgnoreCase);
                bool isGetOrHead = HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method);
                var path = context.Request.Path.Value ?? "/";

                if (isGetOrHead && string.Equals(host, "jamsilkids.com", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.Redirect("https://" + canonicalHost + context.Request.Path + context.Request.QueryString, permanent: true);
                    return;
                }

                if (isGetOrHead && path.Equals("/robots.txt", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.ContentType = "text/plain; charset=utf-8";
                    context.Response.Headers["Cache-Control"] = "public, max-age=3600";
                    await context.Response.WriteAsync(isCanonical
                        ? "User-agent: *\nDisallow: /Admin\nDisallow: /Identity\nDisallow: /HumanResource\nDisallow: /Present\nDisallow: /LoginHistMgmt\nDisallow: /SuperAdminAnomaly\nDisallow: /DailyMission2FamilyAdmin\nDisallow: /CheckPoint\nDisallow: /hanmadi\n\nSitemap: https://" + canonicalHost + "/sitemap.xml\n"
                        : "User-agent: *\nDisallow: /\n");
                    return;
                }

                if (isGetOrHead && isCanonical && path.Equals("/sitemap.xml", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.ContentType = "application/xml; charset=utf-8";
                    context.Response.Headers["Cache-Control"] = "public, max-age=3600";
                    await context.Response.WriteAsync(
                        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                        "<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n" +
                        "  <url><loc>https://" + canonicalHost + "/</loc></url>\n" +
                        "</urlset>\n");
                    return;
                }

                // 공개 주소의 홈("/")만 색인 허용. 그 밖의 모든 응답(화면 · 이미지 · 스크립트)은 색인 제외.
                if (!(isCanonical && path == "/"))
                    context.Response.Headers["X-Robots-Tag"] = "noindex, nofollow";

                await next();
            });

            app.UseStaticFiles();

            app.UseCors("AllowAllOrigins");

            // 🔐 로그인 · 회원가입 화면 (2026-10-11)
            //  · 화면(GET)은 캐시하지 않는다 — 오래된 위조 방지 토큰이 든 화면이 뒤로 가기로 다시 쓰이는 것을 막는다
            //  · 토큰이 맞지 않아 빈 화면의 400 이 나면(예: 배포 전에 열어 둔 화면) 같은 화면으로 돌려보내고 안내 한 줄을 보여 준다
            app.Use(async (context, next) =>
            {
                var p = context.Request.Path.Value ?? string.Empty;
                bool isAuthPage = p.Equals("/Identity/Account/Login", StringComparison.OrdinalIgnoreCase)
                               || p.Equals("/Identity/Account/Register", StringComparison.OrdinalIgnoreCase);

                if (isAuthPage && HttpMethods.IsGet(context.Request.Method))
                {
                    context.Response.Headers["Cache-Control"] = "no-store, no-cache, max-age=0";
                    context.Response.Headers["Pragma"] = "no-cache";
                }

                await next();

                if (isAuthPage && HttpMethods.IsPost(context.Request.Method)
                    && context.Response.StatusCode == StatusCodes.Status400BadRequest
                    && !context.Response.HasStarted)
                {
                    context.Response.Clear();
                    var returnUrl = context.Request.Query["ReturnUrl"].ToString();
                    var back = p + "?stale=1" + (string.IsNullOrEmpty(returnUrl) ? "" : "&ReturnUrl=" + Uri.EscapeDataString(returnUrl));
                    context.Response.Redirect(back);
                }
            });

            // 📊 방문자 집계 — 홈이 아니라 어느 화면으로 들어와도 하루 1번 센다 (카톡 공유 링크 등). 집계 실패는 내부에서 삼킨다.
            app.Use(async (context, next) =>
            {
                BulletinBoard.Services.SiteVisit.SiteVisitCounter.CountHomeEntry(context);
                await next();
            });

            // DailyMission3 ·StudentPointDashboard3 는 더 이상 쓰지 않는다 (2026-10-03).
            // 북마크 · 공유한 주소 · 저장된 알림 링크로 들어오는 GET 은 4번으로 보낸다. 폼 저장(POST)은 건드리지 않는다.
            app.Use(async (context, next) =>
            {
                if (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method))
                {
                    var p = context.Request.Path;
                    PathString rest;
                    string target = null;
                    if (p.StartsWithSegments("/DailyMission3", out rest)) target = "/DailyMission4";
                    else if (p.StartsWithSegments("/StudentPointDashboard3", out rest)) target = "/StudentPointDashboard4";
                    if (target != null)
                    {
                        context.Response.Redirect(target + rest.Value + context.Request.QueryString.Value);
                        return;
                    }
                }
                await next();
            });

            app.UseRouting();

            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "teacherBooks",
                    pattern: "viewTeacherBooks/{id}",
                    defaults: new { controller = "TeacherBooks", action = "Index" });

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapRazorPages();
                endpoints.MapHub<BulletinBoard.Areas.Hanmadi.Hubs.LiveHub>("/hanmadi-hub");
            });
        }
    }
}
