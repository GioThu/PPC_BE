using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;

namespace PPC.Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultPersonTypeController : ControllerBase
    {
        private readonly IResultPersonTypeService _resultPersonTypeService;

        public ResultPersonTypeController(IResultPersonTypeService resultPersonTypeService)
        {
            _resultPersonTypeService = resultPersonTypeService;
        }

        [HttpPost("generate/{surveyId}")]
        public async Task<IActionResult> GeneratePersonTypePairs(string surveyId)
        {
            var response = await _resultPersonTypeService.GenerateAllPersonTypePairsAsync(surveyId);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }
    }
}
