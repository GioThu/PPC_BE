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

        [HttpGet("overview-booking")]
        public async Task<IActionResult> GetSummaryAdmin()
        {
            var resp = await _dashboardService.GetOverviewAsync();
            if (resp.Success) return Ok(resp);
            return BadRequest(resp);
        }
    }
}
