using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest;

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
    }
}
