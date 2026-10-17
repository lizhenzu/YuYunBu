using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    public class IntroController : Controller
    {
        public IntroController()
        {
        }
        // GET: JobCategory
        public async Task<IActionResult> Index()
        { 
 
            return View();
        }
        public async Task<IActionResult> Index1()
        {
            return View();
        }
        public async Task<IActionResult> Index2()
        {
            return View();
        }
        public async Task<IActionResult> Index3()
        {
            string userName = string.Empty;

            return View();
        }
        public async Task<IActionResult> Index4()
        {
            string userName = string.Empty;

            return View();
        }
        public async Task<IActionResult> Index5()
        {
            string userName = string.Empty;

            return View();
        }
        public async Task<IActionResult> Index6()
        {
            string userName = string.Empty;

            return View();
        }
        public async Task<IActionResult> Index7()
        {
            string userName = string.Empty;

            return View();
        }

        public async Task<IActionResult> WhatChallenge2014()
        { 
 
            return View();
        }


        public async Task<IActionResult> HowtogiveReward()
        { 
 

            return View();
        }
        public async Task<IActionResult> HowTo1()
        {
            string userName = string.Empty;

            return View();
        }
        public async Task<IActionResult> HowTo2()
        {
            string userName = string.Empty;

            return View();
        }
        public async Task<IActionResult> HowTo3()
        {
            string userName = string.Empty;

            return View();
        }
        public async Task<IActionResult> HowTo4()
        {
            string userName = string.Empty;

            return View();
        }
        public async Task<IActionResult> HowTo5()
        {
            string userName = string.Empty;

            return View();
        }
        public async Task<IActionResult> HowTo6()
        {
            string userName = string.Empty;

            return View();
        }
        public async Task<IActionResult> HowTo7()
        {
            string userName = string.Empty;

            return View();
        } 
        public async Task<IActionResult> HowToPublishExam()
        {
   
            return View();
        }
         

    }
}


