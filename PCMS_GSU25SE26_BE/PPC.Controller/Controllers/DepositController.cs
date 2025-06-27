using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.DepositRequest;

namespace PPC.Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepositController : ControllerBase
    {
        private readonly IDepositService _depositService;

        public DepositController(IDepositService depositService)
        {
            _depositService = depositService;
        }

        [Authorize(Roles = "3")]
        [HttpPost("Deposit")]
        public async Task<IActionResult> CreateDeposit([FromBody] DepositCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var accountId = User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value;
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("AccountId not found in token.");

            var response = await _depositService.CreateDepositAsync(accountId, request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "2")]
        [HttpPost("withdraw")]
        public async Task<IActionResult> CreateWithdraw([FromBody] WithdrawCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var accountId = User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value;
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("AccountId not found in token.");

            var response = await _depositService.CreateWithdrawAsync(accountId, request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "1")]
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetDepositsByStatus(int status)
        {
            var response = await _depositService.GetDepositsByStatusAsync(status);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "2")] 
        [HttpGet("my-withdraws")]
        public async Task<IActionResult> GetMyDeposits()
        {
            var accountId = User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value;
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("AccountId not found in token.");

            var response = await _depositService.GetMyDepositsAsync(accountId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "1")] 
        [HttpPut("change-status")]
        public async Task<IActionResult> ChangeDepositStatus([FromBody] DepositChangeStatusRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _depositService.ChangeDepositStatusAsync(request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

    }
}
