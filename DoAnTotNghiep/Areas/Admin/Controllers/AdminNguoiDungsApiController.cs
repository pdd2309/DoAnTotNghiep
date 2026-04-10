using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DoAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/users")]
    public class AdminNguoiDungsApiController : ControllerBase
    {
        private readonly CuaHangCongNgheDBContext _context;

        public AdminNguoiDungsApiController(CuaHangCongNgheDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _context.NguoiDungs
                .OrderByDescending(x => x.MaNguoiDung)
                .Select(x => new
                {
                    maNguoiDung = x.MaNguoiDung,
                    tenDangNhap = x.TenDangNhap,
                    hoTen = x.HoTen,
                    email = x.Email,
                    vaiTro = x.VaiTro
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPut("{id:int}/role")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateUserRoleRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.VaiTro))
            {
                return BadRequest(new { message = "Role is required." });
            }

            var validRoles = new[] { "Admin", "KhachHang" };
            if (!validRoles.Contains(request.VaiTro))
            {
                return BadRequest(new { message = "Role is invalid." });
            }

            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var currentUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(currentUserIdClaim, out var currentUserId)
                && currentUserId == id
                && request.VaiTro != "Admin")
            {
                return BadRequest(new { message = "Cannot remove Admin role of current account." });
            }

            user.VaiTro = request.VaiTro;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                maNguoiDung = user.MaNguoiDung,
                vaiTro = user.VaiTro
            });
        }

        public class UpdateUserRoleRequest
        {
            public string VaiTro { get; set; } = string.Empty;
        }
    }
}
