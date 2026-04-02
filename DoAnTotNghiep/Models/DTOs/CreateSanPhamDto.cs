namespace DoAnTotNghiep.Models.DTOs;

/// <summary>
/// DTO để tạo sản phẩm mới
/// </summary>
public class CreateSanPhamDto
{
    public string TenSanPham { get; set; } = null!;

    public decimal GiaTien { get; set; }

    public string? MoTa { get; set; }

    public string? HinhAnh { get; set; }

    public int? MaDanhMuc { get; set; }
}