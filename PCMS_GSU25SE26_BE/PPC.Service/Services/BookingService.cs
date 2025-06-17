using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Repository.Repositories;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.BookingRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.BookingResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ICounselorRepository _counselorRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IMemberShipService _memberShipService;
        private readonly ISysTransactionRepository _sysTransactionRepository;
        private readonly ISubCategoryRepository _subCategoryRepository;
        private readonly IBookingSubCategoryRepository _bookingSubCategoryRepository;

        public BookingService(
            IBookingRepository bookingRepository,
            ICounselorRepository counselorRepository,
            IMemberRepository memberRepository,
            IAccountRepository accountRepository,
            IWalletRepository walletRepository,
            IMemberShipService memberShipService,
            ISysTransactionRepository sysTransactionRepository,
            ISubCategoryRepository subCategoryRepository,
            IBookingSubCategoryRepository bookingSubCategoryRepository)
        {
            _bookingRepository = bookingRepository;
            _counselorRepository = counselorRepository;
            _memberRepository = memberRepository;
            _accountRepository = accountRepository;
            _walletRepository = walletRepository;
            _memberShipService = memberShipService;
            _sysTransactionRepository = sysTransactionRepository;
            _subCategoryRepository = subCategoryRepository;
            _bookingSubCategoryRepository = bookingSubCategoryRepository;
        }

        public async Task<ServiceResponse<BookingResultDto>> BookCounselingAsync(string memberId, string accountId, BookingRequest request)
        {
            if (string.IsNullOrEmpty(memberId) || string.IsNullOrEmpty(accountId))
                return ServiceResponse<BookingResultDto>.ErrorResponse("Unauthorized");

            var member = await _memberRepository.GetByIdAsync(memberId);
            if (member == null)
                return ServiceResponse<BookingResultDto>.ErrorResponse("Member not found");

            var counselor = await _counselorRepository.GetByIdAsync(request.CounselorId);
            if (counselor == null || counselor.Status == 0)
                return ServiceResponse<BookingResultDto>.ErrorResponse("Counselor not found");

            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null || string.IsNullOrEmpty(account.WalletId))
                return ServiceResponse<BookingResultDto>.ErrorResponse("Wallet not found");

            var wallet = await _walletRepository.GetByIdAsync(account.WalletId);
            if (wallet == null || wallet.Status != 1)
                return ServiceResponse<BookingResultDto>.ErrorResponse("Wallet invalid or inactive");

            var durationMinutes = (request.TimeEnd - request.TimeStart).TotalMinutes + 10;

            var basePrice = (counselor.Price > 0 ? counselor.Price : 0) * (durationMinutes / 60.0);
            
            var discount = await _memberShipService.GetMaxBookingDiscountByMemberAsync(memberId);
            var finalPrice = basePrice * (1 - discount / 100.0);

            if ((wallet.Remaining ?? 0) < finalPrice)
                return ServiceResponse<BookingResultDto>.ErrorResponse("Not enough balance");

            // Tạo booking
            var booking = new Booking
            {
                Id = Utils.Utils.GenerateIdModel("Booking"),
                MemberId = memberId,
                CounselorId = request.CounselorId,
                TimeStart = request.TimeStart,
                TimeEnd = request.TimeEnd,
                Note = request.Note,
                Price = finalPrice,
                Status = 1,
                CreateAt = DateTime.UtcNow
            };
            await _bookingRepository.CreateAsync(booking);

            if (request.SubCategoryIds != null && request.SubCategoryIds.Any())
            {
                var subCategories = await _subCategoryRepository.GetByIdsAsync(request.SubCategoryIds);

                if (subCategories != null && subCategories.Any())
                {
                    var bookingSubCategories = new List<BookingSubCategory>();

                    foreach (var sc in subCategories)
                    {
                        if (string.IsNullOrEmpty(sc.Id) || string.IsNullOrEmpty(sc.CategoryId))
                            continue;

                        bookingSubCategories.Add(new BookingSubCategory
                        {
                            Id = Utils.Utils.GenerateIdModel("BookingSubCategory"),
                            BookingId = booking.Id,
                            SubCategoryId = sc.Id,
                            CategoryId = sc.CategoryId,
                            Status = 1
                        });
                    }

                    await _bookingSubCategoryRepository.CreateRangeAsync(bookingSubCategories);
                }
            }

            // Trừ tiền
            wallet.Remaining -= finalPrice;
            await _walletRepository.UpdateAsync(wallet);

            // Tạo transaction
            var transaction = new SysTransaction
            {
                Id = Utils.Utils.GenerateIdModel("SysTransaction"),
                TransactionType = "1",
                DocNo = booking.Id,
                CreateBy = accountId,
                CreateDate = DateTime.UtcNow
            };
            await _sysTransactionRepository.CreateAsync(transaction);

            return ServiceResponse<BookingResultDto>.SuccessResponse(new BookingResultDto
            {
                BookingId = booking.Id,
                Price = finalPrice,
                Remaining = wallet.Remaining,
                TransactionId = transaction.Id,
                Message = "Booking successful"
            });
        }
    }
}
