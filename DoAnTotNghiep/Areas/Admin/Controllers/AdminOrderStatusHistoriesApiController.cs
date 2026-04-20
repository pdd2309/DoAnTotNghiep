using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/order-status-histories")]
    public class AdminOrderStatusHistoriesApiController : ControllerBase
    {
        private readonly CuaHangCongNgheDBContext _context;

        public AdminOrderStatusHistoriesApiController(CuaHangCongNgheDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.OrderStatusHistories
                .GroupJoin(_context.NguoiDungs,
                    h => h.ChangedByUserId,
                    u => (int?)u.MaNguoiDung,
                    (h, users) => new { h, users })
                .SelectMany(x => x.users.DefaultIfEmpty(), (x, u) => new
                {
                    id = x.h.Id,
                    maDonHang = x.h.MaDonHang,
                    status = x.h.Status,
                    changedByUserId = x.h.ChangedByUserId,
                    changedByUser = u != null ? u.TenDangNhap : null,
                    note = x.h.Note,
                    changedAt = x.h.ChangedAt
                })
                .OrderByDescending(x => x.changedAt)
                .ToListAsync();

            return Ok(items);
        }
    }
}
