using Microsoft.AspNetCore.Mvc;
using DoAnTotNghiep.Models; // Thay bằng namespace chuẩn của Đông nếu khác
using Microsoft.AspNetCore.Http;

namespace DoAnTotNghiep.Controllers
{
    public class AccountController : Controller
    {
        private readonly CuaHangCongNgheDbContext _db;

        public AccountController(CuaHangCongNgheDbContext db)
        {
            _db = db;
        }

        // Trang hiển thị Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Xử lý khi nhấn nút Đăng nhập
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Kiểm tra trong SQL
            var user = _db.NguoiDungs.FirstOrDefault(u => u.TenDangNhap == username && u.MatKhau == password);

            if (user != null)
            {
                // LƯU QUAN TRỌNG: Cất MaNguoiDung vào Session để dùng cho Giỏ Hàng
                HttpContext.Session.SetInt32("UserId", user.MaNguoiDung);
                HttpContext.Session.SetString("UserName", user.HoTen ?? user.TenDangNhap);

                return RedirectToAction("Index", "Home"); // Đăng nhập xong về trang chủ
            }

            ViewBag.Error = "Sai tài khoản hoặc mật khẩu rồi Đông ơi!";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa sạch session khi thoát
            return RedirectToAction("Login");
        }
    }
}