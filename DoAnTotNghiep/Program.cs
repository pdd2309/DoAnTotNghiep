using DoAnTotNghiep.Models;
using DoAnTotNghiep.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;


var builder = WebApplication.CreateBuilder(args);


// --- 1. PHẦN ĐĂNG KÝ SERVICES ---

builder.Services.AddDbContext<CuaHangCongNgheDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IDanhMucService, DanhMucService>();
builder.Services.AddScoped<ISanPhamService, SanPhamService>();

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthorization();

// ĐĂNG KÝ XÁC THỰC (AUTHENTICATION) CHO GOOGLE VÀ FACEBOOK
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Admin/Account/Login";
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/Admin"))
        {
            context.Response.Redirect("/Admin/Account/Login");
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
})
.AddGoogle(options =>
{
    // Điền ClientId và ClientSecret lấy từ Google Cloud Console
    options.ClientId = "862223372768-0pq4kvvg50j56ofqgj7ccmvo9mp7dgod.apps.googleusercontent.com";
    options.ClientSecret = "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";
    options.Scope.Add("email");
})
.AddFacebook(options =>
{
    // Điền AppId và AppSecret lấy từ Facebook Developers
    options.AppId = "1285875816943111";
    options.AppSecret = "ac81d4e442cf2773f04380953f28d296";
    options.Scope.Add("email");
});

var app = builder.Build();

// --- 2. PHẦN CẤU HÌNH MIDDLEWARE ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// Kích hoạt Authentication (Phải đặt trước Authorization)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();