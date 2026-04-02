using Microsoft.AspNetCore.Mvc;

namespace DoAnTotNghiep.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Shop()
        {
            // Thêm dòng này để ép nó tìm đúng file
            return View();
        }
    }
}
