using Microsoft.AspNetCore.Mvc;
using SimpleContact.Models;
using System.Diagnostics;
using SimpleContact.Services;
using SimpleContact.Services.Implementation;
using Microsoft.AspNetCore.Http;
using System.Data.SqlTypes;


namespace SimpleContact.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IEmailService _emailSvc;
        private static readonly Random _rnd = new Random();

        public HomeController(ILogger<HomeController> logger, IEmailService emailSvc)
        {
            _logger = logger;
            _emailSvc = emailSvc;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Index(EmailData model)
        {
            if (ModelState.IsValid)
            {
                if (model.Attachments is { })
                {
                    string tempFolderName = GenerateRandomFolderName();
                    await _emailSvc.SendContactEmailAsync(model, tempFolderName);
                }
                else
                {
                    await _emailSvc.SendContactEmailAsync(model, null);
                }
                //return View();
                //return Redirect("~/Home/Index"); //refresh screen controls.
                return Redirect("~/Home/Success");
            }
            return View(model);
        }


        public IActionResult Success()
        {
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        private static string GenerateRandomFolderName()//a unique upload folder is required to prevent filename collisions between users.
        {
            const string StringChars = "0123456789ABCDEF";

            var charList = StringChars.ToArray();
            string hexString = "A"; //always start with alpha char

            for (int i = 0; i < 10; i++)
            {
                int randIndex = _rnd.Next(0, charList.Length);
                hexString += charList[randIndex];
            }
            return hexString;
        }

    }
}