using System;
using System.Collections.Generic;

namespace DoAnTotNghiep.Models;

public partial class DanhGium
{
    public int MaDanhGia { get; set; }

    public int? MaSanPham { get; set; }

    public int? MaNguoiDung { get; set; }

    public int? SoSao { get; set; }

    public string? NoiDung { get; set; }

    public DateTime? NgayDanhGia { get; set; }

    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }

    public virtual SanPham? MaSanPhamNavigation { get; set; }
}
