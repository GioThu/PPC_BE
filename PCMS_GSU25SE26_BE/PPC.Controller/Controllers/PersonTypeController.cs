using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.PersonTypeRequest;

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
                return Unauthorized("Member not found.");

            var result = await _personTypeService.GetMyPersonTypeAsync(memberId, surveyId);
            return Ok(result);
        }
    }
}
