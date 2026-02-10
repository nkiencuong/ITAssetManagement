using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json; // Cần cái này để giải mã Token

namespace ITAssetManagement.Client
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _http;

        // Biến xác định trạng thái "Chưa đăng nhập" để dùng lại cho gọn
        private readonly AuthenticationState _anonymous;

        public CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient http)
        {
            _localStorage = localStorage;
            _http = http;
            _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // 1. Lấy token từ LocalStorage
                var token = await _localStorage.GetItemAsStringAsync("authToken");

                if (string.IsNullOrWhiteSpace(token))
                {
                    return _anonymous;
                }

                // 2. Tự giải mã Token (Không cần gọi class bên ngoài)
                var claims = ParseClaimsFromJwt(token);

                // 3. Tạo User
                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);

                // 4. Gắn Token vào Header để gọi API sau này
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                return new AuthenticationState(user);
            }
            catch (Exception)
            {
                // 🔥 QUAN TRỌNG: Nếu lỗi (Token rác, hết hạn...) -> Xóa token và về trạng thái khách
                // Tuyệt đối không throw exception ở đây để tránh sập web
                await _localStorage.RemoveItemAsync("authToken");
                _http.DefaultRequestHeaders.Authorization = null;
                return _anonymous;
            }
        }

        public async Task Login(string token)
        {
            await _localStorage.SetItemAsStringAsync("authToken", token);

            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            _http.DefaultRequestHeaders.Authorization = null;
            NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
        }

        // 👇 HÀM GIẢI MÃ TOKEN (ĐƯỢC NHÚNG TRỰC TIẾP VÀO ĐÂY ĐỂ TRÁNH LỖI THIẾU FILE)
        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}