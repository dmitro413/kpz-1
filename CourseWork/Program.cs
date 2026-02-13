using CourseWork.Core.Data;
using CourseWork.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<UnitOfWork>();
builder.Services.AddScoped<IFileService, FileService>();

var jwtKey = "111111111111111111111111111111111";
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});


builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();
builder.Services.AddSignalR();


builder.Services.AddCors(options => {
    options.AddPolicy("BlazorPolicy", builder => builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});


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
app.UseCors("BlazorPolicy");


app.Use(async (context, next) =>
{
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Method: {context.Request.Method}, Path: {context.Request.Path}");
    await next();
});

int requestCount = 0;
app.MapGet("/count", async (context) =>
{
    requestCount++;
    await context.Response.WriteAsync($"The amount of processed requests is {requestCount}");
});

app.MapGet("/who", () => "Виноградов Дмитро");
app.MapGet("/time", () => DateTime.Now.ToString("HH:mm:ss"));

app.Use(async (context, next) =>
{
    if (context.Request.Query.ContainsKey("custom"))
    {
        await context.Response.WriteAsync("You've hit a custom middleware!");
        return;
    }
    await next();
});

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api-test"))
    {
        if (!context.Request.Headers.TryGetValue("X-API-KEY", out var key) || key != "SecretKey123")
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("403 Forbidden: Missing or invalid API Key");
            return;
        }
    }
    await next();
});
app.MapGet("/api-test", () => "API access granted!");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Shop}/{action=Index}/{id?}");

app.MapHub<CourseWork.Hubs.ShopHub>("/shopHub");

app.Run();