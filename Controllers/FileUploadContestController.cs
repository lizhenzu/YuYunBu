using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    public class FileUploadContestController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public FileUploadContestController(IAuthService authService,
            UserManager<ApplicationUser> userManager,
                 IWebHostEnvironment hostingEnvironment) : base(authService, userManager, hostingEnvironment)
        {
            _authService = authService;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(FileUploadViewModel model)
        {
            if (ModelState.IsValid)
            {
                CreateUploadData(model);
 
                ViewBag.Message = "파일 업로드 성공";
                return View(model);
            }

            return View(model);
        }

        public async Task<IActionResult> CreateUploadData(FileUploadViewModel model)
        { 
            if (ModelState.IsValid)
            {
                 var uploadPath = Path.Combine(_hostingEnvironment.WebRootPath, "PhotoContest");
                 
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                string filePathA = string.Empty;
                string fileNameA = string.Empty;

                if (!string.IsNullOrEmpty(model.UploadedFile1.FileName))
                {
                    fileNameA = model.UploadedFile1.FileName;
                    fileNameA = $"{Guid.NewGuid().ToString().Replace("-","_")}_{fileNameA}";
                    filePathA = Path.Combine(uploadPath, fileNameA);
                    using (var stream = new FileStream(filePathA, FileMode.Create))
                    {
                        await model.UploadedFile1.CopyToAsync(stream);
                    }
                }

                ManageUploadData manageUploadData = new ManageUploadData();
                manageUploadData.CreateUploadData(model.SelectedCategory.ToString(), model.FileKoreanName, $"/{fileNameA}");

                ViewBag.Message = "파일 업로드 성공";
                return View(model);
            }
            return View(model);
        }

    }
}



//public async Task<IActionResult> UploadProcessGe1111111111111111111111111111111omorphological(FileUploadViewModel model)
//{
//    ViewBag.Categories = SharedComm.MakeCategory();

//    if (ModelState.IsValid)
//    {

//        //                var uploadPath = Path.Combine(_hostingEnvironment.WebRootPath, model.FilePath);
//        var uploadPath = Path.Combine(_hostingEnvironment.WebRootPath, "HTML", model.Categorys.ToString());

//        var uploadPathSub = $"HTML/{model.Categorys.ToString()}";

//        if (!Directory.Exists(uploadPath))
//        {
//            Directory.CreateDirectory(uploadPath);
//        }
//        string filePathA = string.Empty;

//        string fileNameA = string.Empty;


//        if (!string.IsNullOrEmpty(model.UploadedFileA.FileName))
//        {
//            filePathA = Path.Combine(uploadPath, model.UploadedFileA.FileName);
//            fileNameA = model.UploadedFileA.FileName;
//            using (var stream = new FileStream(filePathA, FileMode.Create))
//            {
//                await model.UploadedFileA.CopyToAsync(stream);
//            }
//        }

//        ManageUploadData manageUploadData = new ManageUploadData();

//        manageeBiblePeople.CreateBiblePeople(model.FileKoreanName, $"/{uploadPathSub}/{fileNameA}");

//        ViewBag.Message = "파일 업로드 성공";
//        return View(model);
//    }
//    return View(model);
//}
