#nullable disable
using System;

namespace DoAnTotNghiep.Models;

public partial class PaymentTransaction
{
    public int Id { get; set; }

    public int MaDonHang { get; set; }

    public string Provider { get; set; }

    public string TransactionNo { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; }

    public string ResponseCode { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
