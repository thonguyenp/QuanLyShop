using Microsoft.AspNetCore.Mvc;

namespace QuanLyShop.Controllers
{
    public class IntroController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
