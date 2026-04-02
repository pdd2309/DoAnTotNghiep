using DoAnTotNghiep.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnTotNghiep.Services;

public interface IDanhMucService
{
    Task<List<DanhMuc>> GetAllDanhMucsAsync();
    Task<DanhMuc?> GetDanhMucByIdAsync(int id);
}

public class DanhMucService : IDanhMucService
{
    private readonly CuaHangCongNgheDbContext _context;

    public DanhMucService(CuaHangCongNgheDbContext context)
    {
        _context = context;
    }

    public async Task<List<DanhMuc>> GetAllDanhMucsAsync()
    {
        return await _context.DanhMucs
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<DanhMuc?> GetDanhMucByIdAsync(int id)
    {
        return await _context.DanhMucs
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.MaDanhMuc == id);
    }
}