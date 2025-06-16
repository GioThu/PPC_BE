using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.MemberShipRequest;

namespace PPC.Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MemberShipController : ControllerBase
    {
        private readonly IMemberShipService _memberShipService;

        public MemberShipController(IMemberShipService memberShipService)
        {
            _memberShipService = memberShipService;
        }

        [Authorize(Roles = "1")]
        [HttpPost]
        public async Task<IActionResult> CreateMemberShip([FromBody] MemberShipCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _memberShipService.CreateMemberShipAsync(request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMemberShips()
        {
            var response = await _memberShipService.GetAllMemberShipsAsync();
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMemberShip([FromBody] MemberShipUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _memberShipService.UpdateMemberShipAsync(request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMemberShip(string id)
        {
            var response = await _memberShipService.DeleteMemberShipAsync(id);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

    }
}
