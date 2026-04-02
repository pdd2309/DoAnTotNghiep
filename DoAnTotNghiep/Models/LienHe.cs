using System;
using System.Collections.Generic;

namespace DoAnTotNghiep.Models;

public partial class LienHe
{
    public int MaLienHe { get; set; }

    public string? HoTen { get; set; }

    public string? Email { get; set; }

    public string? TinNhan { get; set; }

    public DateTime? NgayGui { get; set; }
}
