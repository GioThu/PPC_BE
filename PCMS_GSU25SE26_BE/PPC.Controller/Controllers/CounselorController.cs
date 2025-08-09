using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest;
using PPC.Service.ModelRequest.AccountRequest;
using PPC.Service.ModelRequest.WorkScheduleRequest;

namespace PPC.Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CounselorController : ControllerBase
    {
        private readonly ICounselorService _counselorService;

        public CounselorController(ICounselorService counselorService)
        {
            _counselorService = counselorService;
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        public async Task<IActionResult> GetAllCounselors()
        {
            var response = await _counselorService.GetAllCounselorsAsync();
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "1")]
        [HttpGet("paging")]
        public async Task<IActionResult> GetPaging([FromQuery] PagingRequest request)
        {
            var response = await _counselorService.GetAllPagingAsync(request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [AllowAnonymous]
        [HttpGet("active-with-sub")]
        public async Task<IActionResult> GetActiveCounselorsWithSub()
        {
            var response = await _counselorService.GetActiveCounselorsWithSubAsync();
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [AllowAnonymous]
        [HttpPost("available-schedule")]
        public async Task<IActionResult> GetAvailableSchedule([FromBody] GetAvailableScheduleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _counselorService.GetAvailableScheduleAsync(request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "1")] 
        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] CounselorStatusUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _counselorService.UpdateStatusAsync(request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpGet("recommend")]
        public async Task<IActionResult> Recommend()
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("Không tìm thấy người dùng");

            var result = await _counselorService.GetRecommendedCounselorsAsync(memberId);
            return Ok(result);
        }

        [HttpGet("recommendations/by-couple/{coupleId}")]
        public async Task<IActionResult> GetRecommendationsByCoupleId(string coupleId)
        {
            var response = await _counselorService.GetRecommendedCounselorsByCoupleIdAsync(coupleId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }
    }
}
