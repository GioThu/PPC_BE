using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.Couple;

namespace PPC.Controller.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CoupleController : ControllerBase
    {
        private readonly ICoupleService _coupleService;

        public CoupleController(ICoupleService coupleService)
        {
            _coupleService = coupleService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCouple()
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("MemberId not found in token.");

            var response = await _coupleService.CreateCoupleAsync(memberId);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpGet("my-room")]
        public async Task<IActionResult> GetMyRoom()
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("MemberId not found in token.");

            var response = await _coupleService.GetMyRoomsAsync(memberId);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinCouple([FromBody] JoinCoupleRequest request)
        {
            if (string.IsNullOrEmpty(request.AccessCode))
                return BadRequest("AccessCode is required.");

            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("MemberId not found in token.");

            var response = await _coupleService.JoinCoupleByAccessCodeAsync(memberId, request.AccessCode);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpGet("{coupleId}")]
        public async Task<IActionResult> GetCoupleDetail(string coupleId)
        {
            var response = await _coupleService.GetCoupleDetailAsync(coupleId);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }
    }
}
