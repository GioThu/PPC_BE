using Microsoft.IdentityModel.Tokens;
using PPC.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PPC.Service.Services
{
    public class LiveKitService : ILiveKitService
    {
        public string GenerateLiveKitToken(string room, string id, string name, DateTime startTime, DateTime endTime)
        {
            var apiKey = "APItJgZdfH9Du4U";
            var apiSecret = "yWDqIOHThQX7z8aNdFuzpTHxzjmrvMSsZYF4eXb8tbL";

            var exp = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds();
            var nbf = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var iat = nbf;

            var payload = new Dictionary<string, object>
        {
            { "exp", exp },
            { "nbf", nbf },
            { "iat", iat },
            { "iss", apiKey },
            { "sub", id },
            { "name", name },
            { "room", room },
            { "startTime", startTime.ToString("o") },
            { "endTime", endTime.ToString("o") },
            { "video", new Dictionary<string, object>
                {
                    { "canPublish", true },
                    { "canPublishData", true },
                    { "canSubscribe", true },
                    { "room", room },
                    { "roomJoin", true }
                }
            }
        };

            // Mã hóa secret key từ apiSecret
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(apiSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Tạo JWT token với các payload đã cung cấp
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = new JwtSecurityToken(
                issuer: apiKey,
                audience: apiKey, // Có thể là apiKey hoặc giá trị khác tùy vào ứng dụng của bạn
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: credentials
            );

            // Không cần Clear() payload nữa
            foreach (var entry in payload)
            {
                securityToken.Payload[entry.Key] = entry.Value;
            }

            // Trả về token
            return tokenHandler.WriteToken(securityToken);
        }
    }
}
