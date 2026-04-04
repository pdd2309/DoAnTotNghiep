using System.Collections.Generic;

namespace DoAnTotNghiep.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!; // dùng UserName từ Session hoặc Claim làm key
        public virtual List<CartItem> Items { get; set; } = new();
    }
}