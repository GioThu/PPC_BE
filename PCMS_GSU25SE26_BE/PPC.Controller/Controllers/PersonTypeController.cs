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
    }
}
