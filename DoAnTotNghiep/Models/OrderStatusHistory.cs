#nullable disable
using System;

namespace DoAnTotNghiep.Models;

public partial class OrderStatusHistory
{
    public int Id { get; set; }

    public int MaDonHang { get; set; }

    public string Status { get; set; }

    public int? ChangedByUserId { get; set; }

    public string Note { get; set; }

    public DateTime ChangedAt { get; set; }
}
