using Microsoft.AspNetCore.Mvc;
using DoAnTotNghiep.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnTotNghiep.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DanhMucController : ControllerBase
{
    private readonly CuaHangCongNgheDBContext _context;

    public DanhMucController(CuaHangCongNgheDBContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy tất cả danh mục
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DanhMuc>>> GetDanhMucs()
    {
        var danhMucs = await _context.DanhMucs.ToListAsync();
        return Ok(danhMucs);
    }

    /// <summary>
    /// Lấy danh mục theo ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DanhMuc>> GetDanhMuc(int id)
    {
        var danhMuc = await _context.DanhMucs.FindAsync(id);
        if (danhMuc == null)
            return NotFound();
        
        return Ok(danhMuc);
    }

    /// <summary>
    /// Thêm danh mục mới
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DanhMuc>> CreateDanhMuc(DanhMuc danhMuc)
    {
        _context.DanhMucs.Add(danhMuc);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetDanhMuc), new { id = danhMuc.MaDanhMuc }, danhMuc);
    }

    /// <summary>
    /// Cập nhật danh mục
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDanhMuc(int id, DanhMuc danhMuc)
    {
        if (id != danhMuc.MaDanhMuc)
            return BadRequest();

        _context.Entry(danhMuc).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DanhMucExists(id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Xóa danh mục
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDanhMuc(int id)
    {
        var danhMuc = await _context.DanhMucs.FindAsync(id);
        if (danhMuc == null)
            return NotFound();

        _context.DanhMucs.Remove(danhMuc);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private bool DanhMucExists(int id)
    {
        return _context.DanhMucs.Any(e => e.MaDanhMuc == id);
    }
}