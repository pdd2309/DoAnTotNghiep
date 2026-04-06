using Microsoft.AspNetCore.Mvc;
using DoAnTotNghiep.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnTotNghiep.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderApi : ControllerBase
    {
        private readonly CuaHangCongNgheDBContext _db;

        public OrderApi(CuaHangCongNgheDBContext db)
        {
            _db = db;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] OrderRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName");
            if (userId == null) return Unauthorized("Vui lòng đăng nhập.");

            var cart = await _db.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserName == userName);
            if (cart == null || !cart.CartItems.Any()) return BadRequest("Giỏ hàng trống.");

            decimal totalAmount = 0;
            foreach (var item in cart.CartItems)
            {
                totalAmount += (Convert.ToDecimal(item.Price) * Convert.ToInt32(item.Quantity));
            }

            var newOrder = new DonHang
            {
                MaNguoiDung = userId.Value,
                HoTen = request.HoTen,
                DiaChiGiaoHang = request.DiaChi,
                SoDienThoai = request.SDT,
                Email = request.Email,
                GhiChu = request.GhiChu,
                NgayDat = DateTime.Now,
                TrangThai = "Chờ xử lý",
                TongTien = totalAmount
            };
            _db.DonHangs.Add(newOrder);
            await _db.SaveChangesAsync();

            foreach (var item in cart.CartItems)
            {
                _db.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    MaDonHang = newOrder.MaDonHang,
                    MaSanPham = item.ProductId,
                    SoLuong = item.Quantity,
                    DonGiaLucMua = item.Price
                });
            }
            _db.CartItems.RemoveRange(cart.CartItems);
            await _db.SaveChangesAsync();
            return Ok(new { orderId = newOrder.MaDonHang });
        }

        // 2. LẤY CHI TIẾT ĐƠN HÀNG
        [HttpGet("GetDetails/{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var details = await _db.ChiTietDonHangs
                .Include(d => d.MaSanPhamNavigation)
                .Where(d => d.MaDonHang == id)
                .Select(d => new {
                    tenSanPham = d.MaSanPhamNavigation.TenSanPham,
                    hinhAnh = d.MaSanPhamNavigation.HinhAnh,
                    soLuong = d.SoLuong,
                    giaLucMua = d.DonGiaLucMua,
                    thanhTien = (d.SoLuong ?? 0) * (d.DonGiaLucMua ?? 0)
                })
                .ToListAsync();

            return Ok(details);
        }

        // 3. XÁC NHẬN ĐÃ NHẬN HÀNG
        [HttpPut("ConfirmReceived/{id}")]
        public async Task<IActionResult> ConfirmReceived(int id)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null) return Unauthorized();

                var order = await _db.DonHangs.FirstOrDefaultAsync(o => o.MaDonHang == id && o.MaNguoiDung == userId);
                if (order == null) return NotFound(new { success = false, message = "Không tìm thấy đơn hàng." });

                order.TrangThai = "Đã giao hàng";
                _db.DonHangs.Update(order);
                await _db.SaveChangesAsync();

                return Ok(new { success = true, message = "Xác nhận thành công!" });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }
    }

    public class OrderRequest { public string HoTen { get; set; } public string DiaChi { get; set; } public string SDT { get; set; } public string Email { get; set; } public string GhiChu { get; set; } }
}