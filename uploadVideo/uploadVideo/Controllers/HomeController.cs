using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using uploadVideo.Models;

namespace uploadVideo.Controllers
{
    public class HomeController : Controller
    {
        private readonly VideoOperation videoOperation;

        public HomeController(VideoOperation context)
        {
            videoOperation = context;
        }
        public async Task<IActionResult> Index()
        {
            var Users = await videoOperation.GetRegisteredUsers();
            var model = new IndexViewModel { talentShower = Users };
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(TalentShower ts)
        {
            ts.hasFile = false;
            if (ModelState.IsValid)
            {
                await videoOperation.RegisterInMongoDB(ts);
                return RedirectToAction("Index");
            }
            return View(ts);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
