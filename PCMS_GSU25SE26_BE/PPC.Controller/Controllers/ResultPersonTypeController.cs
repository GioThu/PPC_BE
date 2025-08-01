using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.PersonTypeRequest;

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

        [HttpGet("by-persontype/{personTypeId}")]
        public async Task<IActionResult> GetByPersonTypeId(string personTypeId)
        {
            var response = await _resultPersonTypeService.GetResultByPersonTypeIdAsync(personTypeId);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPut("edit")]
        public async Task<IActionResult> Edit([FromBody] ResultPersonTypeEditRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _resultPersonTypeService.UpdateResultPersonTypeAsync(request);
            return response.Success ? Ok(response) : BadRequest(response);
        }   
    }
}
