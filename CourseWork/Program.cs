using Microsoft.EntityFrameworkCore;
using CourseWork.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Globalization; 


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<UnitOfWork>();

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; 
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });
builder.Services.AddHttpContextAccessor();

builder.Services.AddSession(); 

var app = builder.Build();



var cultureInfo = new CultureInfo("uk-UA");

CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization(); 

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Shop}/{action=Index}/{id?}");

app.Run();
