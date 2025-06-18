using AutoMapper;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Repository.Repositories;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.BookingRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.BookingResponse;
using PPC.Service.ModelResponse.CounselorResponse;
using PPC.Service.ModelResponse.MemberResponse;
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
            ILiveKitService liveKitService
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
            {
                return ServiceResponse<List<BookingDto>>.ErrorResponse("No bookings found.");
            }

            var bookingDtos = new List<BookingDto>();

            foreach (var booking in bookings)
            {
                var dto = _mapper.Map<BookingDto>(booking);

                var member = await _memberRepository.GetByIdAsync(booking.MemberId);
                dto.Member = _mapper.Map<MemberDto>(member);

                if (!string.IsNullOrEmpty(booking.Member2Id))
                {
                    var member2 = await _memberRepository.GetByIdAsync(booking.Member2Id);
                    dto.Member2 = _mapper.Map<MemberDto>(member2);
                }

                var counselor = await _counselorRepository.GetByIdAsync(booking.CounselorId);
                dto.Counselor = _mapper.Map<CounselorDto>(counselor);

                bookingDtos.Add(dto);
            }

            return ServiceResponse<List<BookingDto>>.SuccessResponse(bookingDtos);
        }
        public async Task<ServiceResponse<List<BookingDto>>> GetBookingsByMemberAsync(string memberId)
        {
            var bookings = await _bookingRepository.GetBookingsByMemberIdAsync(memberId);
            if (bookings == null || !bookings.Any())
            {
                return ServiceResponse<List<BookingDto>>.ErrorResponse("No bookings found.");
            }

            var bookingDtos = new List<BookingDto>();

            foreach (var booking in bookings)
            {
                var bookingDto = _mapper.Map<BookingDto>(booking);

                var member = await _memberRepository.GetByIdAsync(booking.MemberId);
                if (member == null)
                    continue;

                bookingDto.Member = _mapper.Map<MemberDto>(member);

                var counselor = await _counselorRepository.GetByIdAsync(booking.CounselorId);
                if (counselor == null)
                    continue;

                bookingDto.Counselor = _mapper.Map<CounselorDto>(counselor);

                bookingDtos.Add(bookingDto);
            }

            return ServiceResponse<List<BookingDto>>.SuccessResponse(bookingDtos);
        }
        public async Task<ServiceResponse<PagingResponse<BookingDto>>> GetBookingsByCounselorAsync(string counselorId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;  

            var bookings = await _bookingRepository.GetBookingsByCounselorIdAsync(counselorId);
            var totalCount = bookings.Count();  

            var pagedBookings = bookings
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var bookingDtos = new List<BookingDto>();

            foreach (var booking in pagedBookings)
            {
                var dto = _mapper.Map<BookingDto>(booking);

                var member = await _memberRepository.GetByIdAsync(booking.MemberId);
                dto.Member = _mapper.Map<MemberDto>(member);

                if (!string.IsNullOrEmpty(booking.Member2Id))
                {
                    var member2 = await _memberRepository.GetByIdAsync(booking.Member2Id);
                    dto.Member2 = _mapper.Map<MemberDto>(member2);
                }

                var counselor = await _counselorRepository.GetByIdAsync(booking.CounselorId);
                dto.Counselor = _mapper.Map<CounselorDto>(counselor);

                bookingDtos.Add(dto);
            }

            var pagingResponse = new PagingResponse<BookingDto>(bookingDtos, totalCount, pageNumber, pageSize);

            return ServiceResponse<PagingResponse<BookingDto>>.SuccessResponse(pagingResponse);
        }
        public async Task<ServiceResponse<PagingResponse<BookingDto>>> GetBookingsByMemberAsync(string memberId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;  

            var bookings = await _bookingRepository.GetBookingsByMemberIdAsync(memberId);
            var totalCount = bookings.Count();  

            var pagedBookings = bookings
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var bookingDtos = new List<BookingDto>();

            foreach (var booking in pagedBookings)
            {
                var bookingDto = _mapper.Map<BookingDto>(booking);

                var member = await _memberRepository.GetByIdAsync(booking.MemberId);
                if (member == null) continue;

                bookingDto.Member = _mapper.Map<MemberDto>(member);

                var counselor = await _counselorRepository.GetByIdAsync(booking.CounselorId);
                if (counselor == null) continue;

                bookingDto.Counselor = _mapper.Map<CounselorDto>(counselor);

                bookingDtos.Add(bookingDto);
            }

            var pagingResponse = new PagingResponse<BookingDto>(bookingDtos, totalCount, pageNumber, pageSize);

            return ServiceResponse<PagingResponse<BookingDto>>.SuccessResponse(pagingResponse);
        }
        public async Task<ServiceResponse<BookingDto>> GetBookingByIdAsync(string bookingId)
        {
            var booking = await _bookingRepository.GetDtoByIdAsync(bookingId);
            if (booking == null)
            {
                return ServiceResponse<BookingDto>.ErrorResponse("Booking not found.");
            }

            var bookingDto = _mapper.Map<BookingDto>(booking);

            var member = await _memberRepository.GetByIdAsync(booking.MemberId);
            bookingDto.Member = _mapper.Map<MemberDto>(member);

            if (!string.IsNullOrEmpty(booking.Member2Id))
            {
                var member2 = await _memberRepository.GetByIdAsync(booking.Member2Id);
                bookingDto.Member2 = _mapper.Map<MemberDto>(member2);
            }

            var counselor = await _counselorRepository.GetByIdAsync(booking.CounselorId);
            bookingDto.Counselor = _mapper.Map<CounselorDto>(counselor);

            return ServiceResponse<BookingDto>.SuccessResponse(bookingDto);
        }
        public async Task<ServiceResponse<string>> GetLiveKitToken(string accountId, string bookingId, int role)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return ServiceResponse<string>.ErrorResponse("Booking not found.");
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
                    return ServiceResponse<string>.ErrorResponse("Counselor not found.");
                }

                id = counselor.Id;  
                name = counselor.Fullname;  
            }
            else if (role == 3)
            {
                var member = await _memberRepository.GetByIdAsync(booking.MemberId);
                if (member == null)
                {
                    return ServiceResponse<string>.ErrorResponse("Member not found.");
                }

                id = member.Id;  
                name = member.Fullname;  
            }
            else
            {
                return ServiceResponse<string>.ErrorResponse("Invalid role.");
            }

            var token = _liveKitService.GenerateLiveKitToken(room, id, name, startTime, endTime);

            return ServiceResponse<string>.SuccessResponse(token);
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
    }
}
