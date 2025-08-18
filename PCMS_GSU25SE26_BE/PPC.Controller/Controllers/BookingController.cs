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
        public async Task<IActionResult> GetMyBookingsForCounselorPaging(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] int? status = null)
        {
            var counselorId = User.Claims.FirstOrDefault(c => c.Type == "counselorId")?.Value;
            if (string.IsNullOrEmpty(counselorId))
                return Unauthorized("CounselorId not found in token.");

            var response = await _bookingService.GetBookingsByCounselorAsync(counselorId, pageNumber, pageSize, status);
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
        public async Task<IActionResult> GetMyBookingsForMemberPaging([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] int? status = null)
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("MemberId not found in token.");

            var response = await _bookingService.GetBookingsByMemberAsync(memberId, pageNumber, pageSize, status);
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

        [Authorize]
        [HttpGet("{bookingId}/GetRoomUrl")]
        public async Task<IActionResult> GetRoomUrl(string bookingId)
        {

            var accountId = User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value;
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(role))
                return Unauthorized("Invalid token claims.");

            // Chuyển role thành int
            if (!int.TryParse(role, out var roleInt))
                return BadRequest("Invalid role in token.");

            var response = await _bookingService.CreateDailyRoomAsync(accountId, bookingId, roleInt);

            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        //[HttpPost("livekit-webhook")]
        //public async Task<IActionResult> Webhook()
        //{
        //    try
        //    {
        //        using var reader = new StreamReader(Request.Body);
        //        var rawBody = await reader.ReadToEndAsync();

        //        var authHeader = Request.Headers["Authorization"];

        //        if (string.IsNullOrEmpty(authHeader))
        //        {
        //            return Unauthorized("Authorization header is missing.");
        //        }

        //        var success = await _livekitService.HandleWebhookAsync(rawBody, authHeader);

        //        return success ? Ok() : Unauthorized("Invalid webhook token or payload.");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error handling webhook: {ex.Message}");
        //        return StatusCode(500, "Internal Server Error");
        //    }
        //}

        [Authorize(Roles = "2")]
        [HttpPut("{bookingId}/finish")]
        public async Task<IActionResult> EndBooking(string bookingId)
        {
            var response = await _bookingService.ChangeStatusBookingAsync(bookingId, 2);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "3")]
        [HttpPut("{bookingId}/member-cancel")]
        public async Task<IActionResult> MemberCancelBooking(string bookingId)
        {
            var response = await _bookingService.ChangeStatusBookingAsync(bookingId, 4);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "2")]
        [HttpPut("counselor-cancel")]
        public async Task<IActionResult> CancelByCounselor([FromBody] CancelBookingByCounselorRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _bookingService.CancelByCounselorAsync(request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "3")]
        [HttpPut("report")]
        public async Task<IActionResult> ReportBooking([FromBody] BookingReportRequest request)
        {
            if (string.IsNullOrEmpty(request.BookingId) || string.IsNullOrEmpty(request.ReportMessage))
                return BadRequest("BookingId and ReportMessage are required.");

            var response = await _bookingService.ReportBookingAsync(request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "1")] 
        [HttpGet("all-paging")]
        public async Task<IActionResult> GetAllAdminPaging([FromQuery] BookingPagingRequest request)
        {
            var response = await _bookingService.GetAllAdminPagingAsync(request);   
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "1")]
        [HttpPut("change-status")]
        public async Task<IActionResult> GetAllAdminPaging([FromBody] ChangeBookingStatusRequest request)
        {
            var response = await _bookingService.ChangeStatusBookingAsync(request.BookingId, request.Status);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "2")]
        [HttpPut("update-note")]
        public async Task<IActionResult> UpdateBookingNote([FromBody] BookingNoteUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _bookingService.UpdateBookingNoteAsync(request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "3")] 
        [HttpPut("rating")]
        public async Task<IActionResult> RateBooking([FromBody] BookingRatingRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _bookingService.RateBookingAsync(request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpGet("feedbacks/{counselorId}")]
        public async Task<IActionResult> GetFeedbacksForCounselor(string counselorId)
        {
            var result = await _bookingService.GetRatingFeedbackByCounselorAsync(counselorId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [Authorize(Roles = "3")]
        [HttpGet("my-booking-discount")]
        public async Task<IActionResult> GetMyBookingDiscount()
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("MemberId not found in token.");

            var response = await _bookingService.GetMaxBookingDiscountByMemberWrappedAsync(memberId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpPut("assign-member2")]
        public async Task<IActionResult> AssignMember2([FromQuery] string bookingId, [FromQuery] string memberCode)
        {
            if (string.IsNullOrWhiteSpace(bookingId) || string.IsNullOrWhiteSpace(memberCode))
                return BadRequest("Booking ID and Member Code are required.");

            var response = await _bookingService.UpdateMember2Async(bookingId, memberCode);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpGet("invitations")]
        [Authorize(Roles = "3")] // Member role
        public async Task<IActionResult> GetInvitations()
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("MemberId not found in token.");

            var response = await _bookingService.GetInvitationsForMemberAsync(memberId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpPost("accept-invitation")]
        [Authorize(Roles = "3")]
        public async Task<IActionResult> AcceptInvitation([FromQuery] string bookingId)
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("MemberId not found in token.");

            var response = await _bookingService.AcceptInvitationAsync(bookingId, memberId);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("decline-invitation")]
        [Authorize(Roles = "3")]
        public async Task<IActionResult> DeclineInvitation([FromQuery] string bookingId)
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("MemberId not found in token.");

            var response = await _bookingService.DeclineInvitationAsync(bookingId, memberId);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("cancel-invitation")]
        [Authorize(Roles = "3")]
        public async Task<IActionResult> CancelInvitation([FromQuery] string bookingId)
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("MemberId not found in token.");

            var response = await _bookingService.CancelInvitationAsync(bookingId, memberId);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("my-dashboard")]
        public async Task<IActionResult> GetMyDashboard()
        {
            var counselorId = User.Claims.FirstOrDefault(c => c.Type == "counselorId")?.Value;
            if (string.IsNullOrEmpty(counselorId))
                return Unauthorized("CounselorId not found in token.");

            var response = await _bookingService.GetMyDashboardAsync(counselorId);
            if (response.Success) return Ok(response);
            return BadRequest(response);
        }
    }
}
