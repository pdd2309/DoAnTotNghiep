using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnTotNghiep.Models;

namespace DoAnTotNghiep.Controllers
{
    public class HomeController : Controller
    {
        private readonly CuaHangCongNgheDBContext _db;

        public HomeController(CuaHangCongNgheDBContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var activeBanners = _db.Banners
                .Where(x => x.IsActive)
                .OrderBy(x => x.Position)
                .ThenBy(x => x.DisplayOrder)
                .AsNoTracking()
                .ToList();

            ViewBag.HeroBanner = activeBanners.FirstOrDefault(x => x.Position == "Hero");
            ViewBag.BottomBanners = activeBanners
                .Where(x => x.Position == "Bottom")
                .Take(2)
                .ToList();

            return View();
        }
        public IActionResult Shop(string searchString)
        {
            // Không cần xử lý lọc ở đây vì file JS của ông sẽ tự làm việc đó
            // Chỉ cần trả về View Shop trống để JS nhảy vào load đồ
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
            // Lấy tên và UserId từ Session
            var userName = HttpContext.Session.GetString("UserName");
            var userId = HttpContext.Session.GetInt32("UserId");

            // Nếu chưa đăng nhập, bắt quay xe về trang Login
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Gửi tên sang View để hiện thị
            ViewBag.CurrentUserName = userName;
            return View();
        }
    }
}
