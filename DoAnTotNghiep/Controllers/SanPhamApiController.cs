using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnTotNghiep.Models;
using DoAnTotNghiep.Models.DTOs;

namespace DoAnTotNghiep.Controllers
{
    [Route("api/[controller]")]
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

        /// <summary>
        /// Lấy danh sách tất cả sản phẩm
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SanPham>>> GetAllSanPhams()
        {
            try
            {
                var sanPhams = await _context.SanPhams
                    .Include(sp => sp.MaDanhMucNavigation)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách sản phẩm thành công",
                    data = sanPhams,
                    count = sanPhams.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách sản phẩm");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi truy xuất dữ liệu",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy sản phẩm theo ID
        /// </summary>
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
                    return NotFound(new
                    {
                        success = false,
                        message = $"Không tìm thấy sản phẩm với ID: {id}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy sản phẩm thành công",
                    data = sanPham
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy sản phẩm ID: {id}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy sản phẩm theo danh mục
        /// </summary>
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<SanPham>>> GetSanPhamByCategory(int categoryId)
        {
            try
            {
                var sanPhams = await _context.SanPhams
                    .Where(sp => sp.MaDanhMuc == categoryId)
                    .Include(sp => sp.MaDanhMucNavigation)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Lấy sản phẩm theo danh mục thành công",
                    data = sanPhams,
                    count = sanPhams.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy sản phẩm theo danh mục: {categoryId}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra",
                    error = ex.Message
                });
            }
        }

        // ============ CREATE METHOD ============

        /// <summary>
        /// Thêm sản phẩm mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<SanPham>> CreateSanPham([FromBody] CreateSanPhamDto dto)
        {
            try
            {
                // Validate dữ liệu
                if (string.IsNullOrWhiteSpace(dto.TenSanPham))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tên sản phẩm không được để trống"
                    });
                }

                if (dto.GiaTien <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Giá tiền phải lớn hơn 0"
                    });
                }

                // Kiểm tra danh mục có tồn tại không
                if (dto.MaDanhMuc.HasValue)
                {
                    var danhMucExists = await _context.DanhMucs
                        .AnyAsync(dm => dm.MaDanhMuc == dto.MaDanhMuc);

                    if (!danhMucExists)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = $"Danh mục ID {dto.MaDanhMuc} không tồn tại"
                        });
                    }
                }

                var sanPham = new SanPham
                {
                    TenSanPham = dto.TenSanPham,
                    GiaTien = dto.GiaTien,
                    MoTa = dto.MoTa,
                    HinhAnh = dto.HinhAnh,
                    MaDanhMuc = dto.MaDanhMuc
                };

                _context.SanPhams.Add(sanPham);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Thêm sản phẩm mới thành công: {sanPham.MaSanPham}");

                return CreatedAtAction(nameof(GetSanPhamById), new { id = sanPham.MaSanPham }, new
                {
                    success = true,
                    message = "Thêm sản phẩm thành công",
                    data = sanPham
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm sản phẩm");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi thêm sản phẩm",
                    error = ex.Message
                });
            }
        }

        // ============ UPDATE METHOD ============

        /// <summary>
        /// Cập nhật sản phẩm
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSanPham(int id, [FromBody] UpdateSanPhamDto dto)
        {
            try
            {
                var sanPham = await _context.SanPhams.FindAsync(id);

                if (sanPham == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Không tìm thấy sản phẩm ID: {id}"
                    });
                }

                // Validate dữ liệu
                if (!string.IsNullOrWhiteSpace(dto.TenSanPham) && string.IsNullOrWhiteSpace(dto.TenSanPham))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tên sản phẩm không được để trống"
                    });
                }

                if (dto.GiaTien.HasValue && dto.GiaTien <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Giá tiền phải lớn hơn 0"
                    });
                }

                // Kiểm tra danh mục có tồn tại không
                if (dto.MaDanhMuc.HasValue)
                {
                    var danhMucExists = await _context.DanhMucs
                        .AnyAsync(dm => dm.MaDanhMuc == dto.MaDanhMuc);

                    if (!danhMucExists)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = $"Danh mục ID {dto.MaDanhMuc} không tồn tại"
                        });
                    }
                }

                // Cập nhật các trường
                if (!string.IsNullOrWhiteSpace(dto.TenSanPham))
                    sanPham.TenSanPham = dto.TenSanPham;

                if (dto.GiaTien.HasValue)
                    sanPham.GiaTien = dto.GiaTien.Value;

                if (dto.MoTa != null)
                    sanPham.MoTa = dto.MoTa;

                if (!string.IsNullOrWhiteSpace(dto.HinhAnh))
                    sanPham.HinhAnh = dto.HinhAnh;

                if (dto.MaDanhMuc.HasValue)
                    sanPham.MaDanhMuc = dto.MaDanhMuc;

                _context.SanPhams.Update(sanPham);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Cập nhật sản phẩm thành công: {id}");

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật sản phẩm thành công",
                    data = sanPham
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi cập nhật sản phẩm ID: {id}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi cập nhật sản phẩm",
                    error = ex.Message
                });
            }
        }

        // ============ DELETE METHOD ============

        /// <summary>
        /// Xóa sản phẩm
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSanPham(int id)
        {
            try
            {
                var sanPham = await _context.SanPhams.FindAsync(id);

                if (sanPham == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Không tìm thấy sản phẩm ID: {id}"
                    });
                }

                // Kiểm tra xem sản phẩm có được sử dụng trong đơn hàng không
                var hasOrders = await _context.ChiTietDonHangs
                    .AnyAsync(ctdh => ctdh.MaSanPham == id);

                if (hasOrders)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Không thể xóa sản phẩm này vì nó đang được sử dụng trong đơn hàng"
                    });
                }

                _context.SanPhams.Remove(sanPham);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Xóa sản phẩm thành công: {id}");

                return Ok(new
                {
                    success = true,
                    message = "Xóa sản phẩm thành công",
                    data = new { maSanPham = id }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa sản phẩm ID: {id}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi xóa sản phẩm",
                    error = ex.Message
                });
            }
        }

        // ============ SEARCH METHOD ============

        /// <summary>
        /// Tìm kiếm sản phẩm theo tên
        /// </summary>
        [HttpGet("search/{keyword}")]
        public async Task<ActionResult<IEnumerable<SanPham>>> SearchSanPham(string keyword)
        {
            try
            {
                var sanPhams = await _context.SanPhams
                    .Where(sp => sp.TenSanPham.Contains(keyword))
                    .Include(sp => sp.MaDanhMucNavigation)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Tìm thấy {sanPhams.Count} sản phẩm",
                    data = sanPhams,
                    count = sanPhams.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tìm kiếm sản phẩm: {keyword}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra",
                    error = ex.Message
                });
            }
        }
    }
}