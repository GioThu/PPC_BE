using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.SurveyRequest;
using PPC.Service.Services;

namespace PPC.Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SurveyController : ControllerBase
    {
        private readonly ISurveyService _surveyService;
        private readonly IQuestionService _questionService;

        public SurveyController(ISurveyService surveyService, IQuestionService questionService)
        {
            _surveyService = surveyService;
            _questionService = questionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSurveys()
        {
            var result = await _surveyService.GetAllSurveysAsync();
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpGet("randomSurvey")]
        public async Task<IActionResult> GetRandom([FromQuery] string surveyId, [FromQuery] int count = 25)
        {
            if (string.IsNullOrEmpty(surveyId))
                return BadRequest("SurveyId is required.");

            var result = await _questionService.GetRandomQuestionsAsync(surveyId, count);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("getResultSurvey")]
        public async Task<IActionResult> SubmitResult([FromBody] SurveyResultRequest request)
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId)) return Unauthorized("Không tìm thấy người dùng");

            var response = await _surveyService.SubmitResultAsync(memberId, request);
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}
