using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.DataAccessLayer;
using BulletinBoard.Enums;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using BulletinBoard.ViewModels;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    [Authorize]
    public class HumanResourceController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public HumanResourceController(IAuthService authService,
      UserManager<ApplicationUser> userManager,
      IWebHostEnvironment hostingEnvironment) : base(authService, userManager, hostingEnvironment)
        {
            _authService = authService;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: JobCategory
        public async Task<IActionResult> Index()
        {
            ViewData["ClassYear"] = 0;
            ViewData["ClassBan"] = 0;
            ViewData["BanMode"] = "Index";
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> ViewMainBirthDay()
        {

            ManageDeptInfo manageDeptInfo = new ManageDeptInfo();
            List<CodeData> codeDatas = manageDeptInfo.getCodeData("D001");
            return View(codeDatas);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBirth(BirthConfirmVM model)
        {
            if (string.IsNullOrWhiteSpace(model.UserId))
                return BadRequest();


            // ⏳ 다음에 할게요(n)면 생일 검증 제거
            if (model.birthDay_PublishYN == "n" || model.birthDay_PublishYN == "h")
            {
                ModelState.Remove(nameof(model.BMonth));
                ModelState.Remove(nameof(model.BDay));
            }
            else
            {
                // ✅ 기본 검증
                if (model.BMonth < 1 || model.BMonth > 12 || model.BDay < 1 || model.BDay > 31)
                {
                    ModelState.AddModelError("", "생일(월/일)을 다시 확인해 주세요.");
                    return View(model);
                }
            }

            // ✅ 저장 로직(프로젝트 방식에 맞게 구현)
            // 예시:
            // - DB의 UserInfos(또는 Users 테이블) bMonth/bDay 업데이트
            // UpdateUserBirthday(model.UserId, model.BMonth, model.BDay);

            ControlHumanResource controlHumanResource = new ControlHumanResource();
            // Save the image as a binary to the ProfilePicture field using dynamic SQL
            await controlHumanResource.SavePersonMasterBirthAsync(model.UserId, model.birthDay_PublishYN, model.BMonth, model.BDay);

            // 홈 생일 목록은 10분 캐시라 그냥 두면 고친 생일이 한동안 반영되지 않는다
            BulletinBoard.Helpers.HomeCacheHelper.InvalidateBirthdays(HttpContext);

            // ✅ 저장 후 선물 선택 화면으로 이동
            var redirect = string.IsNullOrWhiteSpace(model.ReturnUrl)
                ? $"/Present/Birth?userid={model.UserId}"
                : model.ReturnUrl;

            return Redirect("/Home/Index");
        }



        [HttpGet]
       public async Task<IActionResult> ConfirmBirth(string UserId,
       string? returnUrl)
        {

            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();
            }       
            
            if(string.IsNullOrEmpty(UserId))
            {
                UserId = this.UserId;
            }

            // ✅ 여기에서 "홍길동 + D1001 + StudentOnly"로 1명 조회
            ManageHumanResource manageHumanResourceA = new ManageHumanResource();
            List<UserInfo> users = manageHumanResourceA.getUserInfos(
                _hostingEnvironment,
               UserId,
               string.Empty,
                0, 0, 0,
                string.Empty,
                string.Empty,
                RankQueryOption.None);

            var u = users.FirstOrDefault(); // 이름이 정확히 1명 나온다고 가정 (여러명이면 정책 필요)

            if (u == null || string.IsNullOrWhiteSpace(u.UserId))
            {
                // 프로젝트에 맞게 처리 (에러 페이지/문구 표시)
                return Content("해당 이름의 학생 정보를 찾지 못했습니다.");
            }

            var giftUrl = string.IsNullOrWhiteSpace(returnUrl)
                ? $"/Present/Birth?userid={u.UserId}"
                : returnUrl;

            var vm = new BirthConfirmVM
            {
                UserId = u.UserId,
                FullName = u.FullName,
                UserName = u.UserName,
                ProfilePicturePath = u.ProfilePicturePath,
                BMonth = (u.bMonth > 0 ? u.bMonth : 0),
                BDay = (u.bDay > 0 ? u.bDay : 0),
                birthDay_PublishYN =u.birthDay_PublishYN,
                ReturnUrl = giftUrl
            };

            return View(vm);
        }



        public async Task<IActionResult> printStudentCard(string DeptCode, int ClassYear,int ClassBan)
        {
            ManageHumanResource manageHumanResource = new ManageHumanResource();

            if (string.IsNullOrEmpty(DeptCode))
            {
                DeptCode = "D1001"; // HttpContext.Session.GetString(HttpContextSessionCategory.DeptCode.ToString()) as string;
            }
            List<UserInfo> userInfo = manageHumanResource.getUserInfos(_hostingEnvironment, string.Empty, string.Empty, 0, ClassYear, ClassBan, string.Empty, DeptCode, RankQueryOption.None);
         
            var orderedList = userInfo.OrderBy(user => user.ClassYear).ThenBy(user => user.ClassBan).ToList();
            ViewData["ClassYear"] = ClassYear;
            ViewData["ClassBan"] = ClassBan;
            return View(orderedList);
        }

        //

        // GET: JobCategory
        public async Task<IActionResult> IndexAdressOnly()
        {
            ViewData["ClassYear"] = 0;
            ViewData["ClassBan"] = 0;
            ViewData["BanMode"] = "IndexAdressOnly";
            return View(nameof(HomeController.Index));
        }
        // GET: JobCategory
        public async Task<IActionResult> IndexBan()
        {
            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();
                ManageHumanResource manageHumanResource = new ManageHumanResource();

                List<UserInfo> userInfo = manageHumanResource.getUserInfos(_hostingEnvironment, UserId, string.Empty, 0, 0, 0, string.Empty, string.Empty, RankQueryOption.None);

                ViewData["ClassYear"] = userInfo[0].ClassYear;
                ViewData["ClassBan"] = userInfo[0].ClassBan;
                ViewData["BanMode"] = "IndexBan";
            }

            return View("Index");
        }
        public async Task<IActionResult> IndexTeacher()
        {
            ManageHumanResource manageHumanResource = new ManageHumanResource();
            ViewData["ClassYear"] = 0;
            ViewData["ClassBan"] = 0;
            ViewData["BanMode"] = "IndexTeacher";

            return View("Index");
        }

        // GET: JobCategory
        public async Task<IActionResult> Index2(string HumanResourceCategory)
        {
            if (System.Enum.TryParse(HumanResourceCategory, out HumanResourceCategory pmrm))
            {
                ViewBag.HumanResourceCategory = HumanResourceCategory;
            }
            return View();
        }

        public async Task<IActionResult> addressBookMain()
        {
            ManageHumanResource manageHumanResource = new ManageHumanResource();
            var data = manageHumanResource.getClassInfos();
            return View(data);
        }

        public async Task<IActionResult> addressBookDetail(string DeptCode, string RankQueryOptionData, int ClassYear, int ClassBan)
        {         

            ManageHumanResource manageHumanResource = new ManageHumanResource();

            RankQueryOption groupNameEnum = (RankQueryOption)Enum.Parse(typeof(RankQueryOption), RankQueryOptionData);

            var data = manageHumanResource.getUserInfos(_hostingEnvironment, string.Empty, string.Empty, 0, ClassYear, ClassBan, string.Empty, DeptCode, groupNameEnum);
            ViewData["DisplayMode"] = "All";
            return View(data);
        }

        [AllowAnonymous]
        public async Task<IActionResult> addressBookDetailBirthDay(string DeptCode, string RankQueryOptionData, int ClassYear, int ClassBan)
        {

            ManageHumanResource manageHumanResource = new ManageHumanResource();

            RankQueryOption groupNameEnum = (RankQueryOption)Enum.Parse(typeof(RankQueryOption), RankQueryOptionData);
            ViewData["DisplayMode"] = "BirthDay";

            var data = manageHumanResource.getUserInfos(_hostingEnvironment, string.Empty, string.Empty, 0, ClassYear, ClassBan, string.Empty, DeptCode, groupNameEnum);

            var orderedList = data.OrderBy(user => user.bMonth).ThenBy(user => user.bDay).ToList();

            return View("addressBookDetail", orderedList);
        }

        public async Task<IActionResult> ModifyHRPhoto(string UserId)
        {
            ManageHumanResource manageHumanResource = new ManageHumanResource();

            List<UserInfo> userInfo = manageHumanResource.getUserInfos(_hostingEnvironment, UserId, string.Empty, 0, 0, 0, string.Empty, string.Empty, RankQueryOption.None);

            return View(userInfo[0]);
        }

        public async Task<IActionResult> ManageUserInfo(string UserId, int PersonId)
        {
            if (string.IsNullOrEmpty(UserId) && PersonId == 0)
            {

                if (await _authService.IsSignedIn(User))
                {
                    var result = await SetUserInfo();
                    UserId = this.UserId;
                }
            }

            ManageHumanResource manageHumanResource = new ManageHumanResource();

            List<UserInfo> userInfo = manageHumanResource.getUserInfos(_hostingEnvironment , UserId, string.Empty, PersonId, 0,0, string.Empty, string.Empty, RankQueryOption.None);
                        
            return View(userInfo[0]);
        }

        public async Task<IActionResult> ChangeUserInfo(string UserId, int PersonId)
        {
            ManageHumanResource manageHumanResource = new ManageHumanResource();

            List<UserInfo> userInfo = manageHumanResource.getUserInfos(_hostingEnvironment, UserId, string.Empty, PersonId, 0, 0, string.Empty, string.Empty, RankQueryOption.None);

            return View(userInfo[0]);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUserInfoProcess(UserInfo model)
        {
            if (ModelState.IsValid)
            {
                // 유효성 검사를 통과했을 경우, 사용자 정보를 저장합니다.
                // 데이터베이스에 저장하는 로직을 추가하세요.
                ControlHumanResource controlHumanResource = new ControlHumanResource();
                // Save the image as a binary to the ProfilePicture field using dynamic SQL
                await controlHumanResource.SaveProfileEtcAsync(model.UserId, model.LastName, model.FirstName, model.PhoneNumber);
                await controlHumanResource.SavePersonMasterAsync(model.UserId, model.PhoneNumber, model.bMonth, model.bDay);
                //await controlHumanResource.SaveClassInfoAsync(model.UserId, model.ClassYear, model.ClassBan);

                // 저장 후 리다이렉트
                return RedirectToAction("ManageUserInfo", new { UserId = model.UserId, PersonId = model.PersonId });
            }
            return View("ChangeUserInfo", model);
        }

        public async Task<IActionResult> ChangeUserDeptInfoIntro(string UserId)
        {
            ManageHumanResource manageHumanResource = new ManageHumanResource();
            List<UserInfo> userInfo = manageHumanResource.getUserInfos(_hostingEnvironment, UserId, string.Empty, 0, 0, 0, string.Empty, string.Empty, RankQueryOption.None);
            ViewData["userInfo"] = userInfo[0];
            return View();
        }


        public async Task<IActionResult> ChangeUserDeptInfoMain(string UserId)
        {
            ManageHumanResource manageHumanResource = new ManageHumanResource();
            List<UserInfo> userInfo = manageHumanResource.getUserInfos(_hostingEnvironment, UserId, string.Empty, 0, 0, 0, string.Empty, string.Empty, RankQueryOption.None);
            ViewData["userInfo"] = userInfo[0];

            ManageDeptInfo manageDeptInfo = new ManageDeptInfo();
            List<CodeData> codeDatas = manageDeptInfo.getCodeData("D001");
            return View(codeDatas);
        }

        public async Task<IActionResult> ChangeUseRoleMain(string UserId, string DeptCode,string DeptNM)
        {
            ManageHumanResource manageHumanResource = new ManageHumanResource();
            List<UserInfo> userInfo = manageHumanResource.getUserInfos(_hostingEnvironment, UserId, string.Empty, 0, 0, 0,string.Empty, string.Empty, RankQueryOption.None);
            ViewData["userInfo"] = userInfo[0];
            ViewData["DeptCode"] = DeptCode;
            ViewData["DeptNM"] = DeptNM;

            ManageDeptInfo manageDeptInfo = new ManageDeptInfo();

            List<CodeData> codeDatas = manageDeptInfo.getCodeData("T001");
            return View(codeDatas);
        }         

        public async Task<IActionResult> ChangeUseRoleMainProcess(DeptRoleUserAssoc model)
        {
            ManageHumanResource manageHumanResource = new ManageHumanResource();
            //H0101
            if (model.RoleCode == "T1001" || model.RoleCode == "T1002")
            { 
                List<UserInfo> userInfo = manageHumanResource.getUserInfos(_hostingEnvironment, model.UserId, string.Empty, 0, 0, 0, string.Empty, string.Empty, RankQueryOption.None);
                                userInfo = manageHumanResource.getUserInfos(_hostingEnvironment, model.UserId, string.Empty, 0, 0, 0, string.Empty, string.Empty, RankQueryOption.None);
                
                ViewData["DeptCode"] = model.DeptCode;
                ViewData["DeptNM"] = model.DeptNM;
                ViewData["RoleCode"] = model.RoleCode;
                ViewData["RoleNM"] = model.RoleNM;
                return View("ChangeUserClassInfo", userInfo[0]);
            }
            else
            {
                ControlHumanResource controlHumanResource = new ControlHumanResource();

                await controlHumanResource.DeleteClassInfoAsync(model.UserId);
                await controlHumanResource.DeleteDeptRoleUserAssocAsync(model.UserId);
                await controlHumanResource.SaveDeptRoleUserAssocAsync(model.UserId, model.DeptCode, model.RoleCode);
              
                List<UserInfo> userInfo = manageHumanResource.getUserInfos(_hostingEnvironment, model.UserId, string.Empty, 0, 0, 0, string.Empty, string.Empty, RankQueryOption.None);
                userInfo = manageHumanResource.getUserInfos(_hostingEnvironment, model.UserId, string.Empty, 0, 0, 0, string.Empty, string.Empty, RankQueryOption.None);

                return View("ChangeUserDeptInfoProcessResult", userInfo[0]);
            }
        }
        //

        //public async Task<IActionResult> ChangeUserDeptInfoNextProcess(string UserDept, string UserId)
        //{
        //    ManageHumanResource manageHumanResource = new ManageHumanResource();

        //    List<UserInfos> userInfo = manageHumanResource.getUserInfos(_hostingEnvironment, UserId, string.Empty, 0, 0, 0);

        //    return View("ChangeUserDeptInfoProcessResult", userInfo[0]);
        //}
        public async Task<IActionResult> ChangeUserClassInfo(string UserId)
        {
            ManageHumanResource manageHumanResource = new ManageHumanResource();
            List<UserInfo> userInfo = manageHumanResource.getUserInfos(_hostingEnvironment, UserId, string.Empty, 0, 0, 0, string.Empty, string.Empty, RankQueryOption.None);
            return View(userInfo[0]);
        }
        [HttpPost]
        public async Task<IActionResult> ChangeUserClassInfoProcess(DeptRoleUserAssoc model)
        {//string UserId,int ClassYear

            ControlHumanResource controlHumanResource = new ControlHumanResource();

            await controlHumanResource.DeleteClassInfoAsync(model.UserId);
            await controlHumanResource.DeleteDeptRoleUserAssocAsync(model.UserId);

            await controlHumanResource.SaveClassInfoAsync(model.UserId, model.ClassYear, model.ClassBan);
            await controlHumanResource.SaveDeptRoleUserAssocAsync(model.UserId, model.DeptCode, model.RoleCode);

            ManageHumanResource manageHumanResource = new ManageHumanResource();
            List<UserInfo> userInfo = manageHumanResource.getUserInfos(_hostingEnvironment, model.UserId, string.Empty, 0, 0, 0, string.Empty, string.Empty, RankQueryOption.None);
            userInfo = manageHumanResource.getUserInfos(_hostingEnvironment, model.UserId, string.Empty, 0, 0, 0, string.Empty, string.Empty, RankQueryOption.None);

            return View("ChangeUserDeptInfoProcessResult", userInfo[0]);
        }



        public async Task<JsonResult> GetDetails(int ClassYear, int ClassBan , string BanMode)
        {

            ManageHumanResource manageHumanResource = new ManageHumanResource();

     

            return Json(manageHumanResource.getUserInfos(_hostingEnvironment, string.Empty, string.Empty,0, ClassYear, ClassBan, BanMode, string.Empty, RankQueryOption.None));
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetProfilePicture(string userId)
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                // 1) DB 바이너리 우선
                ControlHumanResource controlHumanResource = new ControlHumanResource();
                byte[] imageBytes = await controlHumanResource.RetrieveProfilePictureAsync(userId);
                if (imageBytes != null && imageBytes.Length > 0)
                    return File(imageBytes, "image/jpeg");

                // 2) 파일 시스템 fallback: {WebRootPath}/images/profile/{userId}.{ext}
                var webRoot = _hostingEnvironment.WebRootPath ?? Directory.GetCurrentDirectory();
                string[] exts = { ".png", ".jpg", ".jpeg", ".webp" };
                foreach (var ext in exts)
                {
                    var filePath = Path.Combine(webRoot, "images", "profile", userId + ext);
                    if (System.IO.File.Exists(filePath))
                    {
                        var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
                        var mime = ext == ".webp" ? "image/webp" : ext == ".png" ? "image/png" : "image/jpeg";
                        return File(bytes, mime);
                    }
                }
            }

            // 3) 최종 기본 이미지
            var defaultPath = Path.Combine(_hostingEnvironment.WebRootPath ?? Directory.GetCurrentDirectory(), "images", "icon", "people", "people_01.webp");
            var defaultBytes = await System.IO.File.ReadAllBytesAsync(defaultPath);
            return File(defaultBytes, "image/webp");
        }


        [HttpPost]
        public async Task<IActionResult> UploadProfilePicture([FromBody] ImageUploadModel model)
        {
            if (model?.ProfilePicture == null)
            {
                return Json(new { success = false, message = "No image data provided." });
            }

            // Convert base64 string to byte array
            byte[] imageBytes = Convert.FromBase64String(model.ProfilePicture);          

            try
            {
                var profilePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", "profile");
                string filePath = Path.Combine(profilePath, $"{model.UserId}.png");


                ControlHumanResource controlHumanResource = new ControlHumanResource();
                // Save the image as a binary to the ProfilePicture field using dynamic SQL
                await controlHumanResource.SaveProfilePictureAsync(model.UserId, imageBytes, filePath);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }      
    } 
}


