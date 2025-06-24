using PPC.Service.ModelRequest.TransactionRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.SysTransactionResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface ISysTransactionService
    {
        Task<ServiceResponse<string>> CreateTransactionAsync(SysTransactionCreateRequest request);
        Task<ServiceResponse<PagingResponse<TransactionSummaryDto>>> GetTransactionsByAccountAsync(string accountId, GetTransactionFilterRequest request);
    }
}
