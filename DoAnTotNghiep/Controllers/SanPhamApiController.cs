using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnTotNghiep.Models;
using DoAnTotNghiep.Models.DTOs;

namespace DoAnTotNghiep.Controllers
{
    [Route("api/[controller]")] // Route này sẽ là /api/SanPhamApi
    [ApiController]
    public class SanPhamApiController : ControllerBase
    {
        private readonly CuaHangCongNgheDBContext _context;
        private readonly ILogger<SanPhamApiController> _logger;

        public SanPhamApiController(CuaHangCongNgheDBContext context, ILogger<SanPhamApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============ GET METHODS ============

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SanPham>>> GetAllSanPhams()
        {
            try
            {
                var sanPhams = await _context.SanPhams
                    .Include(sp => sp.MaDanhMucNavigation)
                    .ToListAsync();

                return Ok(new { success = true, data = sanPhams, count = sanPhams.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách sản phẩm");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        [HttpGet("top-selling")]
        public async Task<IActionResult> GetTopSelling([FromQuery] int take = 8)
        {
            if (take <= 0) take = 8;

            try
            {
                var topSales = await _context.ChiTietDonHangs
                    .Where(ct => ct.MaSanPham != null)
                    .GroupBy(ct => ct.MaSanPham)
                    .Select(g => new
                    {
                        MaSanPham = g.Key.Value,
                        SoLuongBan = g.Sum(x => x.SoLuong ?? 0)
                    })
                    .OrderByDescending(x => x.SoLuongBan)
                    .Take(take)
                    .ToListAsync();

                if (topSales.Count == 0)
                {
                    var fallback = await _context.SanPhams
                        .OrderByDescending(sp => sp.MaSanPham)
                        .Take(take)
                        .ToListAsync();

                    return Ok(new { success = true, data = fallback, count = fallback.Count });
                }

                var topIds = topSales.Select(x => x.MaSanPham).ToList();
                var products = await _context.SanPhams
                    .Where(sp => topIds.Contains(sp.MaSanPham))
                    .ToListAsync();

                var ordered = topSales
                    .Join(products, s => s.MaSanPham, p => p.MaSanPham, (s, p) => new { s, p })
                    .OrderByDescending(x => x.s.SoLuongBan)
                    .Select(x => x.p)
                    .ToList();

                return Ok(new { success = true, data = ordered, count = ordered.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy top sản phẩm bán chạy");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SanPham>> GetSanPhamById(int id)
        {
            try
            {
                var sanPham = await _context.SanPhams
                    .Include(sp => sp.MaDanhMucNavigation)
                    .FirstOrDefaultAsync(sp => sp.MaSanPham == id);

                if (sanPham == null)
                {
                    return NotFound(new { success = false, message = $"Không tìm thấy ID: {id}" });
                }

                return Ok(new { success = true, data = sanPham });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        // ============ CHỨC NĂNG ĐÁNH GIÁ (REVIEW) ============

        [HttpGet("{id}/reviews")]
        public async Task<IActionResult> GetReviews(int id)
        {
            try
            {
                var reviews = await _context.DanhGias
                    .Where(d => d.MaSanPham == id)
                    .Include(d => d.MaNguoiDungNavigation)
                    .OrderByDescending(d => d.NgayDanhGia)
                    .Select(d => new
                    {
                        d.MaDanhGia,
                        tenNguoiDung = d.MaNguoiDungNavigation != null ? d.MaNguoiDungNavigation.HoTen : "Khách hàng",
                        d.NoiDung,
                        soSao = d.SoSao ?? 5,
                        ngay = d.NgayDanhGia.HasValue ? d.NgayDanhGia.Value.ToString("dd/MM/yyyy HH:mm") : ""
                    })
                    .ToListAsync();

                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi tải đánh giá", error = ex.Message });
            }
        }

        [HttpPost("reviews")]
        public async Task<IActionResult> PostReview([FromBody] PostReviewDto dto)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return BadRequest(new { success = false, message = "Vui lòng đăng nhập để đánh giá!" });
                }

                var review = new DanhGia
                {
                    MaSanPham = dto.MaSanPham,
                    MaNguoiDung = userId,
                    NoiDung = dto.NoiDung,
                    SoSao = dto.SoSao,
                    NgayDanhGia = DateTime.Now
                };

                _context.DanhGias.Add(review);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Cảm ơn bạn đã gửi đánh giá!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ============ CÁC METHOD KHÁC (SEARCH, DELETE...) ============
        [HttpGet("search/{keyword}")]
        public async Task<ActionResult<IEnumerable<SanPham>>> SearchSanPham(string keyword)
        {
            var sanPhams = await _context.SanPhams.Where(sp => sp.TenSanPham.Contains(keyword)).ToListAsync();
            return Ok(new { success = true, data = sanPhams });
        }
    }

    public class PostReviewDto
    {
        public int MaSanPham { get; set; }
        public string NoiDung { get; set; } = "";
        public int SoSao { get; set; } = 5;
    }
}