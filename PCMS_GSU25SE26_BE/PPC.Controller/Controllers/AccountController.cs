using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest;

namespace PPC.API.Controllers
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
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _accountService.RegisterCounselorAsync(accountRegister);

                if (result > 0)
                {
                    return Ok(new { message = "Counselor registered successfully!", result = result });
                }

                return BadRequest("Failed to register counselor.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("login-counselor")]
        public async Task<IActionResult> LoginCounselor([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var token = await _accountService.CounselorLogin(loginRequest);

                if (token == null)
                {
                    return Unauthorized(new { message = "Invalid credentials or counselor not found." });
                }

                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("register-member")]
        public async Task<IActionResult> RegisterMember([FromBody] AccountRegister accountRegister)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _accountService.RegisterMemberAsync(accountRegister);

                if (result > 0)
                {
                    return Ok(new { message = "Member registered successfully!", result = result });
                }

                return BadRequest("Failed to register member.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("login-member")]
        public async Task<IActionResult> LoginMember([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var token = await _accountService.MemberLogin(loginRequest);

                if (token == null)
                {
                    return Unauthorized(new { message = "Invalid credentials or member not found." });
                }

                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [Authorize(Roles = "2")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAccounts()
        {
            try
            {
                var accounts = await _accountService.GetAllAccountsAsync();
                return Ok(accounts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }


    }
}