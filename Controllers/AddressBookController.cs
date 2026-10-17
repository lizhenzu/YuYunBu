using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BulletinBoard.Services.Abstract;

namespace BulletinBoard.Controllers
{
    [Authorize]
    public class AddressBookController : Controller
    {
        private readonly IAddressBookService _service;

        public AddressBookController(IAddressBookService service)
        {
            _service = service;
        }

        public IActionResult Index() => View("~/Views/AddressBook/Index.cshtml");
        public IActionResult Branch() => View();

        [HttpGet]
        public IActionResult Search(string q, int take = 30)
        {
            q = (q ?? "").Trim();

            if (q.Length < 2)
                return Json(new { ok = true, items = new object[0] });

            var items = _service.Search(q, take);
            return Json(new { ok = true, items });
        }
    }
}
