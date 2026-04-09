using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/categories")]
    public class AdminDanhMucsApiController : ControllerBase
    {
        private readonly CuaHangCongNgheDBContext _context;

        public AdminDanhMucsApiController(CuaHangCongNgheDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.DanhMucs
                .OrderByDescending(x => x.MaDanhMuc)
                .Select(x => new
                {
                    maDanhMuc = x.MaDanhMuc,
                    tenDanhMuc = x.TenDanhMuc
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _context.DanhMucs
                .Where(x => x.MaDanhMuc == id)
                .Select(x => new
                {
                    maDanhMuc = x.MaDanhMuc,
                    tenDanhMuc = x.TenDanhMuc
                })
                .FirstOrDefaultAsync();

            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DanhMuc model)
        {
            if (string.IsNullOrWhiteSpace(model.TenDanhMuc))
            {
                return BadRequest(new { message = "Category name is required." });
            }

            var entity = new DanhMuc
            {
                TenDanhMuc = model.TenDanhMuc.Trim()
            };

            _context.DanhMucs.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = entity.MaDanhMuc }, new
            {
                maDanhMuc = entity.MaDanhMuc,
                tenDanhMuc = entity.TenDanhMuc
            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] DanhMuc model)
        {
            if (string.IsNullOrWhiteSpace(model.TenDanhMuc))
            {
                return BadRequest(new { message = "Category name is required." });
            }

            var entity = await _context.DanhMucs.FindAsync(id);
            if (entity == null) return NotFound();

            entity.TenDanhMuc = model.TenDanhMuc.Trim();
            await _context.SaveChangesAsync();

            return Ok(new
            {
                maDanhMuc = entity.MaDanhMuc,
                tenDanhMuc = entity.TenDanhMuc
            });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.DanhMucs.FindAsync(id);
            if (entity == null) return NotFound();

            _context.DanhMucs.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
