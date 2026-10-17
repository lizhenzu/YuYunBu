using BulletinBoard.Areas.Hanmadi.Data;
using BulletinBoard.Areas.Hanmadi.Data.Entities;
using BulletinBoard.Areas.Hanmadi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Areas.Hanmadi.Pages.Admin.Sessions;

[Authorize]
public class CreateModel : PageModel
{
    private readonly HanmadiDbContext _db;
    public CreateModel(HanmadiDbContext db) => _db = db;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "제목을 입력하세요.")]
        [StringLength(100)]
        public string Title { get; set; } = "";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        // 중복 없는 6자리 코드 생성
        string code;
        do { code = SessionCodeGenerator.Generate(); }
        while (_db.Sessions.Any(s => s.Code == code));

        var session = new HmSession
        {
            Title        = Input.Title.Trim(),
            Code         = code,
            OwnerUserId  = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            Status       = 0,
            CreatedAt    = DateTime.UtcNow,
        };

        _db.Sessions.Add(session);
        await _db.SaveChangesAsync();

        return Redirect("/hanmadi/admin");
    }
}
