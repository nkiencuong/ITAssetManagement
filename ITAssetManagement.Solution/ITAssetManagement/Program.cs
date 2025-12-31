using ITAssetManagement.Models;
using ITAssetManagement.Repo.Interfaces;
using ITAssetManagement.Repo.Repositories;
using Microsoft.EntityFrameworkCore;
using ITAssetManagement.Repo.Interfaces;
using ITAssetManagement.Service.Interfaces;
using ITAssetManagement.Service.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
//Connection String trong file appsettings.json
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddAutoMapper(typeof(ITAssetManagement.Mapper.MappingProfile).Assembly);

// Đăng ký Service
builder.Services.AddScoped<IAssetService, AssetService>();

builder.Services.AddScoped<IAllocationService, AllocationService>();
// 1. Thêm dịch vụ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:7085") // Link trang Blazor của bạn (như trong ảnh)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowBlazorClient");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
