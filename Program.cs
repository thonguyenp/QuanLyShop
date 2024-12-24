using QuanLyShop.Models;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Web.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using QuanLyShop.Services;
using QuanLyShop.Models.Common;
using QuanLyShop.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString(
"DefaultConnection")));

// Cấu hình Cookie Authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Account/Login";
    options.LogoutPath = "/Admin/Account/Logout";
    options.AccessDeniedPath = "/Admin/Account/Login"; // Định tuyến đến trang từ chối quyền truy cập (nếu cần)
    options.ExpireTimeSpan = TimeSpan.FromHours(1); // Thời gian hết hạn của cookie
    options.SlidingExpiration = true; // Cho phép cookie tự động gia hạn khi người dùng vẫn đang hoạt động
});

// Cấu hình Authentication và Cookie
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Identity.Application"; // Scheme mặc định cho Identity
    options.DefaultChallengeScheme = "Identity.Application"; // Scheme mặc định cho challenge
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// Use Session
builder.Services.AddSession(options => 
{ 
    options.IdleTimeout = TimeSpan.FromSeconds(100000); 
    options.Cookie.HttpOnly = true; 
    options.Cookie.IsEssential = true; 
});
builder.Services.AddDistributedMemoryCache(); // Bộ nhớ tạm cho Session

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Không chuyển đổi thành camelCase
    });

builder.Services.AddScoped<IDbConnection>(sp =>
    new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
// Thêm dịch vụ EmailService
builder.Services.AddSingleton<EmailService>();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Cấu hình các tùy chọn, ví dụ:
    options.Password.RequireDigit = false;

})
.AddEntityFrameworkStores<AppDbContext>() // Đảm bảo bạn đã có AppDbContext
.AddDefaultTokenProviders();



var app = builder.Build();

// Endpoint test gửi email
app.MapGet("/send-email", (EmailService common) =>
{
    var success = common.SendMail(
        name: "Your Name",
        subject: "Test Email",
        content: "This is a test email from EmailService class!",
        toMail: "example@gmail.com"
    );

    return success ? "Email sent successfully!" : "Failed to send email.";
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Xác thực người dùng
app.UseAuthorization();  // Phân quyền người dùng

app.UseSession();
app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "sanPhamRoute",
    pattern: "san-pham",
    defaults: new { controller = "Products", action = "Index" }
);

app.MapControllerRoute(
    name: "Contact",
    pattern: "lien-he",
    defaults: new { controller = "Contacts", action = "Index" }
);
app.MapControllerRoute(
    name: "Intro",
    pattern: "gioi-thieu",
    defaults: new { controller = "Intro", action = "Index" }
);
app.MapControllerRoute(
    name: "Cart",
    pattern: "gio-hang",
    defaults: new { controller = "ShoppingCart", action = "Index" }
);
app.MapControllerRoute(
    name: "CheckOut",
    pattern: "thanh-toan",
    defaults: new { controller = "ShoppingCart", action = "CheckOut" }
);
app.MapControllerRoute(
    name: "News",
    pattern: "tin-tuc",
    defaults: new { controller = "News", action = "Index" }
);
app.MapControllerRoute(
    name: "NewsDetail",
    pattern: "tin-tuc/{alias}/{id?}",
    defaults: new { controller = "News", action = "Detail" }
);
app.MapControllerRoute(
    name: "KhuyenMai",
    pattern: "khuyen-mai/",
    defaults: new { controller = "Article", action = "Index" }
);
app.MapControllerRoute(
    name: "KhuyenMaiDetail",
    pattern: "khuyen-mai/{alias}/{id?}",
    defaults: new { controller = "Article", action = "Detail" }
);

app.MapControllerRoute(
    name: "CategoryProduct",
    pattern: "danh-muc-san-pham/{alias}/{id?}",
    defaults: new { controller = "Products", action = "ProductCategory" }
);

app.MapControllerRoute(
	name: "ProductDetail1",
	pattern: "danh-muc-san-pham/{category}/chi-tiet/{alias}/{id}",
	defaults: new { controller = "Products", action = "Detail" }
);
app.MapControllerRoute(
	name: "ProductDetail2",
	pattern: "/chi-tiet/{alias}/{id}",
	defaults: new { controller = "Products", action = "Detail" }
);
app.Run();
