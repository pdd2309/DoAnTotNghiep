using Microsoft.AspNetCore.Mvc;
using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace DoAnTotNghiep.Controllers
{
    public class AccountController : Controller
    {
        private readonly CuaHangCongNgheDBContext _db;
        private readonly PasswordHasher<NguoiDung> _passwordHasher = new();

        public AccountController(CuaHangCongNgheDBContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var normalizedUserName = username?.Trim();
            var user = _db.NguoiDungs.FirstOrDefault(u => u.TenDangNhap == normalizedUserName);

            if (user != null && VerifyPassword(user, password, out var needsRehash))
            {
                if (needsRehash)
                {
                    user.MatKhau = HashPassword(user, password);
                    _db.SaveChanges();
                }

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

        [HttpPost]
        public IActionResult Register(string username, string password, string confirmPassword, string fullName, string email)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin bắt buộc.";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            var normalizedUserName = username.Trim();
            var exists = _db.NguoiDungs.Any(u => u.TenDangNhap == normalizedUserName);
            if (exists)
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại.";
                return View();
            }

            var user = new NguoiDung
            {
                TenDangNhap = normalizedUserName,
                MatKhau = string.Empty,
                HoTen = string.IsNullOrWhiteSpace(fullName) ? normalizedUserName : fullName.Trim(),
                Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
                VaiTro = "KhachHang"
            };

            user.MatKhau = HashPassword(user, password);

            _db.NguoiDungs.Add(user);
            _db.SaveChanges();

            TempData["Success"] = "Đăng ký thành công. Vui lòng đăng nhập.";
            return RedirectToAction("Login");
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

        [HttpPost]
        public IActionResult Profile(string fullName, string email, string currentPassword, string newPassword, string confirmNewPassword)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = _db.NguoiDungs.FirstOrDefault(u => u.MaNguoiDung == userId.Value);
            if (user == null) return RedirectToAction("Login");

            if (string.IsNullOrWhiteSpace(fullName))
            {
                ViewBag.Error = "Họ tên không được để trống.";
                return View(user);
            }

            user.HoTen = fullName.Trim();
            user.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();

            var wantsChangePassword = !string.IsNullOrWhiteSpace(newPassword)
                || !string.IsNullOrWhiteSpace(confirmNewPassword)
                || !string.IsNullOrWhiteSpace(currentPassword);

            if (wantsChangePassword)
            {
                if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmNewPassword))
                {
                    ViewBag.Error = "Vui lòng nhập đầy đủ thông tin đổi mật khẩu.";
                    return View(user);
                }

                if (!VerifyPassword(user, currentPassword, out _))
                {
                    ViewBag.Error = "Mật khẩu hiện tại không đúng.";
                    return View(user);
                }

                if (newPassword != confirmNewPassword)
                {
                    ViewBag.Error = "Xác nhận mật khẩu mới không khớp.";
                    return View(user);
                }

                if (newPassword.Length < 6)
                {
                    ViewBag.Error = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                    return View(user);
                }

                user.MatKhau = HashPassword(user, newPassword);
            }

            _db.SaveChanges();
            HttpContext.Session.SetString("UserName", user.HoTen ?? user.TenDangNhap);
            ViewBag.Success = "Cập nhật thông tin thành công.";

            return View(user);
        }

        [HttpGet]
        public IActionResult AddressBook()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");
            return View();
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
                    MatKhau = string.Empty,
                    HoTen = name ?? "Khách hàng MXH",
                    Email = email ?? (providerKey + "@social.com")
                };
                user.MatKhau = HashPassword(user, Guid.NewGuid().ToString("N"));
                _db.NguoiDungs.Add(user);
                _db.SaveChanges();
            }

            HttpContext.Session.SetInt32("UserId", user.MaNguoiDung);
            HttpContext.Session.SetString("UserName", user.HoTen ?? user.TenDangNhap);

            return RedirectToAction("Index", "Home");
        }

        private string HashPassword(NguoiDung user, string password)
        {
            return _passwordHasher.HashPassword(user, password);
        }

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
                // Legacy or malformed stored password, fallback to plain text compare below.
                result = PasswordVerificationResult.Failed;
            }

            if (result == PasswordVerificationResult.Success)
            {
                return true;
            }

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                needsRehash = true;
                return true;
            }

            // Legacy fallback: old accounts stored plain text.
            if (user.MatKhau == inputPassword)
            {
                needsRehash = true;
                return true;
            }

            return false;
        }
    }
}