using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/products")]
    public class AdminSanPhamsApiController : ControllerBase
    {
        private readonly CuaHangCongNgheDBContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminSanPhamsApiController(CuaHangCongNgheDBContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.SanPhams
                .Include(s => s.MaDanhMucNavigation)
                .OrderByDescending(s => s.MaSanPham)
                .Select(s => new
                {
                    maSanPham = s.MaSanPham,
                    tenSanPham = s.TenSanPham,
                    giaTien = s.GiaTien,
                    moTa = s.MoTa,
                    hinhAnh = s.HinhAnh,
                    maDanhMuc = s.MaDanhMuc,
                    tenDanhMuc = s.MaDanhMucNavigation != null ? s.MaDanhMucNavigation.TenDanhMuc : null
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _context.SanPhams
                .Where(s => s.MaSanPham == id)
                .Select(s => new
                {
                    maSanPham = s.MaSanPham,
                    tenSanPham = s.TenSanPham,
                    giaTien = s.GiaTien,
                    moTa = s.MoTa,
                    hinhAnh = s.HinhAnh,
                    maDanhMuc = s.MaDanhMuc
                })
                .FirstOrDefaultAsync();

            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductUpsertRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TenSanPham) || request.MaDanhMuc <= 0)
            {
                return BadRequest(new { message = "Invalid product payload." });
            }

            var imagePath = await SaveImageAsync(request.ImageFile);

            var entity = new SanPham
            {
                TenSanPham = request.TenSanPham.Trim(),
                GiaTien = request.GiaTien,
                MoTa = request.MoTa,
                MaDanhMuc = request.MaDanhMuc,
                HinhAnh = imagePath
            };

            _context.SanPhams.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = entity.MaSanPham }, new { maSanPham = entity.MaSanPham });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromForm] ProductUpsertRequest request)
        {
            var entity = await _context.SanPhams.FindAsync(id);
            if (entity == null) return NotFound();

            if (string.IsNullOrWhiteSpace(request.TenSanPham) || request.MaDanhMuc <= 0)
            {
                return BadRequest(new { message = "Invalid product payload." });
            }

            entity.TenSanPham = request.TenSanPham.Trim();
            entity.GiaTien = request.GiaTien;
            entity.MoTa = request.MoTa;
            entity.MaDanhMuc = request.MaDanhMuc;

            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                DeleteImage(entity.HinhAnh);
                entity.HinhAnh = await SaveImageAsync(request.ImageFile);
            }

            await _context.SaveChangesAsync();
            return Ok(new { maSanPham = entity.MaSanPham });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.SanPhams.FindAsync(id);
            if (entity == null) return NotFound();

            DeleteImage(entity.HinhAnh);
            _context.SanPhams.Remove(entity);
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

            var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(uploadFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(stream);

            return $"/uploads/products/{fileName}";
        }

        private void DeleteImage(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return;
            }

            var relativePath = imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        public class ProductUpsertRequest
        {
            public string TenSanPham { get; set; } = string.Empty;
            public decimal GiaTien { get; set; }
            public string? MoTa { get; set; }
            public int MaDanhMuc { get; set; }
            public IFormFile? ImageFile { get; set; }
        }
    }
}
