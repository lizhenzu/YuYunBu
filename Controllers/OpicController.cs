using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Enums;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    public class OpicController : Controller
    { 
        public OpicController()
        { 
        }

        // GET: JobCategory
        public async Task<IActionResult> Index()
        {
            return RedirectToAction(nameof(OpicController.DetailList), "Opic");
        }


        public async Task<IActionResult> DetailWords(int Id, string ExposeDate)
        {
          
            var urlInfo = UrlInfoHelper.GetUrlInfo(HttpContext);
            ViewBag.Domain = urlInfo.Domain;
            ViewBag.ControllerName = urlInfo.ControllerName;
            ViewBag.ActionName = urlInfo.ActionName;
            ViewBag.FullURL = $"{ViewBag.Domain}/{ViewBag.ControllerName}/{ViewBag.ActionName}"; 
            if (Id > 0)
            {
            }
            else
            {
                if (string.IsNullOrEmpty(ExposeDate))
                {
                   ExposeDate = SharedComm.GetTodayString();
                }
            }
            ManageOpic manageOpic = new ManageOpic();
            var data = manageOpic.getOpic(Id, ExposeDate, string.Empty, string.Empty);

            if (data.Count == 0)
                return RedirectToAction(nameof(OpicController.DetailList), "Opic");

            //DateTime currentDate = SharedComm.GetToday();
            //int index = SharedComm.CalculateIndex(data.Count, currentDate);
                    
            return View(data[0]);

        }

        public async Task<IActionResult> DeleteView(int Id)
        {
            ManageOpic manageOpic = new ManageOpic();
            manageOpic.DeleteOpic(Id);
            return RedirectToAction(nameof(OpicController.DetailList), "Opic");
        }

        public async Task<IActionResult> MarkView(int Id)
        {
            ManageOpic manageOpic = new ManageOpic();
            manageOpic.WorkVisitedOpic(Id);
            return RedirectToAction(nameof(OpicController.DetailList), "Opic");
        }

        public async Task<IActionResult> ModifyView(int Id)
        {
            ManageOpic manageOpic = new ManageOpic();
            var data = manageOpic.getOpic(Id, string.Empty, string.Empty, string.Empty);
            return View(data[0]);
        }

        public async Task<IActionResult> EnrollView()
        {
            OpicData OpicData = new OpicData(); 
            return View(OpicData);
        }


        public async Task<IActionResult> ModifyProcess(OpicData param)
        {
            ManageOpic manageOpic = new ManageOpic();
            manageOpic.UpdateOpic(param);
            return RedirectToAction(nameof(OpicController.DetailWords), "Opic" , new { Id = param.Id });
        }

        public async Task<IActionResult> EnrollProcess(OpicData param)
        {
            ManageOpic manageOpic = new ManageOpic();
            manageOpic.CreateOpic(param);

            return RedirectToAction(nameof(OpicController.DetailList), "Opic");
        }


        // GET: JobCategory
        public async Task<IActionResult> SearchQuery(string query)
        {
            return RedirectToAction(nameof(OpicController.DetailList), "Opic", new {query = query });
        }

        public async Task<IActionResult> DetailList(string sortBy, string query)
        {
            ManageOpic manageOpic = new ManageOpic();
         
            return View(manageOpic.getOpic(0, string.Empty, sortBy, query));
        } 

    }
}


