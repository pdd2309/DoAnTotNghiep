namespace DoAnTotNghiep.Models.DTOs;

/// <summary>
/// DTO để cập nhật sản phẩm (các trường có thể null)
/// </summary>
public class UpdateSanPhamDto
{
    public string? TenSanPham { get; set; }

    public decimal? GiaTien { get; set; }

    public string? MoTa { get; set; }

    public string? HinhAnh { get; set; }

    public int? MaDanhMuc { get; set; }
}