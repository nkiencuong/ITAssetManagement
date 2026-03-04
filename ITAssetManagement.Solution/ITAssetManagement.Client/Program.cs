using Blazored.LocalStorage; // <--- QUAN TRỌNG: Phải để lên dòng đầu tiên
using ITAssetManagement.Client;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 1. Cấu hình HttpClient (Kết nối đến API)
// Lưu ý: Đảm bảo port 7239 này đúng với port mà Project API đang chạy (xem trong launchSettings.json của API)
//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7239") });
// Code MỚI (Tự động nhận diện IP 192.168.10.81)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
// 2. Đăng ký Blazored LocalStorage
//builder.Services.AddBlazoredLocalStorage();
//3. Phân Quyền
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddBlazoredLocalStorage();

// 3. Chạy ứng dụng
await builder.Build().RunAsync();
//using Blazored.LocalStorage;
//using ITAssetManagement.Client;
//using Microsoft.AspNetCore.Components.Authorization;
//using Microsoft.AspNetCore.Components.Web;
//using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

//var builder = WebAssemblyHostBuilder.CreateDefault(args);
//builder.RootComponents.Add<App>("#app");
//builder.RootComponents.Add<HeadOutlet>("head::after");

//// 1. Cấu hình HttpClient (ĐÃ SỬA ĐỂ CHẠY CHUNG VỚI DOCKER)
//// Chúng ta dùng chính địa chỉ IP của server và cổng 8080 mà bác đã cấu hình cho Docker
//builder.Services.AddScoped(sp => new HttpClient
//{
//    BaseAddress = new Uri("http://192.168.10.81:8080")
//});

//// 2. Đăng ký các dịch vụ khác
//builder.Services.AddBlazoredLocalStorage();
//builder.Services.AddAuthorizationCore();
//builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

//// 3. Chạy ứng dụng
//await builder.Build().RunAsync();