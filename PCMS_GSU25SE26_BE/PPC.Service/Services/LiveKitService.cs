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

        public LiveKitService(ISysTransactionRepository sysTransactionRepository, IBookingRepository bookingRepository)
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
            var apiKey = "APItJgZdfH9Du4U";
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
                return false;

            var token = authorizationHeader["Bearer ".Length..];

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(apiKey)),
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParams, out var validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;

                var hashClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "hash")?.Value;
                using var sha256 = SHA256.Create();
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawBody));
                var calculatedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                if (calculatedHash != hashClaim?.ToLower())
                    return false;

                // Parse JSON
                var json = JsonDocument.Parse(rawBody);
                var eventType = json.RootElement.GetProperty("event").GetString();

                // Handle different events
                switch (eventType)
                {
                    case "participant_left":
                        var roomSid = json.RootElement.GetProperty("room").GetProperty("sid").GetString();
                        var participantIdentity = json.RootElement.GetProperty("participant").GetProperty("identity").GetString();

                        // TODO: Gọi logic kiểm tra còn ai không → nếu không còn, gọi cleanup
                        Console.WriteLine($"Participant {participantIdentity} left room {roomSid}");
                        break;

                    case "room_finished":
                        var roomSidFinished = json.RootElement.GetProperty("room").GetProperty("sid").GetString();
                        new SysTransaction
                        {
                            Id = Guid.NewGuid().ToString(),
                            TransactionType = "LiveKitRoomFinished",
                            CreateBy = "system",
                            DocNo = roomSidFinished,
                        };
                        break;

                    default:
                        Console.WriteLine($"Unhandled event: {eventType}");
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Webhook validation failed: " + ex.Message);
                return false;
            }
        }
    }
}
