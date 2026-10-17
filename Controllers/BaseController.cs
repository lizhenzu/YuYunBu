using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IAuthService? _authService;
        protected readonly UserManager<ApplicationUser>? _userManager;
        protected readonly IWebHostEnvironment? _hostingEnvironment;

        // ✅ 읽기는 어디서나, set은 상속(파생 컨트롤러)에서만 가능
        public string UserId { get; protected set; } = string.Empty;
        public string UserName { get; protected set; } = string.Empty;
        public string UserFullName { get; protected set; } = string.Empty;

        // ✅ 0-인자 생성자(상속 컨트롤러에 생성자 없을 때도 컴파일 에러 방지)
        public BaseController()
        {
        }

        // ✅ DI 생성자 (env 포함)
        public BaseController(
            IAuthService authService,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment hostingEnvironment)
        {
            _authService = authService;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        // ✅ DI 생성자 (env 없음) - 기존 컨트롤러 호환
        public BaseController(
            IAuthService authService,
            UserManager<ApplicationUser> userManager)
        {
            _authService = authService;
            _userManager = userManager;
            _hostingEnvironment = null;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var sessionFullName = HttpContext.Session.GetString(HttpContextSessionCategory.UserFullName.ToString());
            var sessionUserName = HttpContext.Session.GetString(HttpContextSessionCategory.UserName.ToString());
            var sessionUserId   = HttpContext.Session.GetString(HttpContextSessionCategory.UserId.ToString());
            var deptCode        = HttpContext.Session.GetString(HttpContextSessionCategory.DeptCode.ToString());
            var deptNM          = HttpContext.Session.GetString(HttpContextSessionCategory.DeptNM.ToString());

            // 세션에 이미 있으면 그대로 사용 (가장 포함 — SignInAsync 후 세션 갱신됨)
            if (!string.IsNullOrEmpty(sessionFullName))
            {
                ApplyUserContext(sessionUserId, sessionUserName, sessionFullName, deptCode, deptNM);
            }
            // 없으면 현재 Identity 클레임 기준으로 채움
            else
            {
                await SetUserInfo();
            }

            // 4) 공통 메타데이터(교회명/부서명 등) 세션 보장
            await EnsureCommonMetaAsync();

            // 5) 다음 액션 실행
            await next();
        }

        /// <summary>
        /// 공통 메타데이터(교회명/부서명 등)가 세션에 없으면 채움
        /// </summary>
        protected async Task EnsureCommonMetaAsync()
        {
            var churchName = HttpContext.Session.GetString(CommonMetaDataCategory.ChurchName.ToString());
            if (!string.IsNullOrEmpty(churchName))
                return;

            var manageeCommon = new ManageeCommon();
            List<CommonMetaData> meta = manageeCommon.getCommonMetaData();

            foreach (var va in meta)
            {
                ViewData[va.Kind.ToString()] = va.KindDesc;
                HttpContext.Session.SetString(va.Kind.ToString(), va.KindDesc);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 로그인 유저 기반으로 세션/필드 세팅 (DI 미주입이면 안전하게 종료)
        /// </summary>
        protected async Task<string> SetUserInfo()
        {
            if (_authService == null || _userManager == null)
                return "DI_NOT_READY";

            if (!await _authService.IsSignedIn(User))
                return "NOT_SIGNED_IN";

            // 현재 로그인된 Identity 클레임 기준 (가장 중이면 이미 SignInAsync로 교체된 상태)
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                ApplyUserContext(
                    user.Id,
                    user.UserName,
                    $"{user.LastName}{user.FirstName}",
                    "", ""
                );
            }

            return "OK";
        }

        /// <summary>
        /// 실제 세션/뷰데이터/필드 반영
        /// </summary>
        protected void ApplyUserContext(string? userID, string? userName, string? userFullName, string? deptCode, string? deptNM)
        {
            UserId = userID ?? "";
            UserName = userName ?? "";
            UserFullName = userFullName ?? "";

            ViewData[HttpContextSessionCategory.UserFullName.ToString()] = UserFullName;
            ViewData[HttpContextSessionCategory.UserId.ToString()] = UserId;
            ViewData[HttpContextSessionCategory.UserName.ToString()] = UserName;

            HttpContext.Session.SetString(HttpContextSessionCategory.UserFullName.ToString(), UserFullName);
            HttpContext.Session.SetString(HttpContextSessionCategory.UserId.ToString(), UserId);
            HttpContext.Session.SetString(HttpContextSessionCategory.UserName.ToString(), UserName);

            if (!string.IsNullOrEmpty(deptCode))
                HttpContext.Session.SetString(HttpContextSessionCategory.DeptCode.ToString(), deptCode);

            if (!string.IsNullOrEmpty(deptNM))
                HttpContext.Session.SetString(HttpContextSessionCategory.DeptNM.ToString(), deptNM);
        }
    }
}
