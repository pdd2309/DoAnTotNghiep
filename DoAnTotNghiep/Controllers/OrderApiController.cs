using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DoAnTotNghiep.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderApi : ControllerBase
    {
        private const string StatusChoXuLy = "Ch\u1EDD x\u1EED l\u00FD";
        private const string StatusDaThanhToan = "\u0110\u00E3 thanh to\u00E1n";
        private const string StatusDaGiaoHang = "\u0110\u00E3 giao h\u00E0ng";

        private readonly CuaHangCongNgheDBContext _db;
        private readonly IConfiguration _config;

        public OrderApi(CuaHangCongNgheDBContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpPost("ValidateVoucher")]
        public async Task<IActionResult> ValidateVoucher([FromBody] ValidateVoucherRequest request)
        {
            if (request == null || request.SubTotal <= 0)
            {
                return BadRequest(new { message = "Invalid subtotal." });
            }

            var result = await CalculateVoucherDiscountAsync(request.VoucherCode, request.SubTotal);
            if (!result.IsValid)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new
            {
                voucherCode = result.VoucherCode,
                discountAmount = result.DiscountAmount,
                finalTotal = result.FinalTotal,
                message = result.Message
            });
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] OrderRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName");
            if (userId == null) return Unauthorized("Please login.");

            var cart = await FindCartAsync(userId.Value, userName);
            if (cart == null || !cart.CartItems.Any()) return BadRequest("Cart is empty.");

            var subTotal = cart.CartItems.Sum(x => x.Price * x.Quantity);
            var voucherResult = await CalculateVoucherDiscountAsync(request.VoucherCode, subTotal);
            if (!voucherResult.IsValid)
            {
                return BadRequest(voucherResult.Message);
            }

            var newOrder = await CreateOrderFromCartAsync(request, userId.Value, cart, StatusChoXuLy, voucherResult);

            await AddPaymentTransactionAsync(
                orderId: newOrder.MaDonHang,
                provider: "COD",
                transactionNo: null,
                amount: newOrder.TongTien ?? 0,
                status: "Pending",
                responseCode: null,
                paidAt: null);

            return Ok(new { orderId = newOrder.MaDonHang });
        }

        [HttpPost("CreateVnPayPayment")]
        public async Task<IActionResult> CreateVnPayPayment([FromBody] OrderRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName");
            if (userId == null) return Unauthorized("Please login.");

            if (string.IsNullOrWhiteSpace(request.HoTen) || string.IsNullOrWhiteSpace(request.DiaChi) || string.IsNullOrWhiteSpace(request.SDT))
            {
                return BadRequest("Invalid checkout data.");
            }

            var cart = await FindCartAsync(userId.Value, userName);
            if (cart == null || !cart.CartItems.Any()) return BadRequest("Cart is empty.");

            var subTotal = cart.CartItems.Sum(x => x.Price * x.Quantity);
            var voucherResult = await CalculateVoucherDiscountAsync(request.VoucherCode, subTotal);
            if (!voucherResult.IsValid)
            {
                return BadRequest(voucherResult.Message);
            }

            HttpContext.Session.SetString("PendingOrderInfo", JsonSerializer.Serialize(request));

            var paymentUrl = BuildVnPayPaymentUrl(voucherResult.FinalTotal, userId.Value);
            return Ok(new { paymentUrl });
        }

        [HttpGet("VnPayReturn")]
        public async Task<IActionResult> VnPayReturn()
        {
            var isValidSignature = ValidateVnPaySignature(Request.Query);
            var responseCode = Request.Query["vnp_ResponseCode"].ToString();
            var transactionStatus = Request.Query["vnp_TransactionStatus"].ToString();

            if (!isValidSignature || responseCode != "00" || transactionStatus != "00")
            {
                return Redirect("/Home/Checkout?paymentStatus=failed");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName");
            if (userId == null)
            {
                return Redirect("/Account/Login");
            }

            var pendingRaw = HttpContext.Session.GetString("PendingOrderInfo");
            if (string.IsNullOrWhiteSpace(pendingRaw))
            {
                return Redirect("/Home/Checkout?paymentStatus=failed");
            }

            var request = JsonSerializer.Deserialize<OrderRequest>(pendingRaw);
            if (request == null)
            {
                return Redirect("/Home/Checkout?paymentStatus=failed");
            }

            var cart = await FindCartAsync(userId.Value, userName);
            if (cart == null || !cart.CartItems.Any())
            {
                return Redirect("/Home/Checkout?paymentStatus=failed");
            }

            var subTotal = cart.CartItems.Sum(x => x.Price * x.Quantity);
            var voucherResult = await CalculateVoucherDiscountAsync(request.VoucherCode, subTotal);
            if (!voucherResult.IsValid)
            {
                return Redirect("/Home/Checkout?paymentStatus=failed");
            }

            var order = await CreateOrderFromCartAsync(request, userId.Value, cart, StatusDaThanhToan, voucherResult);

            var transactionNo = Request.Query["vnp_TransactionNo"].ToString();
            var amountRaw = Request.Query["vnp_Amount"].ToString();
            var paidAtRaw = Request.Query["vnp_PayDate"].ToString();
            var amount = ParseVnPayAmount(amountRaw, order.TongTien ?? 0);
            var paidAt = ParseVnPayDate(paidAtRaw);

            await AddPaymentTransactionAsync(
                orderId: order.MaDonHang,
                provider: "VNPAY",
                transactionNo: transactionNo,
                amount: amount,
                status: "Success",
                responseCode: responseCode,
                paidAt: paidAt);

            HttpContext.Session.Remove("PendingOrderInfo");

            return Redirect($"/Home/Checkout?paymentStatus=success&orderId={order.MaDonHang}");
        }

        [HttpGet("GetDetails/{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var details = await _db.ChiTietDonHangs
                .Include(d => d.MaSanPhamNavigation)
                .Where(d => d.MaDonHang == id)
                .Select(d => new
                {
                    tenSanPham = d.MaSanPhamNavigation.TenSanPham,
                    hinhAnh = d.MaSanPhamNavigation.HinhAnh,
                    soLuong = d.SoLuong,
                    giaLucMua = d.DonGiaLucMua,
                    thanhTien = (d.SoLuong ?? 0) * (d.DonGiaLucMua ?? 0)
                })
                .ToListAsync();

            return Ok(details);
        }

        [HttpPut("ConfirmReceived/{id}")]
        public async Task<IActionResult> ConfirmReceived(int id)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null) return Unauthorized();

                var order = await _db.DonHangs.FirstOrDefaultAsync(o => o.MaDonHang == id && o.MaNguoiDung == userId);
                if (order == null) return NotFound(new { success = false, message = "Order not found." });

                order.TrangThai = StatusDaGiaoHang;
                _db.DonHangs.Update(order);
                await _db.SaveChangesAsync();

                await AddOrderStatusHistoryAsync(order.MaDonHang, StatusDaGiaoHang, userId, "User confirmed order received");

                return Ok(new { success = true, message = "Updated." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        private async Task<Cart?> FindCartAsync(int userId, string? userName)
        {
            var legacyKey = $"uid:{userId}";
            return await _db.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c =>
                    (!string.IsNullOrEmpty(userName) && c.UserName == userName) ||
                    c.UserName == legacyKey);
        }

        private async Task<DonHang> CreateOrderFromCartAsync(OrderRequest request, int userId, Cart cart, string status, VoucherApplyResult voucherResult)
        {
            var note = request.GhiChu ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(voucherResult.VoucherCode) && voucherResult.DiscountAmount > 0)
            {
                note = string.IsNullOrWhiteSpace(note)
                    ? $"Voucher: {voucherResult.VoucherCode}"
                    : $"{note} | Voucher: {voucherResult.VoucherCode}";
            }

            var newOrder = new DonHang
            {
                MaNguoiDung = userId,
                HoTen = request.HoTen,
                DiaChiGiaoHang = request.DiaChi,
                SoDienThoai = request.SDT,
                Email = request.Email,
                GhiChu = note,
                NgayDat = DateTime.Now,
                TrangThai = status,
                TongTien = voucherResult.FinalTotal
            };

            _db.DonHangs.Add(newOrder);
            await _db.SaveChangesAsync();

            await AddOrderStatusHistoryAsync(newOrder.MaDonHang, status, userId, "\u0110\u01A1n h\u00E0ng \u0111\u01B0\u1EE3c t\u1EA1o");

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

            if (voucherResult.VoucherEntity != null)
            {
                voucherResult.VoucherEntity.Quantity = Math.Max(0, voucherResult.VoucherEntity.Quantity - 1);
                _db.Vouchers.Update(voucherResult.VoucherEntity);

                _db.VoucherUsages.Add(new VoucherUsage
                {
                    VoucherId = voucherResult.VoucherEntity.Id,
                    MaNguoiDung = userId,
                    MaDonHang = newOrder.MaDonHang,
                    VoucherCode = voucherResult.VoucherCode ?? voucherResult.VoucherEntity.Code,
                    DiscountAmount = voucherResult.DiscountAmount,
                    UsedAt = DateTime.Now
                });
            }

            await _db.SaveChangesAsync();
            return newOrder;
        }

        private async Task AddOrderStatusHistoryAsync(int orderId, string status, int? changedByUserId, string? note)
        {
            _db.OrderStatusHistories.Add(new OrderStatusHistory
            {
                MaDonHang = orderId,
                Status = status,
                ChangedByUserId = changedByUserId,
                Note = note,
                ChangedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();
        }

        private async Task AddPaymentTransactionAsync(int orderId, string provider, string? transactionNo, decimal amount, string status, string? responseCode, DateTime? paidAt)
        {
            _db.PaymentTransactions.Add(new PaymentTransaction
            {
                MaDonHang = orderId,
                Provider = provider,
                TransactionNo = transactionNo,
                Amount = amount,
                Status = status,
                ResponseCode = responseCode,
                PaidAt = paidAt,
                CreatedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();
        }

        private static decimal ParseVnPayAmount(string? amountRaw, decimal fallback)
        {
            if (string.IsNullOrWhiteSpace(amountRaw)) return fallback;
            if (!decimal.TryParse(amountRaw, out var raw)) return fallback;
            if (raw <= 0) return fallback;
            return raw / 100m;
        }

        private static DateTime? ParseVnPayDate(string? payDateRaw)
        {
            if (string.IsNullOrWhiteSpace(payDateRaw)) return null;
            if (DateTime.TryParseExact(payDateRaw, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                return dt;
            }
            return null;
        }

        private async Task<VoucherApplyResult> CalculateVoucherDiscountAsync(string? rawCode, decimal subTotal)
        {
            if (subTotal <= 0)
            {
                return VoucherApplyResult.Invalid("Invalid subtotal.");
            }

            if (string.IsNullOrWhiteSpace(rawCode))
            {
                return VoucherApplyResult.NoVoucher(subTotal);
            }

            var code = rawCode.Trim().ToUpperInvariant();
            var now = DateTime.Now;

            var voucher = await _db.Vouchers
                .FirstOrDefaultAsync(v => v.Code == code);

            if (voucher == null)
            {
                return VoucherApplyResult.Invalid("Voucher does not exist.");
            }

            if (!voucher.IsActive)
            {
                return VoucherApplyResult.Invalid("Voucher is inactive.");
            }

            if (voucher.Quantity <= 0)
            {
                return VoucherApplyResult.Invalid("Voucher is out of stock.");
            }

            if (voucher.StartDate.HasValue && now < voucher.StartDate.Value)
            {
                return VoucherApplyResult.Invalid("Voucher is not active yet.");
            }

            if (voucher.EndDate.HasValue && now > voucher.EndDate.Value)
            {
                return VoucherApplyResult.Invalid("Voucher has expired.");
            }

            if (voucher.MinOrderAmount.HasValue && subTotal < voucher.MinOrderAmount.Value)
            {
                return VoucherApplyResult.Invalid($"Order must be at least {voucher.MinOrderAmount.Value:N0} VND.");
            }

            decimal discount = 0;
            if (string.Equals(voucher.DiscountType, "Percent", StringComparison.OrdinalIgnoreCase))
            {
                discount = Math.Round(subTotal * voucher.DiscountValue / 100m, 0);
            }
            else
            {
                discount = voucher.DiscountValue;
            }

            if (voucher.MaxDiscountAmount.HasValue && discount > voucher.MaxDiscountAmount.Value)
            {
                discount = voucher.MaxDiscountAmount.Value;
            }

            if (discount < 0) discount = 0;
            if (discount > subTotal) discount = subTotal;

            var finalTotal = subTotal - discount;

            return VoucherApplyResult.Valid(voucher, code, discount, finalTotal);
        }

        private string BuildVnPayPaymentUrl(decimal amount, int userId)
        {
            var section = _config.GetSection("VnPay");
            var tmnCode = (section["TmnCode"] ?? string.Empty).Trim();
            var hashSecret = (section["HashSecret"] ?? string.Empty).Trim();
            var payUrl = (section["PayUrl"] ?? string.Empty).Trim();
            var returnUrl = (section["ReturnUrl"] ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(tmnCode) || string.IsNullOrWhiteSpace(hashSecret) || string.IsNullOrWhiteSpace(payUrl) || string.IsNullOrWhiteSpace(returnUrl))
            {
                throw new InvalidOperationException("Missing VnPay config.");
            }

            var txnRef = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var createDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            var amountValue = ((long)Math.Round(amount * 100, 0)).ToString(CultureInfo.InvariantCulture);

            var data = new SortedDictionary<string, string>
            {
                ["vnp_Version"] = "2.1.0",
                ["vnp_Command"] = "pay",
                ["vnp_TmnCode"] = tmnCode,
                ["vnp_Amount"] = amountValue,
                ["vnp_CreateDate"] = createDate,
                ["vnp_CurrCode"] = "VND",
                ["vnp_IpAddr"] = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                ["vnp_Locale"] = "vn",
                ["vnp_OrderInfo"] = $"Thanh toan don hang user {userId}",
                ["vnp_OrderType"] = "other",
                ["vnp_ReturnUrl"] = returnUrl,
                ["vnp_TxnRef"] = txnRef
            };

            var query = BuildQueryString(data);
            var secureHash = ComputeHmacSha512(hashSecret, query);
            return $"{payUrl}?{query}&vnp_SecureHash={secureHash}";
        }

        private bool ValidateVnPaySignature(IQueryCollection queryCollection)
        {
            var section = _config.GetSection("VnPay");
            var hashSecret = (section["HashSecret"] ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(hashSecret)) return false;

            var secureHash = queryCollection["vnp_SecureHash"].ToString();
            if (string.IsNullOrWhiteSpace(secureHash)) return false;

            var data = new SortedDictionary<string, string>();
            foreach (var key in queryCollection.Keys)
            {
                if (!key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase)) continue;
                if (key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase)) continue;
                if (key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase)) continue;
                data[key] = queryCollection[key].ToString();
            }

            var signData = BuildQueryString(data);
            var expectedHash = ComputeHmacSha512(hashSecret, signData);
            return secureHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildQueryString(SortedDictionary<string, string> data)
        {
            return string.Join("&", data
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Select(x => $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}"));
        }

        private static string ComputeHmacSha512(string key, string inputData)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using var hmac = new HMACSHA512(keyBytes);
            var hashBytes = hmac.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        public class ValidateVoucherRequest
        {
            public string? VoucherCode { get; set; }
            public decimal SubTotal { get; set; }
        }

        private sealed class VoucherApplyResult
        {
            public bool IsValid { get; private set; }
            public string Message { get; private set; } = string.Empty;
            public string? VoucherCode { get; private set; }
            public decimal DiscountAmount { get; private set; }
            public decimal FinalTotal { get; private set; }
            public Voucher? VoucherEntity { get; private set; }

            public static VoucherApplyResult NoVoucher(decimal subtotal) => new()
            {
                IsValid = true,
                DiscountAmount = 0,
                FinalTotal = subtotal,
                Message = "No voucher."
            };

            public static VoucherApplyResult Valid(Voucher voucher, string code, decimal discount, decimal finalTotal) => new()
            {
                IsValid = true,
                VoucherEntity = voucher,
                VoucherCode = code,
                DiscountAmount = discount,
                FinalTotal = finalTotal,
                Message = "Voucher applied."
            };

            public static VoucherApplyResult Invalid(string message) => new()
            {
                IsValid = false,
                Message = message,
                DiscountAmount = 0,
                FinalTotal = 0
            };
        }
    }

    public class OrderRequest
    {
        public string HoTen { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public string SDT { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string GhiChu { get; set; } = string.Empty;
        public string? VoucherCode { get; set; }
    }
}
