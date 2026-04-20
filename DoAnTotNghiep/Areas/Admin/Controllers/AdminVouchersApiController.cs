using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/vouchers")]
    public class AdminVouchersApiController : ControllerBase
    {
        private readonly CuaHangCongNgheDBContext _context;

        public AdminVouchersApiController(CuaHangCongNgheDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.Vouchers
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    id = x.Id,
                    code = x.Code,
                    name = x.Name,
                    discountType = x.DiscountType,
                    discountValue = x.DiscountValue,
                    maxDiscountAmount = x.MaxDiscountAmount,
                    minOrderAmount = x.MinOrderAmount,
                    quantity = x.Quantity,
                    isActive = x.IsActive,
                    startDate = x.StartDate,
                    endDate = x.EndDate,
                    createdAt = x.CreatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _context.Vouchers
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    id = x.Id,
                    code = x.Code,
                    name = x.Name,
                    discountType = x.DiscountType,
                    discountValue = x.DiscountValue,
                    maxDiscountAmount = x.MaxDiscountAmount,
                    minOrderAmount = x.MinOrderAmount,
                    quantity = x.Quantity,
                    isActive = x.IsActive,
                    startDate = x.StartDate,
                    endDate = x.EndDate
                })
                .FirstOrDefaultAsync();

            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VoucherUpsertRequest request)
        {
            var validation = await ValidateRequestAsync(request, null);
            if (validation != null) return validation;

            var entity = new Voucher
            {
                Code = request.Code!.Trim().ToUpperInvariant(),
                Name = request.Name?.Trim(),
                DiscountType = request.DiscountType!.Trim(),
                DiscountValue = request.DiscountValue,
                MaxDiscountAmount = request.MaxDiscountAmount,
                MinOrderAmount = request.MinOrderAmount,
                Quantity = request.Quantity,
                IsActive = request.IsActive,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CreatedAt = DateTime.Now
            };

            _context.Vouchers.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(new { id = entity.Id });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] VoucherUpsertRequest request)
        {
            var entity = await _context.Vouchers.FindAsync(id);
            if (entity == null) return NotFound();

            var validation = await ValidateRequestAsync(request, id);
            if (validation != null) return validation;

            entity.Code = request.Code!.Trim().ToUpperInvariant();
            entity.Name = request.Name?.Trim();
            entity.DiscountType = request.DiscountType!.Trim();
            entity.DiscountValue = request.DiscountValue;
            entity.MaxDiscountAmount = request.MaxDiscountAmount;
            entity.MinOrderAmount = request.MinOrderAmount;
            entity.Quantity = request.Quantity;
            entity.IsActive = request.IsActive;
            entity.StartDate = request.StartDate;
            entity.EndDate = request.EndDate;

            await _context.SaveChangesAsync();
            return Ok(new { id = entity.Id });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var used = await _context.VoucherUsages.AnyAsync(x => x.VoucherId == id);
            if (used)
            {
                return BadRequest(new { message = "Voucher da duoc su dung, khong the xoa." });
            }

            var entity = await _context.Vouchers.FindAsync(id);
            if (entity == null) return NotFound();

            _context.Vouchers.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<IActionResult?> ValidateRequestAsync(VoucherUpsertRequest? request, int? currentId)
        {
            if (request == null) return BadRequest(new { message = "Du lieu khong hop le." });

            if (string.IsNullOrWhiteSpace(request.Code))
                return BadRequest(new { message = "Code la bat buoc." });

            if (string.IsNullOrWhiteSpace(request.DiscountType))
                return BadRequest(new { message = "DiscountType la bat buoc." });

            if (request.DiscountValue <= 0)
                return BadRequest(new { message = "DiscountValue phai lon hon 0." });

            if (request.Quantity < 0)
                return BadRequest(new { message = "Quantity khong duoc am." });

            var type = request.DiscountType.Trim();
            if (!string.Equals(type, "Amount", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(type, "Percent", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "DiscountType chi nhan Amount hoac Percent." });
            }

            if (string.Equals(type, "Percent", StringComparison.OrdinalIgnoreCase) && request.DiscountValue > 100)
            {
                return BadRequest(new { message = "Percent khong duoc vuot qua 100." });
            }

            if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate > request.EndDate)
            {
                return BadRequest(new { message = "StartDate phai nho hon hoac bang EndDate." });
            }

            var normalizedCode = request.Code.Trim().ToUpperInvariant();
            var exists = await _context.Vouchers.AnyAsync(x => x.Code == normalizedCode && (!currentId.HasValue || x.Id != currentId.Value));
            if (exists)
            {
                return BadRequest(new { message = "Code da ton tai." });
            }

            return null;
        }

        public class VoucherUpsertRequest
        {
            public string? Code { get; set; }
            public string? Name { get; set; }
            public string? DiscountType { get; set; }
            public decimal DiscountValue { get; set; }
            public decimal? MaxDiscountAmount { get; set; }
            public decimal? MinOrderAmount { get; set; }
            public int Quantity { get; set; }
            public bool IsActive { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
        }
    }
}
