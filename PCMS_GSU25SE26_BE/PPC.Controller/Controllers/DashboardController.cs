using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;

namespace PPC.Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("counselor-wallet")]
        public async Task<IActionResult> GetSummary()
        {
            var counselorId = User.Claims.FirstOrDefault(c => c.Type == "counselorId")?.Value;
            var accountId = User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value;

            if (string.IsNullOrEmpty(counselorId) || string.IsNullOrEmpty(accountId))
                return Unauthorized("Missing counselorId or accountId in token.");

            var resp = await _dashboardService.GetSummaryAsync(counselorId, accountId);
            if (resp.Success) return Ok(resp);
            return BadRequest(resp);
        }

        [HttpGet("admin-booking")]
        public async Task<IActionResult> GetSummaryBooking()
        {
            var resp = await _dashboardService.GetDashboardAsync();
            if (resp.Success) return Ok(resp);
            return BadRequest(resp);
        }

        [HttpGet("summary/last-3-months")]
        public async Task<IActionResult> Last3Months()
        {
            var resp = await _dashboardService.GetOverviewLast3MonthsAsync();
            if (resp.Success) return Ok(resp);
            return BadRequest(resp);
        }

        [HttpGet("weekly")]
        public async Task<IActionResult> Weekly([FromQuery] int year, [FromQuery] int month)
        {
            if (year <= 0 || month < 1 || month > 12) return BadRequest("year/month không hợp lệ.");
            var resp = await _dashboardService.GetWeeklyCountsAsync(year, month);
            if (resp.Success) return Ok(resp);
            return BadRequest(resp);
        }
    }
}
