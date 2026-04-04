using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnTotNghiep.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaiVietApiController : ControllerBase
{
    private readonly CuaHangCongNgheDBContext _context;

    public BaiVietApiController(CuaHangCongNgheDBContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy danh sách bài viết
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BaiViet>>> GetBaiViets()
    {
        var baiViets = await _context.BaiViets.ToListAsync();
        return Ok(baiViets);
    }

    /// <summary>
    /// Lấy chi tiết bài viết theo ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<BaiViet>> GetBaiViet(int id)
    {
        var baiViet = await _context.BaiViets.FindAsync(id);
        if (baiViet == null)
            return NotFound();

        return Ok(baiViet);
    }
}