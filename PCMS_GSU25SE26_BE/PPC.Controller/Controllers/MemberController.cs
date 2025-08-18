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

        [Authorize(Roles = "3")]
        [HttpGet("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var accountId = User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value;
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("AccountId not found in token.");

            var response = await _memberService.GetMyProfileAsync(accountId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "3")]
        [HttpPut("my-profile")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] MemberProfileUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var accountId = User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value;
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("AccountId not found in token.");

            var response = await _memberService.UpdateMyProfileAsync(accountId, request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "3")]
        [HttpGet("my-subcategories")]
        public async Task<IActionResult> GetMyRecommendedSubCategories()
        {
            var memberId = User.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value;
            if (string.IsNullOrEmpty(memberId))
                return Unauthorized("MemberId not found in token.");

            var response = await _memberService.GetRecommendedSubCategoriesAsync(memberId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }
    }
}
