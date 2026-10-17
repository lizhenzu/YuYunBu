using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Enums;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    public class BoardController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment; 

        public BoardController(IAuthService authService,
            UserManager<ApplicationUser> userManager,
                 IWebHostEnvironment hostingEnvironment) : base(authService, userManager, hostingEnvironment)
        {
            _authService = authService;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;

        }
        public IActionResult Index()
        {
            return View();
        }    
        
        public IActionResult WhatAnonymous()
        {
            return View();
        }
        
        public IActionResult IndexByBoard(string boardName)
        {
            ViewBag.BoardName = boardName;
            BoardRepository _boardRepository = new BoardRepository();
            var boards = _boardRepository.GetBoardList(boardName);
            return View("Index", boards);
        }
        public IActionResult DeleteByID(string boardName, int id)
        {
            ViewBag.BoardName = boardName;
            BoardRepository _boardRepository = new BoardRepository();
            _boardRepository.DeleteBoard(boardName,id); 
            return RedirectToAction(nameof(BoardController.IndexByBoard), "Board", new { boardName = boardName });
        }

        public async Task<IActionResult> Detail(string boardName, int id)
        {
            ViewBag.BoardName = boardName;
            BoardRepository _boardRepository = new BoardRepository();
            var board = _boardRepository.GetBoardById(boardName, id);
            var comments = _boardRepository.GetCommentsByBoardId(boardName, id);

            var userFullName = HttpContext.Session.GetString(HttpContextSessionCategory.UserFullName.ToString()) as string;
            var UserName = HttpContext.Session.GetString(HttpContextSessionCategory.UserName.ToString()) as string;

            if (!SharedComm.isAdmin(UserName))
            {
                _boardRepository.HitBoard(boardName, id);
            } 

            var model = new BoardViewModel
            {
                Board = board,
                Comments = comments
            };

            return View(model);
        }
        // 신규 게시글 작성 폼
        public IActionResult Create(string boardName)
        {
            ViewBag.BoardName = boardName;
            return View();
        }     // 신규 게시글 작성 폼
        public IActionResult Create2(string boardName)
        {
            ViewBag.BoardName = boardName;
            return View();
        }


        public IActionResult Results(string boardName)
        {
            ViewBag.BoardName = "개선방";
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(string boardName, [FromForm] Board board)

        {
            // 익명 의견함: 보내는 사람은 "익명" 또는 로그인한 본인 이름만 허용 (폼 값을 그대로 믿지 않는다)
            if (string.Equals(boardName, BoardCategory.Anonymous.ToString(), System.StringComparison.OrdinalIgnoreCase))
            {
                board.UserName = await ResolveAnonymousSenderAsync(board.UserName);
            }

            if (string.IsNullOrEmpty(board.UserName))
            {
                var userFullName = HttpContext.Session.GetString(HttpContextSessionCategory.UserFullName.ToString()) as string;
                board.UserName = userFullName;
            }

            ViewBag.BoardName = boardName;
            if (ModelState.IsValid)
            {
                BoardRepository _boardRepository = new BoardRepository();
                _boardRepository.AddBoard(boardName,board);

                ManageMessageCenter manageMessageCenter = new ManageMessageCenter();
                PopMessage _s1 = new PopMessage() { FromWhom = board.UserName, MessageText = board.Content, ToWhom = "SuperAdmin" };
                manageMessageCenter.CreatePopMessage(_s1);
                PopMessage _s2 = new PopMessage() { FromWhom = board.UserName, MessageText = board.Content, ToWhom = "Basic" };
                manageMessageCenter.CreatePopMessage(_s2);


                if (boardName.ToLower() == BoardCategory.Anonymous.ToString().ToLower())
                {
                    return RedirectToAction("Results", new { boardName = boardName });
                }
                else
                {
                    return Json("OK");
                }
            }
            return View(board);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(string boardName,int boardId, string content, string userName, int? parentCommentId)
        {
            if (string.IsNullOrEmpty(userName))
            {
                var userFullName = HttpContext.Session.GetString(HttpContextSessionCategory.UserFullName.ToString()) as string;
                userName = userFullName;
            }

            ViewBag.BoardName = boardName;
            BoardRepository _boardRepository = new BoardRepository();
            _boardRepository.AddComment(boardName, boardId, content, userName, parentCommentId);
            return RedirectToAction("Detail", new { boardName = boardName, id = boardId });
        }
        private const string AnonymousSender = "익명";

        /// <summary>
        /// 익명 의견함 작성자 결정
        /// - 로그인 안 함 → 무조건 "익명"
        /// - 로그인 함   → "익명" 또는 본인 이름만. 그 밖의 값이 오면 본인 이름으로 바꾼다.
        /// </summary>
        private async Task<string> ResolveAnonymousSenderAsync(string requested)
        {
            if (!await _authService.IsSignedIn(User)) return AnonymousSender;

            requested = (requested ?? "").Trim();
            if (requested == AnonymousSender) return AnonymousSender;

            // Create2 화면이 보여주는 이름과 같은 규칙 (성+이름, 없으면 UserName)
            var user = await _authService.GetSignedUser(User);
            var myName = $"{user?.LastName}{user?.FirstName}".Trim();
            if (string.IsNullOrEmpty(myName)) myName = user?.UserName ?? "";
            var sessionName = HttpContext.Session.GetString(HttpContextSessionCategory.UserFullName.ToString()) ?? "";

            if (requested.Length > 0 && (requested == myName || requested == sessionName)) return requested;
            if (!string.IsNullOrEmpty(myName)) return myName;
            return string.IsNullOrEmpty(sessionName) ? AnonymousSender : sessionName;
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        } 
    }
}


