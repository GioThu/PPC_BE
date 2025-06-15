using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;

namespace PPC.Controller.Controllers
{
    [Authorize(Roles = "3")]
    [ApiController]
    [Route("api/[controller]")]
    public class CounselorController : ControllerBase
    {
        private readonly ICounselorService _counselorService;

        public CounselorController(ICounselorService counselorService)
        {
            _counselorService = counselorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCounselors()
        {
            var response = await _counselorService.GetAllCounselorsAsync();
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }
    }
}
