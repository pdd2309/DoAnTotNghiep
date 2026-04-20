#nullable disable
using System;

namespace DoAnTotNghiep.Models;

public partial class AddressBook
{
    public int Id { get; set; }

    public int MaNguoiDung { get; set; }

    public string FullName { get; set; }

    public string Phone { get; set; }

    public string AddressLine { get; set; }

    public string Ward { get; set; }

    public string District { get; set; }

    public string Province { get; set; }

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; }
}
