using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;

namespace PPC.Controller.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SysTransactionController : ControllerBase
    {
        private readonly ISysTransactionService _sysTransactionService;

        public SysTransactionController(ISysTransactionService sysTransactionService)
        {
            _sysTransactionService = sysTransactionService;
        }

        [HttpGet("my-transactions")]
        public async Task<IActionResult> GetMyTransactions()
        {
            var accountId = User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value;
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Account not found in token.");

            var response = await _sysTransactionService.GetTransactionsByAccountAsync(accountId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }
    }
}
