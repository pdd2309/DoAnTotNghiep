using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnTotNghiep.Models;

[Table("DanhGia")]
public partial class DanhGia
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
