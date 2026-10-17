using BulletinBoard.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]/[action]")]
public class GlobalCookieController : ControllerBase
{
    private readonly IGlobalCookieService _globalCookieService;

    public GlobalCookieController(IGlobalCookieService globalCookieService)
    {
        _globalCookieService = globalCookieService;
    }

    // 배너를 보여줄지 여부 체크
    [HttpPost]
    public async Task<IActionResult> ShouldShow([FromBody] GlobalCookieCheckRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CookieKey))
            return BadRequest(new { message = "CookieKey is required." });

        var shouldShow = await _globalCookieService.ShouldShowAsync(request.CookieKey, HttpContext);
        return Ok(new { shouldShow });
    }

    // 숨김 기간 저장
    [HttpPost]
    public async Task<IActionResult> Hide([FromBody] GlobalCookieHideRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CookieKey))
            return BadRequest(new { message = "CookieKey is required." });

        await _globalCookieService.SetHideAsync(request.CookieKey, request.DurationCode, HttpContext);
        return Ok(new { result = "ok" });
    }
}
