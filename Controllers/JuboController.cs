using BulletinBoard.Helpers;
using BulletinBoard.Models.Jubo;
using BulletinBoard.Services.Jubo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    public class JuboController : Controller
    {
        private readonly IJuboService _juboService;

        public JuboController(IJuboService juboService)
        {
            _juboService = juboService;
        }

        // ─────────────────────────────────────────────────────────────────────
        // LIST
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            await _juboService.EnsureTablesAsync();
            var vm = await _juboService.GetJuboListAsync(page);
            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet, Authorize]
        public IActionResult Create()
        {
            return View(new JuboCreateViewModel());
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JuboCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                await _juboService.EnsureTablesAsync();
                var id = await _juboService.CreateJuboAsync(model, User.Identity?.Name ?? "Unknown");
                TempData["Success"] = "주보가 성공적으로 만들어졌어요! ✨";
                return RedirectToAction(nameof(Edit), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"저장 중 오류가 발생했습니다: {ex.Message}");
                return View(model);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // EDIT
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet, Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            await _juboService.EnsureTablesAsync();
            var vm = await _juboService.GetJuboForEditAsync(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveMeta(int id, string title, string? season, string templateType)
        {
            if (string.IsNullOrWhiteSpace(title))
                return BadRequest("제목을 입력해주세요.");

            var success = await _juboService.UpdateJuboMetaAsync(
                id, title, season, templateType, User.Identity?.Name ?? "Unknown");

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success });

            TempData["Success"] = "기본 정보가 저장되었습니다.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        // POST /Jubo/SaveBlock  (AJAX, JSON body)
        [HttpPost, Authorize]
        public async Task<IActionResult> SaveBlock([FromBody] JuboBlockSaveRequest? request)
        {
            if (request == null) return BadRequest();

            var block = new BlockEditViewModel
            {
                Id           = request.Id,
                JuboId       = request.JuboId,
                BlockType    = request.BlockType,
                BlockTitle   = request.BlockTitle,
                BlockContent = request.BlockContent,
                SortOrder    = request.SortOrder,
                IsVisible    = request.IsVisible
            };

            var success = await _juboService.SaveSingleBlockAsync(block, User.Identity?.Name ?? "Unknown");

            return Json(new
            {
                success,
                modifiedBy = User.Identity?.Name,
                modifiedAt = DateHelper.GetKoreaNow().ToString("yyyy-MM-dd HH:mm")
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // PREVIEW & PRINT
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Preview(int id)
        {
            await _juboService.EnsureTablesAsync();
            var vm = await _juboService.GetJuboForPreviewAsync(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Print(int id)
        {
            await _juboService.EnsureTablesAsync();
            var vm = await _juboService.GetJuboForPreviewAsync(id);
            if (vm == null) return NotFound();

            if (User.Identity?.IsAuthenticated == true)
                await _juboService.CreateSnapshotAsync(id, User.Identity.Name ?? "Unknown");

            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────────────
        // DELETE / PUBLISH
        // ─────────────────────────────────────────────────────────────────────

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _juboService.DeleteJuboAsync(id);
            TempData["Success"] = "주보가 삭제되었습니다.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> TogglePublish(int id)
        {
            await _juboService.TogglePublishAsync(id, User.Identity?.Name ?? "Unknown");
            return RedirectToAction(nameof(Edit), new { id });
        }

        // ─────────────────────────────────────────────────────────────────────
        // ATTENDANCE
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet, Authorize]
        public async Task<IActionResult> Attendance(int id)
        {
            await _juboService.EnsureTablesAsync();
            var jubo = await _juboService.GetJuboForEditAsync(id);
            if (jubo == null) return NotFound();

            var summary   = await _juboService.GetAttendanceSummaryAsync(jubo.JuboDate);
            var newComers = await _juboService.GetNewComersAsync(jubo.JuboDate);

            var vm = new AttendancePageViewModel
            {
                JuboId    = id,
                JuboDate  = jubo.JuboDate,
                JuboTitle = jubo.Title,
                Summary   = summary,
                NewComers = newComers
            };

            return View(vm);
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> SaveAttendanceClass(
            [FromBody] JuboAttendanceSaveRequest? req)
        {
            if (req == null) return BadRequest();

            var item = new ClassAttendanceSaveItem
            {
                Id            = req.Id,
                ClassName     = req.ClassName,
                TotalCount    = req.TotalCount,
                PresentCount  = req.PresentCount,
                NewComerCount = req.NewComerCount,
                Note          = req.Note
            };

            var success = await _juboService.SaveAttendanceClassAsync(
                item, req.JuboDate, User.Identity?.Name ?? "Unknown");

            return Json(new { success });
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> DeleteAttendanceClass(int id)
        {
            var success = await _juboService.DeleteAttendanceClassAsync(id);
            return Json(new { success });
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> SaveNewComer([FromBody] SaveNewComerViewModel? model)
        {
            if (model == null) return BadRequest();
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "입력 정보를 확인해주세요." });

            var newId = await _juboService.SaveNewComerAsync(model, User.Identity?.Name ?? "Unknown");
            return Json(new { success = newId > 0, id = newId });
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> DeleteNewComer(int id)
        {
            var success = await _juboService.DeleteNewComerAsync(id);
            return Json(new { success });
        }

        // ─────────────────────────────────────────────────────────────────────
        // SNAPSHOT
        // ─────────────────────────────────────────────────────────────────────

        [HttpPost, Authorize]
        public async Task<IActionResult> CreateSnapshot(int id)
        {
            var snapshotId = await _juboService.CreateSnapshotAsync(
                id, User.Identity?.Name ?? "Unknown");
            return Json(new { success = snapshotId > 0, snapshotId });
        }

        // ─────────────────────────────────────────────────────────────────────
        // INIT (admin helper)
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet, Authorize]
        public async Task<IActionResult> InitTables()
        {
            await _juboService.EnsureTablesAsync();
            TempData["Success"] = "주보 테이블이 성공적으로 초기화되었습니다! 🎉";
            return RedirectToAction(nameof(Index));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Request DTOs (AJAX JSON body)
    // ─────────────────────────────────────────────────────────────────────────

    public class JuboBlockSaveRequest
    {
        public int     Id           { get; set; }
        public int     JuboId       { get; set; }
        public string  BlockType    { get; set; } = string.Empty;
        public string? BlockTitle   { get; set; }
        public string? BlockContent { get; set; }
        public int     SortOrder    { get; set; }
        public bool    IsVisible    { get; set; }
    }

    public class JuboAttendanceSaveRequest
    {
        public int      Id            { get; set; }
        public DateTime JuboDate      { get; set; }
        public string   ClassName     { get; set; } = string.Empty;
        public int      TotalCount    { get; set; }
        public int      PresentCount  { get; set; }
        public int      NewComerCount { get; set; }
        public string?  Note          { get; set; }
    }
}
