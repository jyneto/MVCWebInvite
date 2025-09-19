using System.Text;
using System.Text.Json;

namespace MVCWebInvite.Utils
{
    public class JwtUtils
    {
        public static bool IsJwtExpired(string? token)
        {
            if (string.IsNullOrWhiteSpace(token)) return true;

            try
            {
                var parts = token.Split('.');
                if (parts.Length < 2) return true;

                string payload = parts[1].Replace('-', '+').Replace('_', '/');
                switch (payload.Length % 4) { case 2: payload += "=="; break; case 3: payload += "="; break; }

                var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                using var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("exp", out var expEl)) return true;
                var exp = expEl.GetInt64();
                var expUtc = DateTimeOffset.FromUnixTimeSeconds(exp);

                return expUtc <= DateTimeOffset.UtcNow;
            }
            catch
            {
                return true;
            }
        }
    }
}
