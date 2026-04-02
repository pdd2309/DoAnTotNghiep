using System;
using System.Collections.Generic;

namespace DoAnTotNghiep.Models;

public partial class SanPham
{
    public int MaSanPham { get; set; }

    public string TenSanPham { get; set; } = null!;

    public decimal GiaTien { get; set; }

    public string? MoTa { get; set; }

    public string? HinhAnh { get; set; }

    public int? MaDanhMuc { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<DanhGia> DanhGia { get; set; } = new List<DanhGia>();

    public virtual DanhMuc? MaDanhMucNavigation { get; set; }
}
