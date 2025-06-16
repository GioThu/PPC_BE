using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.AccountRequest;

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
    }
}
