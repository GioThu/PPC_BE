using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.SurveyRequest;

namespace PPC.Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;

        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        [Authorize(Roles = "1")]
        [HttpPost]
        public async Task<IActionResult> CreateSurveyQuestion([FromBody] SurveyQuestionCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _questionService.CreateQuestionAsync(request);
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [Authorize(Roles = "1")]
        [HttpGet("paging")]
        public async Task<IActionResult> GetPaging([FromQuery] PagingSurveyQuestionRequest request)
        {
            var result = await _questionService.GetPagingAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [Authorize(Roles = "1")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SurveyQuestionUpdateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _questionService.UpdateAsync(request.Id, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [Authorize(Roles = "1")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _questionService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }


    }
}

