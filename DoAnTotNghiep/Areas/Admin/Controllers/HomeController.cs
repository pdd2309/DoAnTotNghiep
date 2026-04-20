using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnTotNghiep.Models;
using System.Text.Json;

namespace DoAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly CuaHangCongNgheDBContext _context;

        public HomeController(CuaHangCongNgheDBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var totalRevenue = _context.DonHangs.Sum(x => x.TongTien ?? 0);

            var pendingOrders = _context.DonHangs
                .Count(x => x.TrangThai == "Chờ xử lý");

            var todayRevenue = _context.DonHangs
                .Where(x => x.NgayDat >= today && x.NgayDat < tomorrow)
                .Sum(x => x.TongTien ?? 0);

            var vouchersExpiringSoon = _context.Vouchers
                .Count(x => x.IsActive
                    && x.EndDate != null
                    && x.EndDate >= today
                    && x.EndDate < today.AddDays(7));

            var topProducts = _context.ChiTietDonHangs
                .Where(x => x.MaSanPham != null)
                .GroupBy(x => x.MaSanPham)
                .Select(g => new
                {
                    MaSanPham = g.Key,
                    SoLuongBan = g.Sum(x => x.SoLuong ?? 0)
                })
                .OrderByDescending(x => x.SoLuongBan)
                .Take(5)
                .Join(_context.SanPhams,
                    x => x.MaSanPham,
                    sp => sp.MaSanPham,
                    (x, sp) => new
                    {
                        TenSanPham = sp.TenSanPham,
                        SoLuongBan = x.SoLuongBan
                    })
                .ToList();

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.PendingOrders = pendingOrders;
            ViewBag.TodayRevenue = todayRevenue;
            ViewBag.VouchersExpiringSoon = vouchersExpiringSoon;
            ViewBag.TopProductLabels = JsonSerializer.Serialize(topProducts.Select(x => x.TenSanPham).ToList());
            ViewBag.TopProductValues = JsonSerializer.Serialize(topProducts.Select(x => x.SoLuongBan).ToList());

            return View();
        }
    }
}