using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting.Internal;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    public class FileUploadController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public FileUploadController(IAuthService authService,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment hostingEnvironment) : base(authService, userManager, hostingEnvironment)
        {
            _authService = authService;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public IActionResult Upload(string category, int Id, int YYear, int MMonth,int WWeek, string WeeklyWordToReadID)
        { 
            FileUploadViewModel fileUploadViewModel = new FileUploadViewModel();

            fileUploadViewModel.Id = Id;
            fileUploadViewModel.YYear = YYear;
            fileUploadViewModel.MMonth = MMonth;
            fileUploadViewModel.WWeek = WWeek;
            fileUploadViewModel.SelectedCategory = (Category)Enum.Parse(typeof(Category), category);

            fileUploadViewModel.Categorys = Enum.GetValues(typeof(Category))
                             .Cast<Category>()
                             .Select(g => new SelectListItem
                             {
                                 Value = g.ToString(),
                                 Text = g.ToString()
                             }).ToList();

            return View(fileUploadViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(FileUploadViewModel model, string WeeklyWordToReadID)
        {

            if (ModelState.IsValid)
            {

                if (model.SelectedCategory == Category.People ||
                    model.SelectedCategory == Category.Related ||
                    model.SelectedCategory == Category.RelatedBible ||
                    model.SelectedCategory == Category.BibileStudy ||
                    model.SelectedCategory == Category.Adventure ||
                    model.SelectedCategory == Category.BackGround ||
                    model.SelectedCategory == Category.Student ||
                    model.SelectedCategory == Category.Teacher ||
                    model.SelectedCategory == Category.Encourage ||
                    model.SelectedCategory == Category.BibleExplanation ||
                    model.SelectedCategory == Category.StoryTellingBook ||                    
                    model.SelectedCategory == Category.Value ||
                    model.SelectedCategory == Category.Geomorphological)
                {
                    CreateUploadData(model);
                }
                else
                {
                    //                var uploadPath = Path.Combine(_hostingEnvironment.WebRootPath, model.FilePath);
                    var uploadPath = Path.Combine(_hostingEnvironment.WebRootPath, model.SelectedCategory.ToString(), $"{model.YYear}{model.MMonth.ToString().PadLeft(2, '0')}{model.WWeek.ToString().PadLeft(2, '0')}");

                    var uploadPathSub = $"{model.SelectedCategory.ToString()}/{model.YYear}{model.MMonth.ToString().PadLeft(2, '0')}{model.WWeek.ToString().PadLeft(2, '0')}";

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }
                    string filePathA = string.Empty;
                    string filePathB = string.Empty;
                    string filePathC = string.Empty;

                    string fileNameA = string.Empty;
                    string fileNameB = string.Empty;
                    string fileNameC = string.Empty;


                    if (!string.IsNullOrEmpty(model.UploadedFile1.FileName))
                    {
                        filePathA = Path.Combine(uploadPath, model.UploadedFile1.FileName);
                        fileNameA = model.UploadedFile1.FileName;
                        using (var stream = new FileStream(filePathA, FileMode.Create))
                        {
                            await model.UploadedFile1.CopyToAsync(stream);
                        }
                    }
                    if (model.UploadedFile2 != null && !string.IsNullOrEmpty(model.UploadedFile2.FileName))
                    {
                        filePathB = Path.Combine(uploadPath, model.UploadedFile2.FileName);
                        fileNameB = model.UploadedFile2.FileName;
                        using (var stream = new FileStream(filePathB, FileMode.Create))
                        {
                            await model.UploadedFile2.CopyToAsync(stream);
                        }
                    }
                    if (model.UploadedFile3 != null && !string.IsNullOrEmpty(model.UploadedFile3.FileName))
                    {
                        filePathC = Path.Combine(uploadPath, model.UploadedFile3.FileName);
                        fileNameC = model.UploadedFile3.FileName;

                        using (var stream = new FileStream(filePathC, FileMode.Create))
                        {
                            await model.UploadedFile3.CopyToAsync(stream);
                        }
                    }

                    ManageWeeklyWordToRead manageWeeklyWordToRead = new ManageWeeklyWordToRead();

                    manageWeeklyWordToRead.UpdateWeeklyWordToRead_RelativeImagePath(model.Id, uploadPathSub, fileNameA, fileNameB, fileNameC);
                }

                ViewBag.Message = "파일 업로드 성공";
                return View(model);
            }

            return View(model);
        }

        public async Task<IActionResult> CreateUploadData(FileUploadViewModel model)
        { 
            if (ModelState.IsValid)
            {

                //                var uploadPath = Path.Combine(_hostingEnvironment.WebRootPath, model.FilePath);
                var uploadPath = Path.Combine(_hostingEnvironment.WebRootPath, "HTML", model.SelectedCategory.ToString());

                var uploadPathSub = $"HTML/{model.SelectedCategory.ToString()}";

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                string FileKoreanNameBackup = model.FileKoreanName;


                await CreateUploadDataSub(model, model.UploadedFile1, uploadPath, uploadPathSub, FileKoreanNameBackup);

        

                await CreateUploadDataSub(model, model.UploadedFile2, uploadPath, uploadPathSub, FileKoreanNameBackup);

                await CreateUploadDataSub(model, model.UploadedFile3, uploadPath, uploadPathSub, FileKoreanNameBackup);

                await CreateUploadDataSub(model, model.UploadedFile4, uploadPath, uploadPathSub, FileKoreanNameBackup);

                await CreateUploadDataSub(model, model.UploadedFile5, uploadPath, uploadPathSub, FileKoreanNameBackup);

                await CreateUploadDataSub(model, model.UploadedFile6, uploadPath, uploadPathSub, FileKoreanNameBackup);

                await CreateUploadDataSub(model, model.UploadedFile7, uploadPath, uploadPathSub, FileKoreanNameBackup);



                ViewBag.Message = "파일 업로드 성공";
                return View(model);
            }
            return View(model);
        }

        public async Task<FileUploadViewModel> CreateUploadDataSub(FileUploadViewModel model, IFormFile UploadedFile, string uploadPath, string uploadPathSub, string FileKoreanNameBackup)
        {
            if (UploadedFile == null)
            {
                return model;
            }

            if (UploadedFile.FileName == null) {
                return model;
            }

            string extension = GetFileExtension(UploadedFile.FileName);

            if(extension != null)
            {
                extension = extension.ToLower();
                if (extension == ".jpg" || extension == ".png")
                {
                    model.FileKoreanName = FileKoreanNameBackup + "_" + UploadedFile.FileName;
                }
            }

            string fileName = UploadedFile.FileName;

            if (!string.IsNullOrEmpty(fileName))
            {
                fileName = $"{model.SelectedCategory.ToString()}_{fileName}";
                string filePathA = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePathA, FileMode.Create))
                {
                    await UploadedFile.CopyToAsync(stream);
                }
            }

            ManageUploadData manageUploadData = new ManageUploadData();
            manageUploadData.CreateUploadData(model.SelectedCategory.ToString(), model.FileKoreanName, $"/{uploadPathSub}/{fileName}");
            manageUploadData.InsertLastOne(model.WeeklyWordToReadID);
            return model;

        }


        public static string GetFileExtension(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("The file path cannot be null or empty.", nameof(filePath));
            }

            return Path.GetExtension(filePath);
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
