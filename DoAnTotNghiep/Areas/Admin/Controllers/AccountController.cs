using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity; // Cần thêm cái này để dùng PasswordHasher
using System.Security.Claims;

namespace DoAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly CuaHangCongNgheDBContext _db;
        // Thêm bộ băm mật khẩu giống hệt bên ngoài trang chủ
        private readonly PasswordHasher<NguoiDung> _passwordHasher = new();

        public AccountController(CuaHangCongNgheDBContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // 1. Tìm user theo tên đăng nhập và phải là Admin
            var user = _db.NguoiDungs.FirstOrDefault(u => u.TenDangNhap == username && u.VaiTro == "Admin");

            // 2. Sử dụng logic Verify thông minh (vừa check được Pass cũ, vừa check được Pass Hash)
            if (user != null && VerifyPassword(user, password, out var needsRehash))
            {
                // Nếu đăng nhập bằng pass cũ thành công, tự động nâng cấp lên Pass Hash cho bảo mật
                if (needsRehash)
                {
                    user.MatKhau = _passwordHasher.HashPassword(user, password);
                    _db.SaveChanges();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.MaNguoiDung.ToString()),
                    new Claim(ClaimTypes.Name, user.HoTen ?? user.TenDangNhap),
                    new Claim(ClaimTypes.Role, user.VaiTro ?? "Admin")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            ViewBag.Error = "Tài khoản không hợp lệ hoặc mật khẩu sai!";
            return View();
        }

        // Hàm bổ trợ để kiểm tra mật khẩu (Copy từ file AccountController gốc của ông)
        private bool VerifyPassword(NguoiDung user, string inputPassword, out bool needsRehash)
        {
            needsRehash = false;
            if (string.IsNullOrWhiteSpace(user.MatKhau) || string.IsNullOrWhiteSpace(inputPassword)) return false;

            PasswordVerificationResult result;
            try
            {
                result = _passwordHasher.VerifyHashedPassword(user, user.MatKhau, inputPassword);
            }
            catch
            {
                result = PasswordVerificationResult.Failed;
            }

            if (result == PasswordVerificationResult.Success) return true;
            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                needsRehash = true;
                return true;
            }

            // Pha cứu sinh: Nếu Database đang là pass cũ chưa mã hóa thì so sánh trực tiếp luôn
            if (user.MatKhau == inputPassword)
            {
                needsRehash = true;
                return true;
            }

            return false;
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account", new { area = "Admin" });
        }
    }
}