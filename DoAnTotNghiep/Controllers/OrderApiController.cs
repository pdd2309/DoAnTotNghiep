using Microsoft.AspNetCore.Mvc;
using DoAnTotNghiep.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

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
            // 1. Kiểm tra đăng nhập qua Session
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName");

            if (userId == null) return Unauthorized("Vui lòng đăng nhập để thực hiện đặt hàng.");

            // 2. Lấy giỏ hàng của người dùng hiện tại
            var cart = await _db.Carts.Include(c => c.CartItems)
                                      .FirstOrDefaultAsync(c => c.UserName == userName);

            if (cart == null || !cart.CartItems.Any()) return BadRequest("Giỏ hàng của bạn đang trống.");

            // Tính tổng tiền bằng vòng lặp để kiểm soát hoàn toàn việc ép kiểu
            decimal totalAmount = 0;
            foreach (var item in cart.CartItems)
            {
                decimal price = Convert.ToDecimal(item.Price);
                int qty = Convert.ToInt32(item.Quantity);
                totalAmount += (price * qty);
            }

            // 3. Khởi tạo Đơn hàng mới
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
                TongTien = totalAmount // Gán biến đã tính ở trên vào
            };
            _db.DonHangs.Add(newOrder);
            await _db.SaveChangesAsync(); // Lưu để lấy ID tự tăng (MaDonHang)

            // 4. Lưu chi tiết đơn hàng (Khớp 100% với Model ChiTietDonHang của ông)
            foreach (var item in cart.CartItems)
            {
                var orderDetail = new ChiTietDonHang
                {
                    MaDonHang = newOrder.MaDonHang,
                    MaSanPham = item.ProductId,
                    SoLuong = item.Quantity,
                    DonGiaLucMua = item.Price // Khớp với bảng ChiTietDonHang
                };
                _db.ChiTietDonHangs.Add(orderDetail);
            }

            // 5. Sau khi đặt hàng thành công, xóa các sản phẩm trong giỏ hàng
            _db.CartItems.RemoveRange(cart.CartItems);
            await _db.SaveChangesAsync();

            // Trả về mã đơn hàng để JS hiển thị thông báo
            return Ok(new { orderId = newOrder.MaDonHang });
        }
    }

    // Class đại diện cho dữ liệu JSON gửi lên từ JavaScript
    public class OrderRequest
    {
        public string HoTen { get; set; }
        public string DiaChi { get; set; }
        public string SDT { get; set; }
        public string Email { get; set; }
        public string GhiChu { get; set; }
    }
}