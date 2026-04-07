using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnTotNghiep.Controllers
{
    [ApiController]
    [Route("api/Cart")]
    public class CartApiController : ControllerBase
    {
        private readonly CuaHangCongNgheDBContext _db;

        public CartApiController(CuaHangCongNgheDBContext db) => _db = db;

        private int? GetUserId() => HttpContext.Session.GetInt32("UserId");
        private string? GetUserName() => HttpContext.Session.GetString("UserName");
        private static string BuildLegacyCartKey(int userId) => $"uid:{userId}";

        private async Task<Cart?> FindCartAsync(int userId, string? displayName, bool includeItems)
        {
            var legacyCartKey = BuildLegacyCartKey(userId);

            IQueryable<Cart> query = _db.Carts;
            if (includeItems)
            {
                query = query.Include(c => c.CartItems);
            }

            return await query.FirstOrDefaultAsync(c =>
                (!string.IsNullOrEmpty(displayName) && c.UserName == displayName) ||
                c.UserName == legacyCartKey);
        }

        private async Task<Cart> GetOrCreateCartAsync(int userId, string? displayName)
        {
            var cart = await FindCartAsync(userId, displayName, includeItems: true);
            if (cart != null)
            {
                // Chuẩn hóa để lưu tên user trong DB cho dễ nhìn
                if (!string.IsNullOrWhiteSpace(displayName) && cart.UserName != displayName)
                {
                    cart.UserName = displayName;
                    await _db.SaveChangesAsync();
                }

                return cart;
            }

            cart = new Cart
            {
                // KHÔNG gán Id thủ công vì Id thường là IDENTITY
                UserName = !string.IsNullOrWhiteSpace(displayName)
                    ? displayName
                    : BuildLegacyCartKey(userId)
            };

            _db.Carts.Add(cart);
            return cart;
        }

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

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetUserId();
            var displayName = GetUserName();
            if (userId == null) return Unauthorized();

            var cart = await FindCartAsync(userId.Value, displayName, includeItems: false);

            if (cart == null)
            {
                return Ok(new { items = new object[0] });
            }

            var items = await _db.CartItems
                .Where(i => i.CartId == cart.Id)
                .Select(i => new
                {
                    id = i.Id,
                    productId = i.ProductId,
                    quantity = i.Quantity,
                    price = i.Price,
                    name = _db.SanPhams
                        .Where(p => p.MaSanPham == i.ProductId)
                        .Select(p => p.TenSanPham)
                        .FirstOrDefault() ?? "",
                    image = _db.SanPhams
                        .Where(p => p.MaSanPham == i.ProductId)
                        .Select(p => p.HinhAnh)
                        .FirstOrDefault() ?? ""
                })
                .ToListAsync();

            return Ok(new { items });
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddItem([FromBody] CartItemDto dto)
        {
            if (dto == null || dto.ProductId <= 0 || dto.Quantity <= 0)
            {
                return BadRequest("Dữ liệu sản phẩm không hợp lệ.");
            }

            var userId = GetUserId();
            var displayName = GetUserName();
            if (userId == null) return Unauthorized();

            var cart = await GetOrCreateCartAsync(userId.Value, displayName);

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

        [HttpPut("Update")]
        public async Task<IActionResult> UpdateItem([FromBody] CartItemDto dto)
        {
            if (dto == null || dto.ProductId <= 0)
            {
                return BadRequest("Dữ liệu sản phẩm không hợp lệ.");
            }

            var userId = GetUserId();
            var displayName = GetUserName();
            if (userId == null) return Unauthorized();

            var cart = await FindCartAsync(userId.Value, displayName, includeItems: true);
            if (cart == null) return NotFound();

            var item = cart.CartItems.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (item == null) return NotFound();

            var originalPrice = item.Price;
            if (dto.Quantity <= 0)
            {
                cart.CartItems.Remove(item);
            }
            else
            {
                item.Quantity = dto.Quantity;
                item.Price = originalPrice;
            }

            await _db.SaveChangesAsync();
            return Ok(cart);
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            if (productId <= 0)
            {
                return BadRequest("productId không hợp lệ.");
            }

            var userId = GetUserId();
            var displayName = GetUserName();
            if (userId == null) return Unauthorized();

            var cart = await FindCartAsync(userId.Value, displayName, includeItems: true);
            if (cart == null) return NotFound();

            var itemsToRemove = cart.CartItems.Where(i => i.ProductId == productId).ToList();
            if (!itemsToRemove.Any()) return Ok(cart);

            _db.CartItems.RemoveRange(itemsToRemove);
            await _db.SaveChangesAsync();

            var updatedCart = await FindCartAsync(userId.Value, displayName, includeItems: true);
            return Ok(updatedCart);
        }

        [HttpPost("MergeLocal")]
        public async Task<IActionResult> MergeLocal([FromBody] MergeCartDto dto)
        {
            if (dto == null || dto.Items == null)
            {
                return BadRequest("Dữ liệu merge không hợp lệ.");
            }

            var userId = GetUserId();
            var displayName = GetUserName();
            if (userId == null) return Unauthorized();

            var cart = await GetOrCreateCartAsync(userId.Value, displayName);

            foreach (var incoming in dto.Items.Where(x => x.ProductId > 0 && x.Quantity > 0))
            {
                var item = cart.CartItems.FirstOrDefault(i => i.ProductId == incoming.ProductId);
                if (item != null)
                {
                    item.Quantity += incoming.Quantity;
                }
                else
                {
                    cart.CartItems.Add(new CartItem
                    {
                        ProductId = incoming.ProductId,
                        Quantity = incoming.Quantity,
                        Price = incoming.Price
                    });
                }
            }

            await _db.SaveChangesAsync();
            return Ok(cart);
        }
    }
}