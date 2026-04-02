using System;
using System.Collections.Generic;

namespace DoAnTotNghiep.Models;

public partial class GioHang
{
    public int MaGioHang { get; set; }

    public int MaNguoiDung { get; set; }

    public int MaSanPham { get; set; }

    public int? SoLuong { get; set; }

    public DateTime? NgayThem { get; set; }

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;

    public virtual SanPham MaSanPhamNavigation { get; set; } = null!;
}
