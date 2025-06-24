using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;

namespace PPC.Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SurveyController : ControllerBase
    {
        private readonly ISurveyService _surveyService;

        public SurveyController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSurveys()
        {
            var result = await _surveyService.GetAllSurveysAsync();
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }
    }
}
