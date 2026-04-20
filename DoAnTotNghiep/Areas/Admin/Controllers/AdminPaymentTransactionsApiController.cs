using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/payment-transactions")]
    public class AdminPaymentTransactionsApiController : ControllerBase
    {
        private readonly CuaHangCongNgheDBContext _context;

        public AdminPaymentTransactionsApiController(CuaHangCongNgheDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.PaymentTransactions
                .Join(_context.DonHangs,
                    p => p.MaDonHang,
                    o => o.MaDonHang,
                    (p, o) => new
                    {
                        id = p.Id,
                        maDonHang = p.MaDonHang,
                        provider = p.Provider,
                        transactionNo = p.TransactionNo,
                        amount = p.Amount,
                        status = p.Status,
                        responseCode = p.ResponseCode,
                        paidAt = p.PaidAt,
                        createdAt = p.CreatedAt,
                        khachHang = o.HoTen
                    })
                .OrderByDescending(x => x.createdAt)
                .ToListAsync();

            return Ok(items);
        }
    }
}
