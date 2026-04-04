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
        public IActionResult Details(int id)
        {
            ViewBag.MaSanPham = id; // Gửi cái ID này qua View để JS biết đường mà lôi dữ liệu
            return View();
        }
        public IActionResult Cart()
        {
            return View();
        }

        public IActionResult Checkout()
        {
            return View();
        }
    }
}
