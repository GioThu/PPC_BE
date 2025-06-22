using AutoMapper;
using Hangfire;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Repository.Repositories;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.BookingRequest;
using PPC.Service.ModelRequest.RoomRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.BookingResponse;
using PPC.Service.ModelResponse.CounselorResponse;
using PPC.Service.ModelResponse.MemberResponse;
using PPC.Service.ModelResponse.RoomResponse;
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
        private readonly IMapper _mapper;
        private readonly ILiveKitService _liveKitService;
        private readonly IRoomService _roomService;

        public BookingService(
            IBookingRepository bookingRepository,
            ICounselorRepository counselorRepository,
            IMemberRepository memberRepository,
            IAccountRepository accountRepository,
            IWalletRepository walletRepository,
            IMemberShipService memberShipService,
            ISysTransactionRepository sysTransactionRepository,
            ISubCategoryRepository subCategoryRepository,
            IBookingSubCategoryRepository bookingSubCategoryRepository,
            IMapper mapper,
            ILiveKitService liveKitService,
            IRoomService roomService
          )
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
            _mapper = mapper;
            _liveKitService = liveKitService;
            _roomService = roomService;
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
                CreateAt = Utils.Utils.GetTimeNow(),
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
                CreateDate = Utils.Utils.GetTimeNow()
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
        public async Task<ServiceResponse<List<BookingDto>>> GetBookingsByCounselorAsync(string counselorId)
        {
            var bookings = await _bookingRepository.GetBookingsByCounselorIdAsync(counselorId);
            if (bookings == null || !bookings.Any())
                return ServiceResponse<List<BookingDto>>.ErrorResponse("No bookings found.");

            var bookingDtos = _mapper.Map<List<BookingDto>>(bookings);
            return ServiceResponse<List<BookingDto>>.SuccessResponse(bookingDtos);
        }
        public async Task<ServiceResponse<List<BookingDto>>> GetBookingsByMemberAsync(string memberId)
        {
            var bookings = await _bookingRepository.GetBookingsByMemberIdAsync(memberId);
            if (bookings == null || !bookings.Any())
                return ServiceResponse<List<BookingDto>>.ErrorResponse("No bookings found.");

            var bookingDtos = _mapper.Map<List<BookingDto>>(bookings);
            return ServiceResponse<List<BookingDto>>.SuccessResponse(bookingDtos);
        }
        public async Task<ServiceResponse<PagingResponse<BookingDto>>> GetBookingsByCounselorAsync(string counselorId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var (bookings, totalCount) = await _bookingRepository
                .GetBookingsByCounselorPagingAsync(counselorId, pageNumber, pageSize);

            var bookingDtos = _mapper.Map<List<BookingDto>>(bookings);

            var pagingResponse = new PagingResponse<BookingDto>(bookingDtos, totalCount, pageNumber, pageSize);
            return ServiceResponse<PagingResponse<BookingDto>>.SuccessResponse(pagingResponse);
        }
        public async Task<ServiceResponse<PagingResponse<BookingDto>>> GetBookingsByMemberAsync(string memberId, int pageNumber, int pageSize, int? status)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var (bookings, totalCount) = await _bookingRepository.GetBookingsByMemberPagingAsync(memberId, pageNumber, pageSize, status);
            var bookingDtos = _mapper.Map<List<BookingDto>>(bookings);

            var paging = new PagingResponse<BookingDto>(bookingDtos, totalCount, pageNumber, pageSize);
            return ServiceResponse<PagingResponse<BookingDto>>.SuccessResponse(paging);
        }
        public async Task<ServiceResponse<BookingDto>> GetBookingByIdAsync(string bookingId)
        {
            var booking = await _bookingRepository.GetDtoByIdAsync(bookingId);
            if (booking == null)
                return ServiceResponse<BookingDto>.ErrorResponse("Booking not found.");

            var bookingDto = _mapper.Map<BookingDto>(booking);
            return ServiceResponse<BookingDto>.SuccessResponse(bookingDto);
        }
        public async Task<ServiceResponse<TokenLivekit>> GetLiveKitToken(string accountId, string bookingId, int role)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return ServiceResponse<TokenLivekit>.ErrorResponse("Booking not found.");
            }

            string room = $"room_{bookingId}";
            string id = string.Empty;
            string name = string.Empty;
            DateTime startTime = booking.TimeStart ?? Utils.Utils.GetTimeNow();
            DateTime endTime = booking.TimeEnd ?? Utils.Utils.GetTimeNow().AddHours(1);

            if (role == 2)
            {
                var counselor = await _counselorRepository.GetByIdAsync(booking.CounselorId);
                if (counselor == null)
                {
                    return ServiceResponse<TokenLivekit>.ErrorResponse("Counselor not found.");
                }

                id = counselor.Id;
                name = counselor.Fullname;
            }
            else if (role == 3)
            {
                var member = await _memberRepository.GetByIdAsync(booking.MemberId);
                if (member == null)
                {
                    return ServiceResponse<TokenLivekit>.ErrorResponse("Member not found.");
                }

                id = member.Id;
                name = member.Fullname;
            }
            else
            {
                return ServiceResponse<TokenLivekit>.ErrorResponse("Invalid role.");
            }

            var token = _liveKitService.GenerateLiveKitToken(room, id, name, startTime, endTime);

            var tokenLivekitResponse = new TokenLivekit(token);
            return ServiceResponse<TokenLivekit>.SuccessResponse(tokenLivekitResponse);
        }
        public async Task<bool> CheckIfMemberCanAccessBooking(string bookingId, string memberId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                return false;
            return booking.MemberId == memberId || booking.Member2Id == memberId;
        }
        public async Task<bool> CheckIfCounselorCanAccessBooking(string bookingId,string counselorId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                return false;
            return booking.CounselorId == counselorId;
        }
        public async Task<ServiceResponse<RoomResponse>> CreateDailyRoomAsync(string accountId, string bookingId, int role)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return ServiceResponse<RoomResponse>.ErrorResponse("Booking not found.");
            }

            string roomName = $"room_{bookingId}";
            string userName = string.Empty;
            DateTime startTime = booking.TimeStart ?? Utils.Utils.GetTimeNow();
            DateTime endTime = booking.TimeEnd ?? Utils.Utils.GetTimeNow().AddHours(1);

            if (role == 2)
            {
                var counselor = await _counselorRepository.GetByIdAsync(booking.CounselorId);
                if (counselor == null)
                {
                    return ServiceResponse<RoomResponse>.ErrorResponse("Counselor not found.");
                }

                userName = counselor.Fullname;
            }
            else if (role == 3)
            {
                var member = await _memberRepository.GetByIdAsync(booking.MemberId);
                if (member == null)
                {
                    return ServiceResponse<RoomResponse>.ErrorResponse("Member not found.");
                }

                userName = member.Fullname;
            }
            else
            {
                return ServiceResponse<RoomResponse>.ErrorResponse("Invalid role.");
            }

            var request = new CreateRoomRequest2
            {
                ApiKey = "106bf9f6fac65aab09b8572ca4c634305061956886d371fafc5c901e6cf74e0f", 
                RoomName = roomName,
                UserName = userName,
                StartTime = startTime,
                EndTime = endTime
            };

            var result = await _roomService.CreateRoomAsync(request); 
            return result;
        }
        public async Task<ServiceResponse<string>> ChangeStatusBookingAsync(string bookingId, int status)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                return ServiceResponse<string>.ErrorResponse("Booking not found.");

            if (booking.Status != 1)
                return ServiceResponse<string>.ErrorResponse("Buổi đặt lịch này không còn hoạt động");


            booking.Status = status;

            var result = await _bookingRepository.UpdateAsync(booking);
            if (result == 0)
                return ServiceResponse<string>.ErrorResponse("Update failed.");

            if (booking.Status == 2)
            {
                BackgroundJob.Schedule<IBookingService>(
                    x => x.AutoCompleteBookingIfStillPending(booking.Id),
                    TimeSpan.FromDays(1)
                );
            }

            if (booking.Status == 4)
            {
                return ServiceResponse<string>.SuccessResponse("Booking ended successfully.");
            }

            return ServiceResponse<string>.SuccessResponse("Booking ended successfully.");
        }
        public async Task<ServiceResponse<string>> ReportBookingAsync(BookingReportRequest request)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
                return ServiceResponse<string>.ErrorResponse("Booking not found.");

            booking.IsReport = true;
            booking.ReportMessage = request.ReportMessage;
            booking.Status = 5;

            var result = await _bookingRepository.UpdateAsync(booking);
            if (result == 0)
                return ServiceResponse<string>.ErrorResponse("Failed to report booking.");

            return ServiceResponse<string>.SuccessResponse("Booking reported successfully.");
        }
        public async Task AutoCompleteBookingIfStillPending(string bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking != null && booking.Status == 2)
            {
                booking.Status = 7;
                await _bookingRepository.UpdateAsync(booking);
            }
        }
        public async Task<ServiceResponse<string>> CancelByCounselorAsync(CancelBookingByCounselorRequest request)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
                return ServiceResponse<string>.ErrorResponse("Booking not found");

            if (booking.Status != 2)
                return ServiceResponse<string>.ErrorResponse("Không thể hủy booking này");

            booking.CancelReason = request.CancelReason;
            booking.Status = 6;

            var result = await _bookingRepository.UpdateAsync(booking);
            if (result == 0)
                return ServiceResponse<string>.ErrorResponse("Failed to cancel booking.");

            return ServiceResponse<string>.SuccessResponse("Booking cancelled by counselor.");
        }
        public async Task<ServiceResponse<PagingResponse<BookingAdminResponse>>> GetAllAdminPagingAsync(BookingPagingRequest request)
        {
            var (bookings, total) = await _bookingRepository.GetAllPagingIncludeAsync(request.PageNumber, request.PageSize, request.Status);
            var responses = _mapper.Map<List<BookingAdminResponse>>(bookings);

            var result = new PagingResponse<BookingAdminResponse>(responses, total, request.PageNumber, request.PageSize);
            return ServiceResponse<PagingResponse<BookingAdminResponse>>.SuccessResponse(result);
        }
        public async Task<ServiceResponse<string>> UpdateBookingNoteAsync(BookingNoteUpdateRequest request)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
                return ServiceResponse<string>.ErrorResponse("Booking not found.");

            booking.ProblemSummary = request.ProblemSummary;
            booking.ProblemAnalysis = request.ProblemAnalysis;
            booking.Guides = request.Guides;

            var result = await _bookingRepository.UpdateAsync(booking);
            if (result == 0)
                return ServiceResponse<string>.ErrorResponse("Failed to update booking notes.");

            return ServiceResponse<string>.SuccessResponse("Booking notes updated successfully.");
        }
        public async Task<ServiceResponse<string>> RateBookingAsync(BookingRatingRequest request)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
                return ServiceResponse<string>.ErrorResponse("Booking not found.");

            if (booking.Status == 1 || booking.Status == 3 || booking.Status == 4 || booking.Status == 6)
                return ServiceResponse<string>.ErrorResponse("Only completed bookings can be rated.");


            booking.Rating = request.Rating;
            booking.Feedback = request.Feedback;
            await _bookingRepository.UpdateAsync(booking);

            // Cập nhật Counselor.Rating trung bình & số lượng đánh giá
            var counselor = await _counselorRepository.GetByIdAsync(booking.CounselorId);
            if (counselor == null)
                return ServiceResponse<string>.ErrorResponse("Counselor not found.");

            var (average, count) = await _bookingRepository.GetRatingStatsByCounselorIdAsync(counselor.Id);
            counselor.Rating = average;
            counselor.Reviews = count;

            await _counselorRepository.UpdateAsync(counselor);

            return ServiceResponse<string>.SuccessResponse("Rating submitted successfully.");
        }
    }
}
