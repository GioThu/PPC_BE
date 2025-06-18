using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.BookingRequest;
using PPC.Service.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PPC.Controller.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILiveKitService _livekitService;
        public BookingController(IBookingService bookingService, ILiveKitService livekitService)
        {
            _bookingService = bookingService;
            _livekitService = livekitService;
        }

        [Authorize(Roles = "3")]
        [HttpPost("book")]
        public async Task<IActionResult> BookCounseling([FromBody] BookingRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Lấy accountId và memberId từ token
            var accountId = User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value;
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;

            if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(memberId))
                return Unauthorized("Invalid token");

            var response = await _bookingService.BookCounselingAsync(memberId, accountId, request);

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "2")] 
        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookingsForCounselor()
        {
            var counselorId = User.Claims.FirstOrDefault(c => c.Type == "counselorId")?.Value;
            if (string.IsNullOrEmpty(counselorId))
                return Unauthorized("CounselorId not found in token.");

            var response = await _bookingService.GetBookingsByCounselorAsync(counselorId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "2")]
        [HttpGet("my-bookings-paging")]
        public async Task<IActionResult> GetMyBookingsForCounselorPaging([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var counselorId = User.Claims.FirstOrDefault(c => c.Type == "counselorId")?.Value;
            if (string.IsNullOrEmpty(counselorId))
                return Unauthorized("CounselorId not found in token.");

            var response = await _bookingService.GetBookingsByCounselorAsync(counselorId, pageNumber, pageSize);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "3")] 
        [HttpGet("my-bookings/member")]
        public async Task<IActionResult> GetMyBookingsForMember()
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("MemberId not found in token.");

            var response = await _bookingService.GetBookingsByMemberAsync(memberId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "3")]
        [HttpGet("my-bookings-paging/member")]
        public async Task<IActionResult> GetMyBookingsForMemberPaging([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("MemberId not found in token.");

            var response = await _bookingService.GetBookingsByMemberAsync(memberId, pageNumber, pageSize);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "1,2,3")]
        [HttpGet("booking-detail/{bookingId}")]
        public async Task<IActionResult> GetBookingById(string bookingId)
        {
            // Lấy accountId và role từ token
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (role == "2")
            {
                if (await _bookingService.CheckIfCounselorCanAccessBooking(bookingId, User.Claims.FirstOrDefault(c => c.Type == "counselorId")?.Value) == false)
                {
                    return Unauthorized("You do not have permission to view this booking.");
                }
            }

            if (role == "3") 
            { 
                if (await _bookingService.CheckIfMemberCanAccessBooking(bookingId, User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value) == false)
                {
                    return Unauthorized("You do not have permission to view this booking.");
                }
            }

            var response = await _bookingService.GetBookingByIdAsync(bookingId);

            if (response.Success)
            {
                return Ok(response.Data);
            }

            return BadRequest(response);
        }


        [Authorize]
        [HttpGet("{bookingId}/livekit-token")]
        public async Task<IActionResult> GetLiveKitToken(string bookingId)
        {

            var accountId = User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value;
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; 

            if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(role))
                return Unauthorized("Invalid token claims.");

            // Chuyển role thành int
            if (!int.TryParse(role, out var roleInt))
                return BadRequest("Invalid role in token.");

            var response = await _bookingService.GetLiveKitToken(accountId, bookingId, roleInt);

            if (response.Success)
            {
                return Ok(response); 
            }

            return BadRequest(response); 
        }

        [HttpPost("livekit-webhook")]
        public async Task<IActionResult> Webhook()
        {
            using var reader = new StreamReader(Request.Body);
            var rawBody = await reader.ReadToEndAsync();
            var authHeader = Request.Headers["Authorization"].ToString();
            var success = await _livekitService.HandleWebhookAsync(rawBody, authHeader);
            return success ? Ok() : Unauthorized();
        }
    }
}
