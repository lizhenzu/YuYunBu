using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
    [LogActionFilter]
public class LikeVotingController : Controller
{
    private readonly IDrawingVoteRepository _repo;

    public LikeVotingController(IDrawingVoteRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public IActionResult Index(string sortBy)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        // 목록 + 현재 IP가 각 그림에 좋아요했는지 여부 포함
        List<DrawingVoteViewModel> vmList = _repo.GetAllWithUserState(sortBy, ip);

        return View(vmList);
    }

    [HttpPost]
    public IActionResult Like([FromForm] int id)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = _repo.ToggleLike(id, ip);

        // 항상 JSON으로 응답 (프론트에서 catch 안 타게)
        return Json(new
        {
            id = result.Id,
            likes = result.LikeCount,
            isLiked = result.IsLikedNow
        });
    }
}
