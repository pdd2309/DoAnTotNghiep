using DoAnTotNghiep.Models;
using DoAnTotNghiep.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// --- 1. PHẦN ĐĂNG KÝ SERVICES (Dưới builder, TRÊN builder.Build) ---

// Đăng ký DbContext
builder.Services.AddDbContext<CuaHangCongNgheDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký Services của Đông
builder.Services.AddScoped<IDanhMucService, DanhMucService>();
builder.Services.AddScoped<ISanPhamService, SanPhamService>();

// Đăng ký MVC và JSON
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Đăng ký Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ĐĂNG KÝ SESSION (Đông phải để ở đây mới đúng)
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ĐĂNG KÝ HTTPCONTEXTACCESSOR (Để cái Header động đọc được Session)
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// --- 2. PHẦN CẤU HÌNH MIDDLEWARE (Dưới builder.Build, TRÊN app.Run) ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// QUAN TRỌNG: Thứ tự các dòng này không được đổi chỗ cho nhau
app.UseStaticFiles();   // Phải có để hiện CSS/JS/Hình ảnh
app.UseRouting();       // Phải có để định tuyến
app.UseSession();       // Phải nằm trước Authorization
app.UseAuthorization();

// Cấu hình đường dẫn mặc định để mở Web lên là vào trang Index ngay
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();