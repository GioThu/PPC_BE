using Livekit.Server.Sdk.Dotnet;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.RoomRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.RoomResponse;
using PPC.Service.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

public class RoomService : IRoomService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public RoomService()
    {
        _httpClient = new HttpClient();
        _apiKey = "106bf9f6fac65aab09b8572ca4c634305061956886d371fafc5c901e6cf74e0f"; 
    }

    public async Task<ServiceResponse<RoomResponse>> CreateRoomAsync(CreateRoomRequest2 request)
    {
        var dailyBaseUrl = "https://api.daily.co/v1";

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        string roomUrl;

        var roomExists = await CheckRoomExistsAsync(request.RoomName);

        if (roomExists)
        {
            roomUrl = await GetRoomUrlAsync(request.RoomName);
        }
        else
        {
            var now = DateTime.UtcNow;
            long nbfUnix = new DateTimeOffset(now).ToUnixTimeSeconds();
            var durationMinutes = (request.EndTime - request.StartTime).TotalMinutes;
            long expUnix = new DateTimeOffset(now.AddMinutes(durationMinutes)).ToUnixTimeSeconds();
            var roomPayload = new
            {
                name = request.RoomName,
                privacy = "public",
                properties = new
                {
                    exp = expUnix,
                    nbf = nbfUnix,
                    enable_chat = true,
                    enable_screenshare = true,
                    start_video_off = false,
                    start_audio_off = false
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(roomPayload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{dailyBaseUrl}/rooms", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Create room failed: {responseJson}");

            dynamic roomData = JsonConvert.DeserializeObject(responseJson);
            roomUrl = roomData.url;
        }

        var token = GenerateMeetingToken(request.RoomName, request.UserName, request.StartTime, request.EndTime);

        return ServiceResponse<RoomResponse>.SuccessResponse(new RoomResponse
        {
            JoinUrl = $"{roomUrl}?t={token}",  
            RoomName = request.RoomName,
            UserName = request.UserName
        });
    }

    private async Task<bool> CheckRoomExistsAsync(string roomName)
    {
        var dailyBaseUrl = "https://api.daily.co/v1";
        var response = await _httpClient.GetAsync($"{dailyBaseUrl}/rooms/{roomName}");

        if (response.IsSuccessStatusCode)
        {
            return true; 
        }
        return false; 
    }

    private async Task<string> GetRoomUrlAsync(string roomName)
    {
        var dailyBaseUrl = "https://api.daily.co/v1";
        var response = await _httpClient.GetAsync($"{dailyBaseUrl}/rooms/{roomName}");
        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to get room URL: {responseJson}");

        dynamic roomData = JsonConvert.DeserializeObject(responseJson);
        return roomData.url;
    }

    private string GenerateMeetingToken(string roomName, string userName, DateTime startTime, DateTime endTime)
    {
        var claims = new[]
        {
            new Claim("iss", _apiKey),  // API Key
            new Claim("nbf", new DateTimeOffset(startTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64), // Thời gian bắt đầu
            new Claim("exp", new DateTimeOffset(endTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64), // Thời gian hết hạn
            new Claim("room", roomName),  // Tên phòng
            new Claim("user_name", userName , ClaimValueTypes.String),  // Tên người dùng
            new Claim("is_owner", "false")  // Người dùng không phải chủ phòng
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_apiKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(claims: claims, signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
