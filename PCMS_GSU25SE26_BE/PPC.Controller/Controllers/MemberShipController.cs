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

        [Authorize(Roles = "1")]
        [HttpGet]
        public async Task<IActionResult> GetAllMemberShips()
        {
            var response = await _memberShipService.GetAllMemberShipsAsync();
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "1")]
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

        [Authorize(Roles = "1")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMemberShip(string id)
        {
            var response = await _memberShipService.DeleteMemberShipAsync(id);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "3")]
        [HttpPost("buy")]
        public async Task<IActionResult> BuyMemberShip([FromBody] MemberBuyMemberShipRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var accountId = User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value;
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            var response = await _memberShipService.BuyMemberShipAsync(memberId, accountId, request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "3")]
        [HttpGet("my-membership-status")]
        public async Task<IActionResult> GetMemberShipStatus()
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;

            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("MemberId not found.");

            var response = await _memberShipService.GetMemberShipStatusAsync(memberId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }
    }
}
