using BulletinBoard.Helpers;
using BulletinBoard.Services;
using BulletinBoard.Services.Abstract;
using BulletinBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulletinBoard.Controllers
{
    // ✅ 교적(주소·전화·수정)은 교사만 — 교사 대시보드와 같은 권한
    [Authorize(Roles = "Admin,SuperAdmin,Moderator")]
    public class ChurchRecordController : Controller
    {
        private readonly IChurchRecordService _IChurchRecordService;

        public ChurchRecordController(IChurchRecordService service)
        {
            _IChurchRecordService = service;
        }

        // GET: /ChurchRecord  (?searchName= 이면 전체 반에서 아이 찾기)
        public IActionResult Index(string? searchName = null)
        {
            var vm = _IChurchRecordService.GetHome(searchName);
            return View("~/Views/ChurchRecord/Index.cshtml", vm);
        }

        // GET: /ChurchRecord/Class?year=3&ban=1
        public IActionResult Class(int year, int ban, string? searchName = null)
        {
            var vm = _IChurchRecordService.GetClassRoster(year, ban, searchName);
            ViewBag.AllClasses = _IChurchRecordService.GetHome().Classes; // 이전·다음 반, 반 바로가기
            return View("~/Views/ChurchRecord/Class.cshtml", vm);
        }


        [HttpGet]
        public IActionResult GetPerson(int personId)
        {
            var data = _IChurchRecordService.GetPersonForEdit(personId);

            if (data == null)
                return Json(new { ok = false, message = "해당 PersonId를 찾지 못했습니다." });

            return Json(new { ok = true, data });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdatePerson([FromBody] PersonEditVM vm)
        {
            if (vm == null)
                return Json(new { ok = false, message = "요청 데이터가 비었습니다." });

            if (!ModelState.IsValid)
                return Json(new { ok = false, message = "입력값이 올바르지 않습니다." });

            var ok = _IChurchRecordService.UpdatePerson(vm, out var msg);
            return Json(new { ok, message = msg });
        }
    }
}
