#nullable disable
using System;

namespace DoAnTotNghiep.Models;

public partial class VoucherUsage
{
    public int Id { get; set; }

    public int VoucherId { get; set; }

    public int MaNguoiDung { get; set; }

    public int MaDonHang { get; set; }

    public string VoucherCode { get; set; }

    public decimal DiscountAmount { get; set; }

    public DateTime UsedAt { get; set; }
}
