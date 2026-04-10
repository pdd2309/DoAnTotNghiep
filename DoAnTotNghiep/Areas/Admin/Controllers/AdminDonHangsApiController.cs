using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/orders")]
    public class AdminDonHangsApiController : ControllerBase
    {
        private const string StatusChoXuLy = "Ch\u1EDD x\u1EED l\u00FD";
        private const string StatusDangGiao = "\u0110ang giao";
        private const string StatusDaGiao = "\u0110\u00E3 giao h\u00E0ng";
        private const string StatusDaHuy = "\u0110\u00E3 h\u1EE7y";
        private const string StatusDaThanhToan = "\u0110\u00E3 thanh to\u00E1n";

        private readonly CuaHangCongNgheDBContext _context;

        public AdminDonHangsApiController(CuaHangCongNgheDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _context.DonHangs
                .Include(d => d.MaNguoiDungNavigation)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            return Ok(orders.Select(d => new
            {
                maDonHang = d.MaDonHang,
                hoTen = d.HoTen,
                tenDangNhap = d.MaNguoiDungNavigation != null ? d.MaNguoiDungNavigation.TenDangNhap : null,
                ngayDat = d.NgayDat,
                tongTien = d.TongTien,
                trangThai = NormalizeTrangThai(d.TrangThai)
            }));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _context.DonHangs
                .Include(d => d.MaNguoiDungNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaSanPhamNavigation)
                .FirstOrDefaultAsync(d => d.MaDonHang == id);

            if (order == null) return NotFound();

            return Ok(new
            {
                maDonHang = order.MaDonHang,
                hoTen = order.HoTen,
                tenDangNhap = order.MaNguoiDungNavigation?.TenDangNhap,
                soDienThoai = order.SoDienThoai,
                email = order.Email,
                diaChiGiaoHang = order.DiaChiGiaoHang,
                ngayDat = order.NgayDat,
                tongTien = order.TongTien,
                trangThai = NormalizeTrangThai(order.TrangThai),
                ghiChu = order.GhiChu,
                chiTiet = order.ChiTietDonHangs.Select(ct => new
                {
                    maSanPham = ct.MaSanPham,
                    tenSanPham = ct.MaSanPhamNavigation != null ? ct.MaSanPhamNavigation.TenSanPham : null,
                    donGia = ct.DonGiaLucMua,
                    soLuong = ct.SoLuong,
                    thanhTien = (ct.DonGiaLucMua ?? 0) * (ct.SoLuong ?? 0)
                })
            });
        }

        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            var allowedStatuses = new[] { StatusChoXuLy, StatusDangGiao, StatusDaGiao, StatusDaHuy, StatusDaThanhToan };

            if (request == null || string.IsNullOrWhiteSpace(request.TrangThai) || !allowedStatuses.Contains(request.TrangThai))
            {
                return BadRequest(new { message = "Invalid order status." });
            }

            var order = await _context.DonHangs.FindAsync(id);
            if (order == null) return NotFound();

            order.TrangThai = request.TrangThai;
            await _context.SaveChangesAsync();

            return Ok(new { maDonHang = order.MaDonHang, trangThai = order.TrangThai });
        }

        private static string NormalizeTrangThai(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return StatusChoXuLy;
            var s = raw.Trim().ToLowerInvariant();

            if (s.Contains("h?y") || s.Contains("huy")) return StatusDaHuy;
            if (s.Contains("thanh toán") || s.Contains("thanh toan")) return StatusDaThanhToan;
            if (s.Contains("giao"))
            {
                if (s.Contains("?ang") || s.Contains("dang")) return StatusDangGiao;
                return StatusDaGiao;
            }

            if (s.Contains("x?") || s.Contains("xu") || s.Contains("ch?") || s.Contains("cho")) return StatusChoXuLy;
            return raw;
        }

        public class UpdateOrderStatusRequest
        {
            public string TrangThai { get; set; } = string.Empty;
        }
    }
}
