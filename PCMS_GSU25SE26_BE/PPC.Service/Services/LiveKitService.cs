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
using System.Text.Json;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class LiveKitService : ILiveKitService
    {
        private readonly ISysTransactionRepository _sysTransactionRepository;

        public LiveKitService()
        {
        }

        public LiveKitService(ISysTransactionRepository sysTransactionRepository)
        {
            _sysTransactionRepository = sysTransactionRepository;
        }
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

        public async Task<bool> HandleWebhookAsync(string rawBody, string authorizationHeader)
        {
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
                return false;

            var token = authorizationHeader.Substring("Bearer ".Length);  // Lấy token JWT từ header

            // Xác thực JWT token
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("yWDqIOHThQX7z8aNdFuzpTHxzjmrvMSsZYF4eXb8tbL"))
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
                    return false;  // Nếu hash không khớp, không xử lý

                // Xử lý các sự kiện của LiveKit
                var eventType = jwtToken.Claims.FirstOrDefault(c => c.Type == "event")?.Value;

                switch (eventType)
                {
                    case "participant_left":
                        break;

                    case "room_finished":
                        new SysTransaction
                        {
                            Id = Guid.NewGuid().ToString(),
                            TransactionType = "LiveKitRoomFinished",
                            CreateBy = "system",
                            DocNo = "finish",
                            CreateDate = DateTime.UtcNow,

                        };
                        await _sysTransactionRepository.CreateAsync(new SysTransaction
                        {
                            Id = Guid.NewGuid().ToString(),
                            TransactionType = "LiveKitRoomFinished",
                            CreateBy = "system",
                            DocNo = "finish",
                            CreateDate = DateTime.UtcNow,
                        });
                        break;

                    default:
                        new SysTransaction
                        {
                            Id = Guid.NewGuid().ToString(),
                            TransactionType = "LiveKitRoomFinished",
                            CreateBy = "system",
                            DocNo = "finish",
                            CreateDate = DateTime.UtcNow,

                        };
                        await _sysTransactionRepository.CreateAsync(new SysTransaction
                        {
                            Id = Guid.NewGuid().ToString(),
                            TransactionType = "LiveKitRoomFinished",
                            CreateBy = "system",
                            DocNo = "finish",
                            CreateDate = DateTime.UtcNow,
                        });
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

