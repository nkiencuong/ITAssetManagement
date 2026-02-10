using System.Security.Claims;
using System.Text.Json;

namespace ITAssetManagement.Client.Helpers
{
    public static class JwtParser
    {
        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1]; // Lấy phần giữa của Token
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs != null)
            {
                foreach (var kvp in keyValuePairs)
                {
                    // Xử lý Role (Quyền)
                    if (kvp.Key == "role" || kvp.Key == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                    {
                        if (kvp.Value.ToString()!.Trim().StartsWith("[")) // Nếu có nhiều quyền
                        {
                            var parsedRoles = JsonSerializer.Deserialize<string[]>(kvp.Value.ToString()!);
                            if (parsedRoles != null)
                            {
                                foreach (var parsedRole in parsedRoles)
                                {
                                    claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                                }
                            }
                        }
                        else // Nếu chỉ có 1 quyền
                        {
                            claims.Add(new Claim(ClaimTypes.Role, kvp.Value.ToString()!));
                        }
                    }
                    else // Các thông tin khác (Tên, Email...)
                    {
                        claims.Add(new Claim(kvp.Key, kvp.Value.ToString()!));
                    }
                }
            }
            return claims;
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
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