#nullable disable
using System;

namespace DoAnTotNghiep.Models;

public partial class Banner
{
    public int Id { get; set; }

    public string Title { get; set; }

    public string SubTitle { get; set; }

    public string Description { get; set; }

    public string ImageUrl { get; set; }

    public string LinkUrl { get; set; }

    public string Position { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
