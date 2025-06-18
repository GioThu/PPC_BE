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
        _apiKey = "106bf9f6fac65aab09b8572ca4c634305061956886d371fafc5c901e6cf74e0f"; // API Key của bạn
    }

    // Phương thức tạo phòng
    public async Task<ServiceResponse<RoomResponse>> CreateRoomAsync(CreateRoomRequest2 request)
    {
        var dailyBaseUrl = "https://api.daily.co/v1";

        // Cấu hình authorization với API key
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        string roomUrl;

        // Kiểm tra xem phòng đã tồn tại chưa
        var roomExists = await CheckRoomExistsAsync(request.RoomName);

        if (roomExists)
        {
            // Nếu phòng đã tồn tại, chỉ cần lấy URL của phòng
            roomUrl = await GetRoomUrlAsync(request.RoomName);
        }
        else
        {
            // Nếu phòng chưa tồn tại, tạo phòng mới
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

            // Kiểm tra xem API có trả về thành công không
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Create room failed: {responseJson}");

            // Lấy URL của phòng mới từ response
            dynamic roomData = JsonConvert.DeserializeObject(responseJson);
            roomUrl = roomData.url;
        }

        // Tạo token cho người dùng (token được tạo ngay cả khi phòng đã tồn tại)
        var token = GenerateMeetingToken(request.RoomName, request.UserName, request.StartTime, request.EndTime);

        // Trả kết quả với joinUrl chứa token
        return ServiceResponse<RoomResponse>.SuccessResponse(new RoomResponse
        {
            JoinUrl = $"{roomUrl}?t={token}",  // Dùng URL phòng đã có hoặc phòng mới và thêm token vào URL
            RoomName = request.RoomName,
            UserName = request.UserName
        });
    }

    // Kiểm tra xem phòng đã tồn tại chưa
    private async Task<bool> CheckRoomExistsAsync(string roomName)
    {
        var dailyBaseUrl = "https://api.daily.co/v1";
        var response = await _httpClient.GetAsync($"{dailyBaseUrl}/rooms/{roomName}");

        if (response.IsSuccessStatusCode)
        {
            return true;  // Phòng đã tồn tại
        }
        return false; // Phòng chưa tồn tại
    }

    // Lấy URL phòng
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

    // Hàm tạo JWT Token
    private string GenerateMeetingToken(string roomName, string userName, DateTime startTime, DateTime endTime)
    {
        var claims = new[]
        {
            new Claim("iss", _apiKey),  // API Key
            new Claim("nbf", new DateTimeOffset(startTime).ToUnixTimeSeconds().ToString()), // Thời gian bắt đầu
            new Claim("exp", new DateTimeOffset(endTime).ToUnixTimeSeconds().ToString()), // Thời gian hết hạn
            new Claim("room", roomName),  // Tên phòng
            new Claim("user_name", userName),  // Tên người dùng
            new Claim("is_owner", "false")  // Người dùng không phải chủ phòng
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_apiKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(claims: claims, signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
