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

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetDepositsByStatus(int status)
        {
            var response = await _depositService.GetDepositsByStatusAsync(status);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }
    }
}
