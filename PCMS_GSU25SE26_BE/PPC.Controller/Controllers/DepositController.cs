using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.DepositRequest;
using PPC.Service.Services;

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

        [HttpPost("vnpay-request")]
        [Authorize] // Token chứa accountId
        public async Task<IActionResult> CreateVNPayRequest([FromBody] VnPayRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var accountId = User.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value;
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Account ID not found in token.");

            var response = await _depositService.CreateVNPayDepositAsync(HttpContext, accountId, request);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VNPayReturn([FromQuery] string accountId, [FromQuery] decimal amount)
        {
            var query = HttpContext.Request.Query;

            var responseCode = query["vnp_ResponseCode"].ToString();
            var transactionStatus = query["vnp_TransactionStatus"].ToString();
            var transactionId = query["vnp_TxnRef"].ToString();


            if (responseCode == "00" && transactionStatus == "00")
            {
                var depositRequest = new DepositCreateRequest
                {
                    Total = ((double)amount),
                };
                var result = await _depositService.CreateDepositAsync(accountId, depositRequest);

                if (!result.Success)
                    return Redirect("https://v0-2-page-payment-website.vercel.app/failure");

                return Redirect("https://v0-2-page-payment-website.vercel.app/success");
            }

            return Redirect("https://v0-2-page-payment-website.vercel.app/failure");
        }

    }
}
