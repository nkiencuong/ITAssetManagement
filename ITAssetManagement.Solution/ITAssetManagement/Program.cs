using ITAssetManagement.Models;
using ITAssetManagement.Models.Entities;
using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Repo.Interfaces;
using ITAssetManagement.Repo.Repositories;
using ITAssetManagement.Service.Implementations;
using ITAssetManagement.Service.Interfaces;
using ITAssetManagement.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. PHẦN CẤU HÌNH SERVICES (GIỮ NGUYÊN)
// ==========================================

// Cấu hình JSON (Tránh vòng lặp)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Cấu hình OpenAPI (Swagger bản mới .NET 9)
builder.Services.AddOpenApi();

// Cấu hình Database
var myConnectionString = "Server=.;Database=ITAssetManagement;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(myConnectionString));

// Đăng ký UnitOfWork & Generic Repository
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// AutoMapper
builder.Services.AddAutoMapper(typeof(ITAssetManagement.Mapper.MappingProfile).Assembly);

// --- ĐĂNG KÝ CÁC SERVICE NGHIỆP VỤ ---
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IAllocationService, AllocationService>();
builder.Services.AddScoped<IRepairService, RepairService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IUserService, UserService>();
// --- ĐĂNG KÝ SERVICE LOGIN ---
builder.Services.AddScoped<IAuthService, AuthService>();

// --- CẤU HÌNH JWT AUTHENTICATION ---
var jwtSettings = builder.Configuration.GetSection("Jwt");
// Lưu ý: Đảm bảo Key trong appsettings.json đủ dài (>= 32 ký tự)
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"]
    };
});

// Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:7085", "https://localhost:7239", "http://localhost:5137")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ==========================================
// 2. PHẦN CẤU HÌNH PIPELINE (ĐÃ SỬA LẠI CHUẨN)
// ==========================================

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // 👇 QUAN TRỌNG: Giúp debug Blazor không bị lỗi Integrity
    app.UseWebAssemblyDebugging();
}

app.UseHttpsRedirection();

// 👇 QUAN TRỌNG NHẤT: Phải có dòng này Server mới phục vụ được file Blazor
app.UseBlazorFrameworkFiles();

// Dòng này để phục vụ file tĩnh (ảnh, css) trong wwwroot
app.UseStaticFiles();

app.UseRouting();

// CORS phải đặt giữa Routing và Auth
app.UseCors("AllowBlazorClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 👇 Dòng này giúp khi F5 ở trang con không bị lỗi 404
app.MapFallbackToFile("index.html");

app.Run();