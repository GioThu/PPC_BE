using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;

namespace PPC.Controller.Controllers
{
    [Authorize(Roles = "3")] 
    [ApiController]
    [Route("api/[controller]")]
    public class ProcessingController : ControllerBase
    {
        private readonly IProcessingService _processingService;

        public ProcessingController(IProcessingService processingService)
        {
            _processingService = processingService;
        }

        [HttpPost("{chapterId}")]
        public async Task<IActionResult> MarkChapterAsDone(string chapterId)
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("Không tìm thấy memberId từ token");

            var result = await _processingService.CreateProcessingAsync(chapterId, memberId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}
