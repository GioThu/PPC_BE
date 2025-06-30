using Livekit.Server.Sdk.Dotnet;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using Livekit.Server.Sdk.Dotnet;


namespace PPC.Service.Services
{
    public class LiveKitService : ILiveKitService
    {
        private readonly ISysTransactionRepository _sysTransactionRepository;
        private readonly ILogger<LiveKitService> _logger;
        private readonly string _apiKey = "APItJgZdfH9Du4U";
        private readonly string _apiSecret = "yWDqIOHThQX7z8aNdFuzpTHxzjmrvMSsZYF4eXb8tbL";

        public LiveKitService(ISysTransactionRepository sysTransactionRepository, ILogger<LiveKitService> logger)
        {
            _sysTransactionRepository = sysTransactionRepository;
            _logger = logger;
        }

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
                notBefore: Utils.Utils.GetTimeNow(),
                expires: Utils.Utils.GetTimeNow().AddMinutes(10),
                signingCredentials: credentials
            );

            foreach (var entry in payload)
            {
                securityToken.Payload[entry.Key] = entry.Value;
            }

            return tokenHandler.WriteToken(securityToken);
        }

        public async Task<bool> HandleWebhookAsync(string rawBody, string authorizationHeader)
        {
            var webhookReceiver = new WebhookReceiver(_apiKey, _apiSecret);  

            try
            {
                var webhookEvent = webhookReceiver.Receive(rawBody, authorizationHeader);

                switch (webhookEvent.Event)
                {
                    case "participant_left":
                        _logger.LogInformation($"Participant left: {webhookEvent.Participant.Identity}");
                        break;

                    case "room_finished":
                        var roomSid = webhookEvent.Room.Sid;
                        var transaction = new SysTransaction
                        {
                            Id = Guid.NewGuid().ToString(),
                            TransactionType = "LiveKitRoomFinished",
                            CreateBy = "system",
                            DocNo = roomSid,
                            CreateDate = Utils.Utils.GetTimeNow()
                        };
                        await _sysTransactionRepository.CreateAsync(transaction);
                        _logger.LogInformation($"Room finished: {roomSid}");
                        break;

                    default:
                        _logger.LogInformation($"Unhandled event: {webhookEvent.Event}");
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing webhook: {ex.Message}");
                return false;
            }
        }
    }
}
