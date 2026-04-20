using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnTotNghiep.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressBookApiController : ControllerBase
    {
        private readonly CuaHangCongNgheDBContext _db;

        public AddressBookApiController(CuaHangCongNgheDBContext db)
        {
            _db = db;
        }

        [HttpGet("My")]
        public async Task<IActionResult> GetMyAddresses()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var items = await _db.AddressBooks
                .Where(x => x.MaNguoiDung == userId.Value)
                .OrderByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    id = x.Id,
                    fullName = x.FullName,
                    phone = x.Phone,
                    addressLine = x.AddressLine,
                    ward = x.Ward,
                    district = x.District,
                    province = x.Province,
                    isDefault = x.IsDefault,
                    createdAt = x.CreatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddressUpsertRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var validationError = ValidateRequest(request);
            if (validationError != null) return BadRequest(new { message = validationError });

            if (request.IsDefault)
            {
                await ClearDefaultAddressAsync(userId.Value);
            }

            var entity = new AddressBook
            {
                MaNguoiDung = userId.Value,
                FullName = request.FullName!.Trim(),
                Phone = request.Phone!.Trim(),
                AddressLine = request.AddressLine!.Trim(),
                Ward = request.Ward?.Trim(),
                District = request.District?.Trim(),
                Province = request.Province?.Trim(),
                IsDefault = request.IsDefault,
                CreatedAt = DateTime.Now
            };

            _db.AddressBooks.Add(entity);
            await _db.SaveChangesAsync();

            return Ok(new { id = entity.Id });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] AddressUpsertRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var validationError = ValidateRequest(request);
            if (validationError != null) return BadRequest(new { message = validationError });

            var entity = await _db.AddressBooks.FirstOrDefaultAsync(x => x.Id == id && x.MaNguoiDung == userId.Value);
            if (entity == null) return NotFound();

            if (request.IsDefault)
            {
                await ClearDefaultAddressAsync(userId.Value, id);
            }

            entity.FullName = request.FullName!.Trim();
            entity.Phone = request.Phone!.Trim();
            entity.AddressLine = request.AddressLine!.Trim();
            entity.Ward = request.Ward?.Trim();
            entity.District = request.District?.Trim();
            entity.Province = request.Province?.Trim();
            entity.IsDefault = request.IsDefault;

            await _db.SaveChangesAsync();
            return Ok(new { id = entity.Id });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var entity = await _db.AddressBooks.FirstOrDefaultAsync(x => x.Id == id && x.MaNguoiDung == userId.Value);
            if (entity == null) return NotFound();

            _db.AddressBooks.Remove(entity);
            await _db.SaveChangesAsync();

            if (entity.IsDefault)
            {
                var fallback = await _db.AddressBooks
                    .Where(x => x.MaNguoiDung == userId.Value)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

                if (fallback != null)
                {
                    fallback.IsDefault = true;
                    await _db.SaveChangesAsync();
                }
            }

            return NoContent();
        }

        [HttpPost("{id:int}/set-default")]
        public async Task<IActionResult> SetDefault(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var entity = await _db.AddressBooks.FirstOrDefaultAsync(x => x.Id == id && x.MaNguoiDung == userId.Value);
            if (entity == null) return NotFound();

            await ClearDefaultAddressAsync(userId.Value, id);
            entity.IsDefault = true;
            await _db.SaveChangesAsync();

            return Ok(new { id = entity.Id });
        }

        private async Task ClearDefaultAddressAsync(int userId, int? exceptId = null)
        {
            var defaults = await _db.AddressBooks
                .Where(x => x.MaNguoiDung == userId && x.IsDefault && (!exceptId.HasValue || x.Id != exceptId.Value))
                .ToListAsync();

            foreach (var item in defaults)
            {
                item.IsDefault = false;
            }

            if (defaults.Count > 0)
            {
                await _db.SaveChangesAsync();
            }
        }

        private static string? ValidateRequest(AddressUpsertRequest? request)
        {
            if (request == null) return "Du lieu khong hop le.";
            if (string.IsNullOrWhiteSpace(request.FullName)) return "Ho ten la bat buoc.";
            if (string.IsNullOrWhiteSpace(request.Phone)) return "So dien thoai la bat buoc.";
            if (string.IsNullOrWhiteSpace(request.AddressLine)) return "Dia chi la bat buoc.";
            return null;
        }

        public class AddressUpsertRequest
        {
            public string? FullName { get; set; }
            public string? Phone { get; set; }
            public string? AddressLine { get; set; }
            public string? Ward { get; set; }
            public string? District { get; set; }
            public string? Province { get; set; }
            public bool IsDefault { get; set; }
        }
    }
}
