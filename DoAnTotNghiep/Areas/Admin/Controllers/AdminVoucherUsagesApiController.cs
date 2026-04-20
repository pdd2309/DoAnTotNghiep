using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/voucher-usages")]
    public class AdminVoucherUsagesApiController : ControllerBase
    {
        private readonly CuaHangCongNgheDBContext _context;

        public AdminVoucherUsagesApiController(CuaHangCongNgheDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.VoucherUsages
                .Join(_context.Vouchers,
                    vu => vu.VoucherId,
                    v => v.Id,
                    (vu, v) => new { vu, v })
                .Join(_context.NguoiDungs,
                    x => x.vu.MaNguoiDung,
                    u => u.MaNguoiDung,
                    (x, u) => new { x.vu, x.v, u })
                .OrderByDescending(x => x.vu.UsedAt)
                .Select(x => new
                {
                    id = x.vu.Id,
                    maDonHang = x.vu.MaDonHang,
                    voucherCode = x.vu.VoucherCode,
                    voucherName = x.v.Name,
                    maNguoiDung = x.vu.MaNguoiDung,
                    tenDangNhap = x.u.TenDangNhap,
                    discountAmount = x.vu.DiscountAmount,
                    usedAt = x.vu.UsedAt
                })
                .ToListAsync();

            return Ok(items);
        }
    }
}
