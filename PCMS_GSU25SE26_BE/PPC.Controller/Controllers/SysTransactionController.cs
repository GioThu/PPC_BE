using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.TransactionRequest;

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
        public async Task<IActionResult> GetMyTransactions([FromQuery] string? transactionType, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var accountId = User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value;
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Account not found in token.");

            var filter = new GetTransactionFilterRequest
            {
                TransactionType = transactionType,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var response = await _sysTransactionService.GetTransactionsByAccountAsync(accountId, filter);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }
    }
}
