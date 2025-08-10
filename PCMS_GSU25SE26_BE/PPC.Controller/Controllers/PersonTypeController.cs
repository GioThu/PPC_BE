using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.PersonTypeRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.PersonTypeResponse;

namespace PPC.Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonTypeController : ControllerBase
    {
        private readonly IPersonTypeService _personTypeService;

        public PersonTypeController(IPersonTypeService personTypeService)
        {
            _personTypeService = personTypeService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePersonType([FromBody] CreatePersonTypeRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _personTypeService.CreatePersonTypeAsync(request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpGet("by-survey/{surveyId}")]
        public async Task<IActionResult> GetBySurvey(string surveyId)
        {
            var result = await _personTypeService.GetPersonTypesBySurveyAsync(surveyId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _personTypeService.GetPersonTypeByIdAsync(id);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] PersonTypeUpdateRequest request)
        {
            var result = await _personTypeService.UpdatePersonTypeAsync(request);
            return Ok(result);
        }


        [HttpGet("my-person-type/{surveyId}")]
        public async Task<IActionResult> GetMyPersonType(string surveyId)
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("Không tìm thấy người dùng");

            var result = await _personTypeService.GetMyPersonTypeAsync(memberId, surveyId);
            return Ok(result);
        }

        [HttpGet("my-history/{surveyId}")]
        public async Task<IActionResult> GetMyHistoryBySurvey(string surveyId)
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("MemberId not found in token.");

            var response = await _personTypeService.GetHistoryByMemberAndSurveyAsync(memberId, surveyId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpPost("get-by-name")]
        public async Task<IActionResult> GetByNameAndSurvey([FromBody] PersonTypeByNameRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.SurveyId))
                return BadRequest("Name and SurveyId are required.");

            var result = await _personTypeService.GetByNameAndSurveyIdAsync(request);
            if (result.Success)
                return Ok(result);

            return NotFound(result);
        }

        [HttpPost("before-booking")]

        public async Task<IActionResult> GetHistoriesBeforeBooking([FromBody] GetHistoryBeforeBookingRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.MemberId) ||
                string.IsNullOrWhiteSpace(request.SurveyId) ||
                string.IsNullOrWhiteSpace(request.BookingId))
            {
                return BadRequest(ServiceResponse<List<ResultHistoryResponse>>
                    .ErrorResponse("Thiếu tham số bắt buộc"));
            }

            var result = await _personTypeService
                .GetHistoryByMemberAndSurveyAsync(request.MemberId, request.SurveyId, request.BookingId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}
