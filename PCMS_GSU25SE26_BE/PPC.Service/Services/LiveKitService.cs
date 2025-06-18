using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class LiveKitService : ILiveKitService
    {
        private readonly ISysTransactionRepository _sysTransactionRepository;
        private readonly string _apiKey = "APItJgZdfH9Du4U";
        private readonly string _apiSecret = "yWDqIOHThQX7z8aNdFuzpTHxzjmrvMSsZYF4eXb8tbL";

        public LiveKitService(ISysTransactionRepository sysTransactionRepository)
        {
            _sysTransactionRepository = sysTransactionRepository;
        }

        // Tạo LiveKit token
        public string GenerateLiveKitToken(string room, string id, string name, DateTime startTime, DateTime endTime)
        {
            var exp = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds();
            var nbf = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var iat = nbf;

            var payload = new Dictionary<string, object>
            {
                { "exp", exp },
                { "nbf", nbf },
                { "iat", iat },
                { "iss", _apiKey },
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

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_apiSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = new JwtSecurityToken(
                issuer: _apiKey,
                audience: _apiKey,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: credentials
            );

            foreach (var entry in payload)
            {
                securityToken.Payload[entry.Key] = entry.Value;
            }

            return tokenHandler.WriteToken(securityToken);
        }

        // Xử lý Webhook và xác thực token
        public async Task<bool> HandleWebhookAsync(string rawBody, string authorizationHeader)
        {
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                Console.WriteLine("Authorization header is missing or invalid.");
                return false;
            }

            var token = authorizationHeader.Substring("Bearer ".Length);  // Lấy token JWT từ header

            // Xác thực JWT token
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _apiKey,  // Đảm bảo rằng bạn điền đúng giá trị
                ValidAudiences = new[] { _apiKey },  // Đảm bảo rằng bạn điền đúng giá trị
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_apiSecret))
            };

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, validationParams, out var validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;

                // Lấy hash từ claims của token
                var hashClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "hash")?.Value;

                // Tính toán lại hash của payload
                using var sha256 = SHA256.Create();
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawBody));
                var calculatedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                // So sánh hash tính toán và hash trong token
                if (calculatedHash != hashClaim?.ToLower())
                {
                    Console.WriteLine("Hash mismatch: Calculated hash does not match the claim.");
                    return false;  // Nếu hash không khớp, không xử lý
                }

                // Lấy loại sự kiện từ webhook
                var eventType = jwtToken.Claims.FirstOrDefault(c => c.Type == "event")?.Value;

                // Xử lý các sự kiện của LiveKit
                switch (eventType)
                {
                    case "participant_left":
                        // Logic xử lý khi người tham gia rời phòng
                        Console.WriteLine("Participant left the room.");
                        break;

                    case "room_finished":
                        var roomSidFinished = jwtToken.Claims.FirstOrDefault(c => c.Type == "roomSid")?.Value;
                        var transaction = new SysTransaction
                        {
                            Id = Guid.NewGuid().ToString(),
                            TransactionType = "LiveKitRoomFinished",
                            CreateBy = "system",
                            DocNo = roomSidFinished,
                            CreateDate = DateTime.UtcNow
                        };
                        await _sysTransactionRepository.CreateAsync(transaction);
                        Console.WriteLine("Room finished transaction recorded.");
                        break;

                    default:
                        Console.WriteLine($"Unhandled event: {eventType}");
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log lỗi nếu có và trả về false
                Console.WriteLine($"Webhook validation failed: {ex.Message}");
                return false;
            }
        }
    }
}
