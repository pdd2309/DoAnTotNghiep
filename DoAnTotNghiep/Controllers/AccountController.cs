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
        public IActionResult Login()
        {
            return View();
        }
        // Thêm hàm này vào AccountController.cs
        public IActionResult LichSuMuaHang()
        {
            // 1. Lấy ID người dùng từ Session
            int? userId = HttpContext.Session.GetInt32("UserId");

            // 2. Nếu chưa đăng nhập thì đá về trang Login
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            // 3. Truy vấn danh sách đơn hàng của người dùng này từ SQL
            // Sắp xếp đơn mới nhất lên đầu (OrderByDescending)
            var danhSachDonHang = _db.DonHangs
                .Where(d => d.MaNguoiDung == userId)
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            return View(danhSachDonHang);
        }
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _db.NguoiDungs.FirstOrDefault(u => u.TenDangNhap == username && u.MatKhau == password);

            if (user != null)
            {
                var displayName = user.HoTen ?? user.TenDangNhap;

                HttpContext.Session.SetInt32("UserId", user.MaNguoiDung);
                HttpContext.Session.SetString("UserName", displayName);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.MaNguoiDung.ToString()),
                    new Claim(ClaimTypes.Name, displayName)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal);

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

        // --- XỬ LÝ ĐĂNG NHẬP BẰNG GOOGLE / FACEBOOK ---

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

            if (!authenticateResult.Succeeded)
            {
                return RedirectToAction("Login");
            }

            var claims = authenticateResult.Principal.Identities.FirstOrDefault()?.Claims;

            // 1. Lấy ProviderKey (ID định danh của GG/FB) - Cái này luôn luôn có
            var providerKey = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            // 2. Lấy Email và Name (Có thì tốt, không có cũng không sao)
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                     ?? claims?.FirstOrDefault(c => c.Type.Contains("emailaddress"))?.Value;

            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                    ?? claims?.FirstOrDefault(c => c.Type.Contains("name"))?.Value;

            if (string.IsNullOrEmpty(providerKey))
            {
                ViewBag.Error = "Không thể xác định danh tính từ tài khoản liên kết.";
                return View("Login");
            }

            // 3. Tìm user trong DB: Ưu tiên tìm theo ProviderKey, nếu không thấy thì tìm theo Email
            var user = _db.NguoiDungs.FirstOrDefault(u => u.TenDangNhap == providerKey || (email != null && u.Email == email));

            if (user == null)
            {
                // Nếu chưa có, tạo tài khoản mới. 
                // Dùng providerKey làm TenDangNhap để đảm bảo tính duy nhất.
                user = new NguoiDung
                {
                    TenDangNhap = providerKey,
                    MatKhau = "",
                    HoTen = name ?? "Khách hàng MXH",
                    Email = email ?? (providerKey + "@social.com") // Tạo email giả nếu FB không cho
                };

                _db.NguoiDungs.Add(user);
                _db.SaveChanges();
            }

            // Lưu Session
            HttpContext.Session.SetInt32("UserId", user.MaNguoiDung);
            HttpContext.Session.SetString("UserName", user.HoTen ?? user.TenDangNhap);

            return RedirectToAction("Index", "Home");
        }
    }
}