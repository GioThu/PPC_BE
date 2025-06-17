using PPC.Repository.Interfaces;
using PPC.Repository.Repositories;
using PPC.Service.Interfaces;
using PPC.Service.Mappers;
using PPC.Service.ModelRequest.TransactionRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.SysTransactionResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class SysTransactionService : ISysTransactionService
    {
        private readonly ISysTransactionRepository _sysTransactionRepository;
        private readonly IBookingRepository _bookingRepository;

        public SysTransactionService(ISysTransactionRepository sysTransactionRepository, IBookingRepository bookingRepository)
        {
            _sysTransactionRepository = sysTransactionRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<ServiceResponse<string>> CreateTransactionAsync(SysTransactionCreateRequest request)
        {
            var transaction = request.ToCreateSysTransaction();
            await _sysTransactionRepository.CreateAsync(transaction);

            return ServiceResponse<string>.SuccessResponse("Transaction created successfully.");
        }

        public async Task<ServiceResponse<List<TransactionSummaryDto>>> GetTransactionsByAccountAsync(string accountId)
        {
            var transactions = await _sysTransactionRepository
                .GetAllAsync(); 

            var filtered = transactions
                .Where(t => t.CreateBy == accountId)
                .OrderByDescending(t => t.CreateDate)
                .ToList();

            var result = new List<TransactionSummaryDto>();

            foreach (var trans in filtered)
            {
                string description = string.Empty;
                double amount = 0;

                switch (trans.TransactionType)
                {
                    case "1":
                        var booking = await _bookingRepository.GetByIdWithCounselor(trans.DocNo);
                        if (booking != null)
                        {
                            description = $"Bạn đã booking tư vấn {booking.Counselor.Fullname} vào {booking.TimeStart?.ToString("dd/MM/yyyy HH:mm")}";
                            amount = booking.Price ?? 0;
                        }
                        break;

                    default:
                        description = "(Unknown transaction type)";
                        break;
                }

                result.Add(new TransactionSummaryDto
                {
                    Id = trans.Id,
                    TransactionType = trans.TransactionType,
                    DocNo = trans.DocNo,
                    CreateBy = trans.CreateBy,
                    Description = description,
                    Amount = amount                  
                });
            }

            return ServiceResponse<List<TransactionSummaryDto>>.SuccessResponse(result);
        }
    }
}
