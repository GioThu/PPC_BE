using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.BookingRequest;

namespace PPC.Controller.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
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
    }
}
