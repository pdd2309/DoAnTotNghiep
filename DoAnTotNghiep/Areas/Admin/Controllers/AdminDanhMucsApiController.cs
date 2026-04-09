using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/categories")]
    public class AdminDanhMucsApiController : ControllerBase
    {
        private readonly CuaHangCongNgheDBContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminDanhMucsApiController(CuaHangCongNgheDBContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.DanhMucs
                .OrderByDescending(x => x.MaDanhMuc)
                .Select(x => new
                {
                    maDanhMuc = x.MaDanhMuc,
                    tenDanhMuc = x.TenDanhMuc,
                    hinhAnh = x.HinhAnh,
                    isHienThiTrangChu = x.IsHienThiTrangChu,
                    thuTuHienThi = x.ThuTuHienThi
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _context.DanhMucs
                .Where(x => x.MaDanhMuc == id)
                .Select(x => new
                {
                    maDanhMuc = x.MaDanhMuc,
                    tenDanhMuc = x.TenDanhMuc,
                    hinhAnh = x.HinhAnh,
                    isHienThiTrangChu = x.IsHienThiTrangChu,
                    thuTuHienThi = x.ThuTuHienThi
                })
                .FirstOrDefaultAsync();

            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CategoryUpsertRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.TenDanhMuc))
            {
                return BadRequest(new { message = "Category name is required." });
            }

            var imagePath = request.HinhAnh?.Trim();
            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                imagePath = await SaveImageAsync(request.ImageFile);
                if (string.IsNullOrWhiteSpace(imagePath))
                {
                    return BadRequest(new { message = "Invalid image file." });
                }
            }

            var entity = new DanhMuc
            {
                TenDanhMuc = request.TenDanhMuc.Trim(),
                HinhAnh = imagePath,
                IsHienThiTrangChu = request.IsHienThiTrangChu,
                ThuTuHienThi = request.ThuTuHienThi
            };

            _context.DanhMucs.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = entity.MaDanhMuc }, new
            {
                maDanhMuc = entity.MaDanhMuc,
                tenDanhMuc = entity.TenDanhMuc,
                hinhAnh = entity.HinhAnh,
                isHienThiTrangChu = entity.IsHienThiTrangChu,
                thuTuHienThi = entity.ThuTuHienThi
            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromForm] CategoryUpsertRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.TenDanhMuc))
            {
                return BadRequest(new { message = "Category name is required." });
            }

            var entity = await _context.DanhMucs.FindAsync(id);
            if (entity == null) return NotFound();

            entity.TenDanhMuc = request.TenDanhMuc.Trim();
            entity.IsHienThiTrangChu = request.IsHienThiTrangChu;
            entity.ThuTuHienThi = request.ThuTuHienThi;

            if (!string.IsNullOrWhiteSpace(request.HinhAnh))
            {
                entity.HinhAnh = request.HinhAnh.Trim();
            }

            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                DeleteImage(entity.HinhAnh);
                var imagePath = await SaveImageAsync(request.ImageFile);
                if (string.IsNullOrWhiteSpace(imagePath))
                {
                    return BadRequest(new { message = "Invalid image file." });
                }

                entity.HinhAnh = imagePath;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                maDanhMuc = entity.MaDanhMuc,
                tenDanhMuc = entity.TenDanhMuc,
                hinhAnh = entity.HinhAnh,
                isHienThiTrangChu = entity.IsHienThiTrangChu,
                thuTuHienThi = entity.ThuTuHienThi
            });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.DanhMucs.FindAsync(id);
            if (entity == null) return NotFound();

            DeleteImage(entity.HinhAnh);
            _context.DanhMucs.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<string?> SaveImageAsync(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return null;
            }

            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            if (!allowedExtensions.Contains(extension))
            {
                return null;
            }

            var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "categories");
            Directory.CreateDirectory(uploadFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(stream);

            return $"/uploads/categories/{fileName}";
        }

        private void DeleteImage(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath)) return;
            if (!imagePath.StartsWith("/uploads/categories/", StringComparison.OrdinalIgnoreCase)) return;

            var relativePath = imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_environment.WebRootPath, relativePath);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        public class CategoryUpsertRequest
        {
            public string TenDanhMuc { get; set; } = string.Empty;
            public string? HinhAnh { get; set; }
            public IFormFile? ImageFile { get; set; }
            public bool IsHienThiTrangChu { get; set; } = true;
            public int ThuTuHienThi { get; set; } = 0;
        }
    }
}
