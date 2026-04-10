using Microsoft.AspNetCore.Mvc;
using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace DoAnTotNghiep.Controllers
{
    public class AccountController : Controller
    {
        private readonly CuaHangCongNgheDBContext _db;

        public AccountController(CuaHangCongNgheDBContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _db.NguoiDungs.FirstOrDefault(u => u.TenDangNhap == username && u.MatKhau == password);

            if (user != null)
            {
                var displayName = user.HoTen ?? user.TenDangNhap;

                // --- LƯU SONG SONG CẢ ID VÀ TÊN ---
                // Dùng UserId (int) để code truy vấn nhanh và chính xác
                HttpContext.Session.SetInt32("UserId", user.MaNguoiDung);
                // Dùng UserName (string) để hiện tên trên Layout và lưu vào DB cho ông dễ nhìn
                HttpContext.Session.SetString("UserName", displayName);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.MaNguoiDung.ToString()),
                    new Claim(ClaimTypes.Name, displayName)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Sai tài khoản hoặc mật khẩu!";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult LichSuMuaHang()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var danhSachDonHang = _db.DonHangs
                .Where(d => d.MaNguoiDung == userId)
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            return View(danhSachDonHang);
        }

        [HttpGet]
        public IActionResult Profile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = _db.NguoiDungs.FirstOrDefault(u => u.MaNguoiDung == userId.Value);
            if (user == null) return RedirectToAction("Login");

            return View(user);
        }

        [HttpGet]
        public IActionResult ExternalLogin(string provider)
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("ExternalLoginCallback") };
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded) return RedirectToAction("Login");

            var claims = authenticateResult.Principal.Identities.FirstOrDefault()?.Claims;
            var providerKey = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                     ?? claims?.FirstOrDefault(c => c.Type.Contains("emailaddress"))?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                    ?? claims?.FirstOrDefault(c => c.Type.Contains("name"))?.Value;

            if (string.IsNullOrEmpty(providerKey)) return RedirectToAction("Login");

            var user = _db.NguoiDungs.FirstOrDefault(u => u.TenDangNhap == providerKey || (email != null && u.Email == email));

            if (user == null)
            {
                user = new NguoiDung
                {
                    TenDangNhap = providerKey,
                    MatKhau = "",
                    HoTen = name ?? "Khách hàng MXH",
                    Email = email ?? (providerKey + "@social.com")
                };
                _db.NguoiDungs.Add(user);
                _db.SaveChanges();
            }

            HttpContext.Session.SetInt32("UserId", user.MaNguoiDung);
            HttpContext.Session.SetString("UserName", user.HoTen ?? user.TenDangNhap);

            return RedirectToAction("Index", "Home");
        }
    }
}