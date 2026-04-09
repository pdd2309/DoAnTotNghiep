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
                .Select(d => new
                {
                    maDonHang = d.MaDonHang,
                    hoTen = d.HoTen,
                    tenDangNhap = d.MaNguoiDungNavigation != null ? d.MaNguoiDungNavigation.TenDangNhap : null,
                    ngayDat = d.NgayDat,
                    tongTien = d.TongTien,
                    trangThai = d.TrangThai
                })
                .ToListAsync();

            return Ok(orders);
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
                trangThai = order.TrangThai,
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
            var statusChoXuLy = "Ch\u1EDD x\u1EED l\u00FD";
            var statusDangGiao = "\u0110ang giao";
            var statusDaGiao = "\u0110\u00E3 giao h\u00E0ng";
            var statusDaHuy = "\u0110\u00E3 h\u1EE7y";
            var allowedStatuses = new[] { statusChoXuLy, statusDangGiao, statusDaGiao, statusDaHuy };

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

        public class UpdateOrderStatusRequest
        {
            public string TrangThai { get; set; } = string.Empty;
        }
    }
}
