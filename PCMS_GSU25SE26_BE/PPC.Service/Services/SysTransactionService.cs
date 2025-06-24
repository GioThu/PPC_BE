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
        private readonly IMemberMemberShipRepository _memberMemberShipRepository;


        public SysTransactionService(ISysTransactionRepository sysTransactionRepository, IBookingRepository bookingRepository, IMemberMemberShipRepository memberMemberShipRepository)
        {
            _sysTransactionRepository = sysTransactionRepository;
            _bookingRepository = bookingRepository;
            _memberMemberShipRepository = memberMemberShipRepository;
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
                            amount = - booking.Price ?? 0;
                        }
                        break;

                    case "2":
                        var booking2 = await _bookingRepository.GetByIdWithCounselor(trans.DocNo);
                        if (booking2 != null)
                        {
                            description = $"Bạn đã hủy booking tư vấn {booking2.Counselor.Fullname} vào {booking2.TimeStart?.ToString("dd/MM/yyyy HH:mm")}";
                            amount = booking2.Price / 2 ?? 0;
                        }
                        break;

                    case "7":
                        var booking7 = await _bookingRepository.GetByIdWithCounselor(trans.DocNo);
                        if (booking7 != null)
                        {
                            description = $"Bạn đã được hoàn tiền từ buổi booking tư vấn {booking7.Counselor.Fullname} vào {booking7.TimeStart?.ToString("dd/MM/yyyy HH:mm")}";
                            amount = booking7.Price ?? 0;
                        }
                        break;

                    case "5":
                        var memberMemberShip = await _memberMemberShipRepository.GetByIdWithMemberShipAsync(trans.DocNo);
                        if (memberMemberShip != null)
                        {
                            description = $"Bạn đã mua gói {memberMemberShip.MemberShip.MemberShipName}";
                            amount = - memberMemberShip.Price ?? 0;
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
                    CreateDate = trans.CreateDate,
                    Description = description,
                    Amount = amount                  
                });
            }

            return ServiceResponse<List<TransactionSummaryDto>>.SuccessResponse(result);
        }
    }
}
