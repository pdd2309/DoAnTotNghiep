using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/address-books")]
    public class AdminAddressBooksApiController : ControllerBase
    {
        private readonly CuaHangCongNgheDBContext _context;

        public AdminAddressBooksApiController(CuaHangCongNgheDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.AddressBooks
                .Join(_context.NguoiDungs,
                    ab => ab.MaNguoiDung,
                    u => u.MaNguoiDung,
                    (ab, u) => new
                    {
                        id = ab.Id,
                        maNguoiDung = ab.MaNguoiDung,
                        tenDangNhap = u.TenDangNhap,
                        hoTen = ab.FullName,
                        phone = ab.Phone,
                        diaChi = ab.AddressLine,
                        ward = ab.Ward,
                        district = ab.District,
                        province = ab.Province,
                        isDefault = ab.IsDefault,
                        createdAt = ab.CreatedAt
                    })
                .OrderByDescending(x => x.createdAt)
                .ToListAsync();

            return Ok(items);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.AddressBooks.FindAsync(id);
            if (entity == null) return NotFound();

            _context.AddressBooks.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
