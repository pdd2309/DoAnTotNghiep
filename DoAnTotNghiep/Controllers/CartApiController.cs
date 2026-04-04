using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnTotNghiep.Controllers
{
    [ApiController]
    [Route("api/Cart")] // <- use fixed route so client calls /api/Cart/...
    public class CartApiController : ControllerBase
    {
        private readonly CuaHangCongNgheDBContext _db;
        public CartApiController(CuaHangCongNgheDBContext db) => _db = db;

        // Helper: lấy username từ session
        private string? GetUserName() => HttpContext.Session.GetString("UserName");

        // DTOs
        public class CartItemDto
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; } = 1;
            public decimal Price { get; set; }
        }

        public class MergeCartDto
        {
            public List<CartItemDto> Items { get; set; } = new();
        }

        // GET api/Cart
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var user = GetUserName();
            if (user == null) return Unauthorized();

            // load cart and items, include product info in one query
            var cart = await _db.Carts
                .Where(c => c.UserName == user)
                .Select(c => new
                {
                    c.Id,
                    c.UserName,
                    Items = c.CartItems.Select(i => new
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        Price = i.Price,
                        // try to include product info if available
                        Product = _db.SanPhams
                            .Where(p => p.MaSanPham == i.ProductId)
                            .Select(p => new { p.MaSanPham, p.TenSanPham, p.HinhAnh, p.GiaTien })
                            .FirstOrDefault()
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (cart == null)
            {
                return Ok(new { items = new object[0] });
            }

            // normalize to a single items array for the client: id, productId, quantity, price, name, image
            var items = cart.Items.Select(i => new {
                id = i.Id,
                productId = i.ProductId,
                quantity = i.Quantity,
                price = i.Price,
                name = i.Product?.TenSanPham ?? "",
                image = i.Product?.HinhAnh ?? "",
            }).ToList();

            return Ok(new { items });
        }

        // POST api/Cart/Add
        [HttpPost("Add")]
        public async Task<IActionResult> AddItem([FromBody] CartItemDto dto)
        {
            var user = GetUserName();
            if (user == null) return Unauthorized();

            var cart = await _db.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserName == user);
            if (cart == null)
            {
                cart = new Cart { UserName = user };
                _db.Carts.Add(cart);
            }

            var item = cart.CartItems.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (item != null)
            {
                item.Quantity += dto.Quantity;
                item.Price = dto.Price;
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    Price = dto.Price
                });
            }

            await _db.SaveChangesAsync();
            return Ok(cart);
        }

        // PUT api/Cart/Update
        [HttpPut("Update")]
        public async Task<IActionResult> UpdateItem([FromBody] CartItemDto dto)
        {
            var user = GetUserName();
            if (user == null) return Unauthorized();

            var cart = await _db.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserName == user);
            if (cart == null) return NotFound();

            var item = cart.CartItems.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (item == null) return NotFound();

            // QUAN TRỌNG: Giữ nguyên Price, không được thay đổi
            decimal originalPrice = item.Price;

            if (dto.Quantity <= 0)
            {
                cart.CartItems.Remove(item);
            }
            else
            {
                item.Quantity = dto.Quantity;
                // ĐỮ NGUYÊN PRICE - KHÔNG THAY ĐỔI
                item.Price = originalPrice;
            }

            await _db.SaveChangesAsync();
            return Ok(cart);
        }

        // DELETE api/Cart/{productId}
        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            var user = GetUserName();
            if (user == null) return Unauthorized();

            var cart = await _db.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserName == user);

            if (cart == null) return NotFound();

            // Tìm các item cần xóa và dùng DbSet.RemoveRange để xóa an toàn với EF
            var itemsToRemove = cart.CartItems.Where(i => i.ProductId == productId).ToList();
            if (!itemsToRemove.Any()) return Ok(cart);

            _db.CartItems.RemoveRange(itemsToRemove);
            await _db.SaveChangesAsync();

            var updatedCart = await _db.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserName == user);

            return Ok(updatedCart);
        }

        // POST api/Cart/MergeLocal
        [HttpPost("MergeLocal")]
        public async Task<IActionResult> MergeLocal([FromBody] MergeCartDto dto)
        {
            var user = GetUserName();
            if (user == null) return Unauthorized();

            var cart = await _db.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserName == user);
            if (cart == null)
            {
                cart = new Cart { UserName = user };
                _db.Carts.Add(cart);
            }

            foreach (var incoming in dto.Items)
            {
                var item = cart.CartItems.FirstOrDefault(i => i.ProductId == incoming.ProductId);
                if (item != null) item.Quantity += incoming.Quantity;
                else cart.CartItems.Add(new CartItem
                {
                    ProductId = incoming.ProductId,
                    Quantity = incoming.Quantity,
                    Price = incoming.Price
                });
            }

            await _db.SaveChangesAsync();
            return Ok(cart);
        }
    }
}