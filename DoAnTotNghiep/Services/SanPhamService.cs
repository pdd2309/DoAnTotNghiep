using DoAnTotNghiep.Models;
using DoAnTotNghiep.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DoAnTotNghiep.Services;

public interface ISanPhamService
{
    Task<List<SanPham>> GetAllSanPhamsAsync();
    Task<SanPham?> GetSanPhamByIdAsync(int id);
    Task<List<SanPham>> GetSanPhamByCategoryAsync(int categoryId);
    Task<SanPham> CreateSanPhamAsync(CreateSanPhamDto dto);
    Task<SanPham> UpdateSanPhamAsync(int id, UpdateSanPhamDto dto);
    Task<bool> DeleteSanPhamAsync(int id);
    Task<List<SanPham>> SearchSanPhamAsync(string keyword);
}

public class SanPhamService : ISanPhamService
{
    private readonly CuaHangCongNgheDbContext _context;

    public SanPhamService(CuaHangCongNgheDbContext context)
    {
        _context = context;
    }

    public async Task<List<SanPham>> GetAllSanPhamsAsync()
    {
        return await _context.SanPhams
            .Include(sp => sp.MaDanhMucNavigation)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<SanPham?> GetSanPhamByIdAsync(int id)
    {
        return await _context.SanPhams
            .Include(sp => sp.MaDanhMucNavigation)
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.MaSanPham == id);
    }

    public async Task<List<SanPham>> GetSanPhamByCategoryAsync(int categoryId)
    {
        return await _context.SanPhams
            .Where(sp => sp.MaDanhMuc == categoryId)
            .Include(sp => sp.MaDanhMucNavigation)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<SanPham> CreateSanPhamAsync(CreateSanPhamDto dto)
    {
        var sanPham = new SanPham
        {
            TenSanPham = dto.TenSanPham,
            GiaTien = dto.GiaTien,
            MoTa = dto.MoTa,
            HinhAnh = dto.HinhAnh,
            MaDanhMuc = dto.MaDanhMuc
        };

        _context.SanPhams.Add(sanPham);
        await _context.SaveChangesAsync();
        return sanPham;
    }

    public async Task<SanPham> UpdateSanPhamAsync(int id, UpdateSanPhamDto dto)
    {
        var sanPham = await _context.SanPhams.FindAsync(id);
        if (sanPham == null)
            throw new KeyNotFoundException($"Sản phẩm ID {id} không tồn tại");

        if (!string.IsNullOrWhiteSpace(dto.TenSanPham))
            sanPham.TenSanPham = dto.TenSanPham;

        if (dto.GiaTien.HasValue)
            sanPham.GiaTien = dto.GiaTien.Value;

        if (dto.MoTa != null)
            sanPham.MoTa = dto.MoTa;

        if (!string.IsNullOrWhiteSpace(dto.HinhAnh))
            sanPham.HinhAnh = dto.HinhAnh;

        if (dto.MaDanhMuc.HasValue)
            sanPham.MaDanhMuc = dto.MaDanhMuc;

        _context.SanPhams.Update(sanPham);
        await _context.SaveChangesAsync();
        return sanPham;
    }

    public async Task<bool> DeleteSanPhamAsync(int id)
    {
        var sanPham = await _context.SanPhams.FindAsync(id);
        if (sanPham == null)
            throw new KeyNotFoundException($"Sản phẩm ID {id} không tồn tại");

        _context.SanPhams.Remove(sanPham);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<SanPham>> SearchSanPhamAsync(string keyword)
    {
        return await _context.SanPhams
            .Where(sp => sp.TenSanPham.Contains(keyword))
            .Include(sp => sp.MaDanhMucNavigation)
            .AsNoTracking()
            .ToListAsync();
    }
}