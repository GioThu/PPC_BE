using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.WorkScheduleRequest;

namespace PPC.Controller.Controllers
{
    [Authorize(Roles = "2")]
    [ApiController]
    [Route("api/[controller]")]
    public class WorkScheduleController : ControllerBase
    {
        private readonly IWorkScheduleService _workScheduleService;

        public WorkScheduleController(IWorkScheduleService workScheduleService)
        {
            _workScheduleService = workScheduleService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateSchedule([FromBody] WorkScheduleCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var counselorId = User.Claims.FirstOrDefault(c => c.Type == "counselorId")?.Value;
            if (string.IsNullOrEmpty(counselorId))
                return Unauthorized("Counselor not found.");

            var response = await _workScheduleService.CreateScheduleAsync(counselorId, request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpGet("my-schedules")]
        public async Task<IActionResult> GetMySchedules()
        {
            var counselorId = User.Claims.FirstOrDefault(c => c.Type == "counselorId")?.Value;
            if (string.IsNullOrEmpty(counselorId))
                return Unauthorized("CounselorId not found in token.");

            var response = await _workScheduleService.GetSchedulesByCounselorAsync(counselorId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpDelete("{scheduleId}")]
        public async Task<IActionResult> DeleteSchedule(string scheduleId)
        {
            var counselorId = User.Claims.FirstOrDefault(c => c.Type == "counselorId")?.Value;
            if (string.IsNullOrEmpty(counselorId))
                return Unauthorized("Counselor not found.");

            var response = await _workScheduleService.DeleteScheduleAsync(counselorId, scheduleId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }
    }
}
