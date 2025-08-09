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
        private readonly IDepositRepository _depositRepository;
        private readonly IEnrollCourseRepository _enrollCourseRepository;


        public SysTransactionService(ISysTransactionRepository sysTransactionRepository, IBookingRepository bookingRepository, IMemberMemberShipRepository memberMemberShipRepository, IDepositRepository depositRepository, IEnrollCourseRepository enrollCourseRepository)
        {
            _sysTransactionRepository = sysTransactionRepository;
            _bookingRepository = bookingRepository;
            _memberMemberShipRepository = memberMemberShipRepository;
            _depositRepository = depositRepository;
            _enrollCourseRepository = enrollCourseRepository;
        }

        public async Task<ServiceResponse<string>> CreateTransactionAsync(SysTransactionCreateRequest request)
        {
            var transaction = request.ToCreateSysTransaction();
            await _sysTransactionRepository.CreateAsync(transaction);

            return ServiceResponse<string>.SuccessResponse("Transaction created successfully.");
        }

        public async Task<ServiceResponse<PagingResponse<TransactionSummaryDto>>> GetTransactionsByAccountAsync(string accountId, GetTransactionFilterRequest request)
        {
            var (transactions, totalCount) = await _sysTransactionRepository
                .GetTransactionsByAccountAsync(accountId, request.TransactionType, request.PageNumber, request.PageSize);

            var result = new List<TransactionSummaryDto>();

            foreach (var trans in transactions)
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
                            amount = -booking.Price ?? 0;
                        }
                        break;

                    case "2":
                        var booking2 = await _bookingRepository.GetByIdWithCounselor(trans.DocNo);
                        if (booking2 != null)
                        {
                            description = $"Bạn đã hủy booking tư vấn {booking2.Counselor.Fullname} vào {booking2.TimeStart?.ToString("dd/MM/yyyy HH:mm")}";
                            amount = booking2.Price/2 ?? 0;
                        }
                        break;

                    case "3":
                        var booking3 = await _bookingRepository.GetByIdWithCounselor(trans.DocNo);
                        if (booking3 != null)
                        {
                            description = $"Bạn đã được hoàn tiền từ buổi booking tư vấn {booking3.Counselor.Fullname} vào {booking3.TimeStart?.ToString("dd/MM/yyyy HH:mm")}";
                            amount = booking3.Price ?? 0;
                        }
                        break;

                    case "4":
                        var enrollCourse4 = await _enrollCourseRepository.GetByIdWithCourseAsync(trans.DocNo);
                        if (enrollCourse4 != null)
                        {
                            description = $"Bạn đã được hoàn mua khóa học {enrollCourse4.Course.Name}";
                            amount = - enrollCourse4.Price ?? 0;
                        }
                        break;

                    case "5":
                        var membership = await _memberMemberShipRepository.GetByIdWithMemberShipAsync(trans.DocNo);
                        if (membership != null)
                        {
                            description = $"Bạn đã mua gói {membership.MemberShip.MemberShipName}";
                            amount = - membership.Price ?? 0;
                        }
                        break;

                    case "7":
                        var booking7 = await _bookingRepository.GetByIdWithMember(trans.DocNo);
                        if (booking7 != null && booking7.TimeStart.HasValue && booking7.TimeEnd.HasValue)
                        {
                            var durationMinutes = (int)(booking7.TimeEnd.Value - booking7.TimeStart.Value).TotalMinutes;
                            description = $"Bạn đã hoàn thành buổi booking với {booking7.Member.Fullname} trong thời gian {durationMinutes} phút";
                            amount = (booking7.Price * 7 / 10) ?? 0;
                        }
                        break;

                    case "8":
                        var deposit8 = await _depositRepository.GetByIdAsync(trans.DocNo);
                        if (deposit8 != null && deposit8.CreateDate.HasValue)
                        {
                            description = $"Bạn đã rút tiền vào lúc {deposit8.CreateDate?.ToString("dd/MM/yyyy HH:mm")}";
                            amount = - deposit8.Total ?? 0;
                        }
                        break;

                    case "9":
                        var deposit9 = await _depositRepository.GetByIdAsync(trans.DocNo);
                        if (deposit9 != null && deposit9.CreateDate.HasValue)
                        {
                            description = $"Bạn đã nạp tiền vào lúc {deposit9.CreateDate?.ToString("dd/MM/yyyy HH:mm")}";
                            amount = deposit9.Total ?? 0;
                        }
                        break;

                    default:
                        description = "(Loại giao dịch không xác định)";
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

            var paging = new PagingResponse<TransactionSummaryDto>(result, totalCount, request.PageNumber, request.PageSize);
            return ServiceResponse<PagingResponse<TransactionSummaryDto>>.SuccessResponse(paging);
        }
    }
}
