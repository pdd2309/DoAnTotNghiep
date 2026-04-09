using DoAnTotNghiep.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/banners")]
    public class AdminBannersApiController : ControllerBase
    {
        private readonly CuaHangCongNgheDBContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminBannersApiController(CuaHangCongNgheDBContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.Banners
                .OrderBy(x => x.Position)
                .ThenBy(x => x.DisplayOrder)
                .ThenByDescending(x => x.Id)
                .Select(x => new
                {
                    id = x.Id,
                    title = x.Title,
                    subTitle = x.SubTitle,
                    description = x.Description,
                    imageUrl = x.ImageUrl,
                    linkUrl = x.LinkUrl,
                    position = x.Position,
                    displayOrder = x.DisplayOrder,
                    isActive = x.IsActive,
                    createdAt = x.CreatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _context.Banners.FindAsync(id);
            if (item == null) return NotFound();

            return Ok(new
            {
                id = item.Id,
                title = item.Title,
                subTitle = item.SubTitle,
                description = item.Description,
                imageUrl = item.ImageUrl,
                linkUrl = item.LinkUrl,
                position = item.Position,
                displayOrder = item.DisplayOrder,
                isActive = item.IsActive
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] BannerUpsertRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Position) || request.ImageFile == null)
            {
                return BadRequest(new { message = "Invalid banner payload." });
            }

            var imageUrl = await SaveImageAsync(request.ImageFile);
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return BadRequest(new { message = "Invalid image file." });
            }

            var entity = new Banner
            {
                Title = request.Title?.Trim(),
                SubTitle = request.SubTitle?.Trim(),
                Description = request.Description?.Trim(),
                LinkUrl = request.LinkUrl?.Trim(),
                Position = NormalizePosition(request.Position),
                DisplayOrder = request.DisplayOrder,
                IsActive = request.IsActive,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.Now
            };

            _context.Banners.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(new { id = entity.Id });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromForm] BannerUpsertRequest request)
        {
            var entity = await _context.Banners.FindAsync(id);
            if (entity == null) return NotFound();

            if (request == null || string.IsNullOrWhiteSpace(request.Position))
            {
                return BadRequest(new { message = "Invalid banner payload." });
            }

            entity.Title = request.Title?.Trim();
            entity.SubTitle = request.SubTitle?.Trim();
            entity.Description = request.Description?.Trim();
            entity.LinkUrl = request.LinkUrl?.Trim();
            entity.Position = NormalizePosition(request.Position);
            entity.DisplayOrder = request.DisplayOrder;
            entity.IsActive = request.IsActive;

            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                DeleteImage(entity.ImageUrl);
                entity.ImageUrl = await SaveImageAsync(request.ImageFile);
            }

            await _context.SaveChangesAsync();
            return Ok(new { id = entity.Id });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Banners.FindAsync(id);
            if (entity == null) return NotFound();

            DeleteImage(entity.ImageUrl);
            _context.Banners.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static string NormalizePosition(string? input)
        {
            if (string.Equals(input, "Bottom", StringComparison.OrdinalIgnoreCase)) return "Bottom";
            return "Hero";
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

            var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "banners");
            Directory.CreateDirectory(uploadFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(stream);

            return $"/uploads/banners/{fileName}";
        }

        private void DeleteImage(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath)) return;

            var relativePath = imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_environment.WebRootPath, relativePath);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        public class BannerUpsertRequest
        {
            public string? Title { get; set; }
            public string? SubTitle { get; set; }
            public string? Description { get; set; }
            public string? LinkUrl { get; set; }
            public string Position { get; set; } = "Hero";
            public int DisplayOrder { get; set; }
            public bool IsActive { get; set; } = true;
            public IFormFile? ImageFile { get; set; }
        }
    }
}
