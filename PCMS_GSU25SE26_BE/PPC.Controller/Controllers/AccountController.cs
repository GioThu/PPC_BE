using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.AccountRequest;
using PPC.Service.Services;
using System.Data;

namespace PPC.Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("register-counselor")]
        public async Task<IActionResult> RegisterCounselor([FromBody] AccountRegister accountRegister)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _accountService.RegisterCounselorAsync(accountRegister);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpPost("login-counselor")]
        public async Task<IActionResult> LoginCounselor([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _accountService.CounselorLogin(loginRequest);
            if (response.Success)
                return Ok(response);
            return Unauthorized(response);
        }

        [HttpPost("register-member")]
        public async Task<IActionResult> RegisterMember([FromBody] AccountRegister accountRegister)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _accountService.RegisterMemberAsync(accountRegister);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpPost("login-member")]
        public async Task<IActionResult> LoginMember([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _accountService.MemberLogin(loginRequest);
            if (response.Success)
                return Ok(response);
            return Unauthorized(response);
        }

        [HttpPost("login-admin")]
        public async Task<IActionResult> LoginAdmin([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _accountService.AdminLogin(loginRequest);
            if (response.Success)
                return Ok(response);
            return Unauthorized(response);
        }

        [Authorize(Roles = "2")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAccounts()
        {
            var response = await _accountService.GetAllAccountsAsync();

            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [Authorize(Roles = "2")]
        [HttpGet("counselor-my-profile")]
        public async Task<IActionResult> CounselorGetMyProfile()
        {
            var counselorId = User.Claims.FirstOrDefault(c => c.Type == "counselorId")?.Value;
            if (string.IsNullOrEmpty(counselorId))
                return Unauthorized("CounselorId not found in token.");

            var response = await _accountService.CounselorGetMyProfileAsync(counselorId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "2")]
        [HttpPut("counselor-edit-profile")]
        public async Task<IActionResult> CounselorEditMyProfile([FromBody] CounselorEditRequest request)
        {
            var counselorId = User.Claims.FirstOrDefault(c => c.Type == "counselorId")?.Value;
            if (string.IsNullOrEmpty(counselorId))
                return Unauthorized("CounselorId not found in token.");

            var response = await _accountService.CounselorEditMyProfileAsync(counselorId, request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "2,3")]
        [HttpGet("wallet-balance")]
        public async Task<IActionResult> GetWalletBalance()
        {
            var accountId = User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value;
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("AccountId not found in token.");

            var response = await _accountService.GetWalletBalanceAsync(accountId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Tuỳ bạn set claim gì trong token — ví dụ "accountId"
            var accountId = User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value
                            ?? User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Không tìm thấy AccountId trong token.");

            var response = await _accountService.ChangePasswordAsync(accountId, request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }
    }
}
