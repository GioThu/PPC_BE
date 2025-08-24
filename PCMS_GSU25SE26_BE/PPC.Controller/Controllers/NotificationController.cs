using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;

namespace PPC.Controller.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("my-notifications")]
        public async Task<IActionResult> GetMyNotifications()
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            var counselorId = User.Claims.FirstOrDefault(c => c.Type == "counselorId")?.Value;

            string idToUse;

            if (!string.IsNullOrEmpty(memberId))
            {
                idToUse = memberId;
            }
            else if (!string.IsNullOrEmpty(counselorId))
            {
                idToUse = counselorId;
            }
            else
            {
                idToUse = "1";
            }

            var response = await _notificationService.GetNotificationsAsync(idToUse);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var userClaims = User.Claims;

            var memberId = userClaims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            var counselorId = userClaims.FirstOrDefault(c => c.Type == "counselorId")?.Value;

            string idToUse;

            if (!string.IsNullOrEmpty(memberId))
            {
                idToUse = memberId;
            }
            else if (!string.IsNullOrEmpty(counselorId))
            {
                idToUse = counselorId;
            }
            else
            {
                idToUse = "1";
            }

            var summary = await _notificationService.GetNotificationSummaryAsync(idToUse);
            return Ok(summary);
        }
    }
}
