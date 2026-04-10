#nullable disable
using System;

namespace DoAnTotNghiep.Models;

public partial class Voucher
{
    public int Id { get; set; }

    public string Code { get; set; }

    public string Name { get; set; }

    public string DiscountType { get; set; }

    public decimal DiscountValue { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    public decimal? MinOrderAmount { get; set; }

    public int Quantity { get; set; }

    public bool IsActive { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime CreatedAt { get; set; }
}
