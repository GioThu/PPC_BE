using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest;
using PPC.Service.ModelRequest.AccountRequest;

namespace PPC.Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MemberController : ControllerBase
    {
        private readonly IMemberService _memberService;

        public MemberController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        [Authorize(Roles = "1")]
        [HttpGet("paging")]
        public async Task<IActionResult> GetPaging([FromQuery] PagingRequest request)
        {
            var response = await _memberService.GetAllPagingAsync(request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "1")]
        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] MemberStatusUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _memberService.UpdateStatusAsync(request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }
    }
}
