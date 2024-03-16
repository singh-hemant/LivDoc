using Microsoft.AspNetCore.Mvc;

namespace LivDocApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
