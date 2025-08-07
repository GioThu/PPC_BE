using Microsoft.AspNetCore.Http;
using PPC.Service.ModelRequest.DepositRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.DepositResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface IDepositService
    {
        Task<ServiceResponse<string>> CreateWithdrawAsync(string accountId, WithdrawCreateRequest request);
        Task<ServiceResponse<string>> CreateDepositAsync(string accountId, DepositCreateRequest request);
        Task<ServiceResponse<List<DepositDto>>> GetDepositsByStatusAsync(int status);
        Task<ServiceResponse<List<DepositDto>>> GetMyDepositsAsync(string accountId);
        Task<ServiceResponse<string>> ChangeDepositStatusAsync(DepositChangeStatusRequest request);
        Task<ServiceResponse<string>> CreateVNPayDepositAsync(HttpContext context, string accountId, VnPayRequest request);

    }
}
