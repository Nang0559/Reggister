using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FVN_REGISTER.Shared.Utils.Helpers
{
  
        public static class JwtParser
        {
            public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(jwt)) return Enumerable.Empty<Claim>();
                    var parts = jwt.Split('.');
                    if (parts.Length < 2) return Enumerable.Empty<Claim>();

                    var payload = parts[1];
                    var jsonBytes = ParseBase64WithoutPadding(payload);
                    var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

                    if (keyValuePairs == null) return Enumerable.Empty<Claim>();

                    var claims = new List<Claim>();
                    foreach (var kvp in keyValuePairs)
                    {
                        var value = kvp.Value?.ToString() ?? "";
                        switch (kvp.Key)
                        {
                            case "name":
                            case "unique_name":
                                claims.Add(new Claim(ClaimTypes.Name, value.Trim()));
                                break;
                            case "sub":
                                claims.Add(new Claim(ClaimTypes.NameIdentifier, value.Trim()));
                                break;
                            case "role":
                                if (kvp.Value is JsonElement element && element.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var item in element.EnumerateArray())
                                        claims.Add(new Claim(ClaimTypes.Role, item.ToString().Trim()));
                                }
                                else { claims.Add(new Claim(ClaimTypes.Role, value.Trim())); }
                                break;
                            default:
                                claims.Add(new Claim(kvp.Key, value.Trim()));
                                break;
                        }
                    }
                    return claims;
                }
                catch { return Enumerable.Empty<Claim>(); }
            }

            private static byte[] ParseBase64WithoutPadding(string base64)
            {
                // Thêm dòng này từ EOL để chống lỗi ký tự đặc biệt
                base64 = base64.Replace('-', '+').Replace('_', '/');
                switch (base64.Length % 4)
                {
                    case 2: base64 += "=="; break;
                    case 3: base64 += "="; break;
                }
                return Convert.FromBase64String(base64);
            }
        }
    
}
