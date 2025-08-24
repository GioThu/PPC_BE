using AutoMapper;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.ModelRequest.BookingRequest;
using PPC.Service.ModelRequest.RoomRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.BookingResponse;
using PPC.Service.ModelResponse.RoomResponse;
using static Livekit.Server.Sdk.Dotnet.IngressState.Types;


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
        private readonly IServiceScopeFactory _scopeFactory;


        private static readonly int[] CompletedStatuses = new[] {  7 }; // Finish, Complete;
        private static readonly int[] CancellableNoCount = new[] { 4, 6 };

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
            IRoomService roomService,
            IServiceScopeFactory scopeFactory
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
            _scopeFactory = scopeFactory;
        }

        public async Task<ServiceResponse<BookingResultDto>> BookCounselingAsync(string memberId, string accountId, BookingRequest request)
        {
            if (string.IsNullOrEmpty(memberId) || string.IsNullOrEmpty(accountId))
                return ServiceResponse<BookingResultDto>.ErrorResponse("Không được phép truy cập");

            var member = await _memberRepository.GetByIdAsync(memberId);
            if (member == null)
                return ServiceResponse<BookingResultDto>.ErrorResponse("Không tìm thấy thành viên");

            var counselor = await _counselorRepository.GetByIdAsync(request.CounselorId);
            if (counselor == null || counselor.Status == 0)
                return ServiceResponse<BookingResultDto>.ErrorResponse("Không tìm thấy tư vấn viên");

            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null || string.IsNullOrEmpty(account.WalletId))
                return ServiceResponse<BookingResultDto>.ErrorResponse("Không tìm thấy ví");

            var wallet = await _walletRepository.GetByIdAsync(account.WalletId);
            if (wallet == null || wallet.Status != 1)
                return ServiceResponse<BookingResultDto>.ErrorResponse("Ví không hợp lệ hoặc đang không hoạt động");

            var durationMinutes = (request.TimeEnd - request.TimeStart).TotalMinutes + 10;

            var basePrice = (counselor.Price > 0 ? counselor.Price : 0) * (durationMinutes / 60.0);
            
            var discount = await _memberShipService.GetMaxBookingDiscountByMemberAsync(memberId);
            var finalPrice = Math.Round(basePrice * (1 - discount / 100.0), 0); 

            if ((wallet.Remaining ?? 0) < finalPrice)
                return ServiceResponse<BookingResultDto>.ErrorResponse("Số dư không đủ");

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
            var delay = booking.TimeEnd.Value - Utils.Utils.GetTimeNow();
            if (delay.TotalSeconds > 0)
            {
                BackgroundJob.Schedule<IBookingService>(
                    x => x.AutoMarkBookingAsFinished(booking.Id),
                    delay
            );
            }

            var startStr = request.TimeStart.ToString("HH:mm dd/MM/yyyy");
            var endStr = request.TimeEnd.ToString("HH:mm dd/MM/yyyy");

            NotificationBackground.FireAndForgetCreateMany(
                _scopeFactory,
                new List<NotificationCreateItem>
                {
        new NotificationCreateItem
        {
            CreatorId   = memberId,
            NotiType    = "1",
            DocNo       = booking.Id,
            Description = $"Bạn đã đặt lịch tư vấn với {counselor.Fullname} từ {startStr} đến {endStr}"
        },
        new NotificationCreateItem
        {
            CreatorId   = request.CounselorId,
            NotiType    = "1",
            DocNo       = booking.Id,
            Description = $"Bạn có lịch tư vấn mới với {member.Fullname} từ {startStr} đến {endStr}"
        }
                }
                
            );

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
                return ServiceResponse<List<BookingDto>>.ErrorResponse("Không tìm thấy lượt đặt lịch nào");

            var bookingDtos = _mapper.Map<List<BookingDto>>(bookings);
            return ServiceResponse<List<BookingDto>>.SuccessResponse(bookingDtos);
        }
        public async Task<ServiceResponse<List<BookingDto>>> GetBookingsByMemberAsync(string memberId)
        {
            var bookings = await _bookingRepository.GetBookingsByMemberIdAsync(memberId);
            if (bookings == null || !bookings.Any())
                return ServiceResponse<List<BookingDto>>.ErrorResponse("Không tìm thấy lượt đặt lịch nào");

            var bookingDtos = _mapper.Map<List<BookingDto>>(bookings);
            return ServiceResponse<List<BookingDto>>.SuccessResponse(bookingDtos);
        }
        public async Task<ServiceResponse<PagingResponse<BookingDto>>> GetBookingsByCounselorAsync(string counselorId, int pageNumber, int pageSize, int? status)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var (bookings, totalCount) = await _bookingRepository
                .GetBookingsByCounselorPagingAsync(counselorId, pageNumber, pageSize, status);

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
                return ServiceResponse<BookingDto>.ErrorResponse("Không tìm thấy lượt đặt lịch nào.");

            var bookingDto = _mapper.Map<BookingDto>(booking);
            return ServiceResponse<BookingDto>.SuccessResponse(bookingDto);
        }
        public async Task<ServiceResponse<TokenLivekit>> GetLiveKitToken(string accountId, string bookingId, int role)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return ServiceResponse<TokenLivekit>.ErrorResponse("Không tìm thấy lượt đặt lịch nào.");
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
                    return ServiceResponse<TokenLivekit>.ErrorResponse("Không tìm thấy chuyên gia tư vấn");
                }

                id = counselor.Id;
                name = counselor.Fullname;
            }
            else if (role == 3)
            {
                var member = await _memberRepository.GetByIdAsync(booking.MemberId);
                if (member == null)
                {
                    return ServiceResponse<TokenLivekit>.ErrorResponse("Không tìm thấy thành viên");
                }

                id = member.Id;
                name = member.Fullname;
            }
            else
            {
                return ServiceResponse<TokenLivekit>.ErrorResponse("Vai trò không hợp lệ");
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
                return ServiceResponse<RoomResponse>.ErrorResponse("Không tìm thấy lượt đặt lịch nào.");
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
                    return ServiceResponse<RoomResponse>.ErrorResponse("Không tìm thấy chuyên gia tư vấn.");
                }

                userName = counselor.Fullname;
            }
            else if (role == 3)
            {
                var member = await _memberRepository.GetByIdAsync(booking.MemberId);
                if (member == null)
                {
                    return ServiceResponse<RoomResponse>.ErrorResponse("Không tìm thấy thành viên");
                }

                userName = member.Fullname;
            }
            else
            {
                return ServiceResponse<RoomResponse>.ErrorResponse("Vai trò không hợp lệ");
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
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy lượt đặt lịch");

            if (status == 2)
            {
                if (booking.Status != 1)
                    return ServiceResponse<string>.ErrorResponse("Buổi đặt lịch này không còn hoạt động.");

                booking.Status = status;
            }

            else if (status == 4)
            {
                if (booking.Status != 1)
                    return ServiceResponse<string>.ErrorResponse("Buổi đặt lịch này không còn hoạt động.");

                var member = await _memberRepository.GetByIdWithWalletAsync(booking.MemberId);
                if (member?.Account?.Wallet == null)
                    return ServiceResponse<string>.ErrorResponse("Không tìm thấy thành viên");

                member.Account.Wallet.Remaining += booking.Price / 2;
                await _walletRepository.UpdateAsync(member.Account.Wallet);

                await _sysTransactionRepository.CreateAsync(new SysTransaction
                {
                    Id = Utils.Utils.GenerateIdModel("SysTransaction"),
                    TransactionType = "2",
                    DocNo = booking.Id,
                    CreateBy = member.Account.Id,
                    CreateDate = Utils.Utils.GetTimeNow()
                });

                booking.Status = status;
            }

            else if (status == 6)
            {
                var member = await _memberRepository.GetByIdWithWalletAsync(booking.MemberId);
                if (member?.Account?.Wallet == null)
                    return ServiceResponse<string>.ErrorResponse("Không tìm thấy thành viên");

                member.Account.Wallet.Remaining += booking.Price;
                await _walletRepository.UpdateAsync(member.Account.Wallet);

                await _sysTransactionRepository.CreateAsync(new SysTransaction
                {
                    Id = Utils.Utils.GenerateIdModel("SysTransaction"),
                    TransactionType = "3",
                    DocNo = booking.Id,
                    CreateBy = member.Account.Id,
                    CreateDate = Utils.Utils.GetTimeNow()
                });

                booking.Status = status;
            }

            else if (status == 7)
            {
                var counselor = await _counselorRepository.GetByIdWithWalletAsync(booking.CounselorId);
                if (counselor?.Account?.Wallet == null)
                    return ServiceResponse<string>.ErrorResponse("Không tìm thấy chuyên gia tư vấn");

                counselor.Account.Wallet.Remaining += booking.Price * 7 / 10;
                await _walletRepository.UpdateAsync(counselor.Account.Wallet);

                await _sysTransactionRepository.CreateAsync(new SysTransaction
                {
                    Id = Utils.Utils.GenerateIdModel("SysTransaction"),
                    TransactionType = "7",
                    DocNo = booking.Id,
                    CreateBy = counselor.Account.Id,
                    CreateDate = Utils.Utils.GetTimeNow()
                });

                booking.Status = status;
            }

            await _bookingRepository.UpdateAsync(booking);

            if (booking.Status == 2)
            {
                BackgroundJob.Schedule<IBookingService>(
                    x => x.AutoCompleteBookingIfStillPending(booking.Id),
                    TimeSpan.FromDays(1)
                );
                return ServiceResponse<string>.SuccessResponse("Booking ended");
            }

            return booking.Status switch
            {
                4 => ServiceResponse<string>.SuccessResponse("Đặt lịch đã được hủy thành công"),
                6 => ServiceResponse<string>.SuccessResponse("Hoàn tiền cho lượt đặt lịch thành công"),
                7 => ServiceResponse<string>.SuccessResponse("Lượt đặt lịch đã hoàn tất thành công"),
                _ => ServiceResponse<string>.SuccessResponse("Trạng thái đặt lịch đã được cập nhật thành công")
            };
        }
        public async Task<ServiceResponse<string>> ReportBookingAsync(BookingReportRequest request)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy lượt đặt lịch");

            booking.IsReport = true;
            booking.ReportMessage = request.ReportMessage;
            booking.Status = 5;

            var result = await _bookingRepository.UpdateAsync(booking);
            if (result == 0)
                return ServiceResponse<string>.ErrorResponse("Báo cáo thất bại");

            return ServiceResponse<string>.SuccessResponse("Báo cáo thành công");
        }
        public async Task AutoCompleteBookingIfStillPending(string bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null || booking.Status != 2)
                return;

            var counselor = await _counselorRepository.GetByIdWithWalletAsync(booking.CounselorId);
            if (counselor?.Account?.Wallet == null)
                return; 

            var amountToAdd = booking.Price * 7 / 10;
            counselor.Account.Wallet.Remaining += amountToAdd;
            await _walletRepository.UpdateAsync(counselor.Account.Wallet);

            var transaction = new SysTransaction
            {
                Id = Utils.Utils.GenerateIdModel("SysTransaction"),
                TransactionType = "7",
                DocNo = booking.Id,
                CreateBy = counselor.Account.Id,
                CreateDate = Utils.Utils.GetTimeNow()
            };
            await _sysTransactionRepository.CreateAsync(transaction);

            booking.Status = 7;
            await _bookingRepository.UpdateAsync(booking);
        }
        public async Task AutoMarkBookingAsFinished(string bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                return;
            if (booking.Status == 1)
            {
                booking.Status = 2; 
                await _bookingRepository.UpdateAsync(booking);
            }
            BackgroundJob.Schedule<IBookingService>(
                    x => x.AutoCompleteBookingIfStillPending(booking.Id),
                    TimeSpan.FromDays(1)
                );
        }
        public async Task<ServiceResponse<string>> CancelByCounselorAsync(CancelBookingByCounselorRequest request)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy buổi đặt lịch");

            if (booking.Status != 1)
                return ServiceResponse<string>.ErrorResponse("Không thể hủy buổi đặt lịch này");

            booking.CancelReason = request.CancelReason;
            booking.Status = 6;

            var member = await _memberRepository.GetByIdWithWalletAsync(booking.MemberId);
            if (member?.Account?.Wallet == null)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy người dùng");

            member.Account.Wallet.Remaining += booking.Price;
            var walletUpdateResult = await _walletRepository.UpdateAsync(member.Account.Wallet);
            if (walletUpdateResult == 0)
                return ServiceResponse<string>.ErrorResponse("Không thể hoàn trả giao dịch này");

            var transaction = new SysTransaction
            {
                Id = Utils.Utils.GenerateIdModel("SysTransaction"),
                TransactionType = "3", 
                DocNo = booking.Id,
                CreateBy = member.Account.Id,
                CreateDate = Utils.Utils.GetTimeNow()
            };
            await _sysTransactionRepository.CreateAsync(transaction);

            // Cập nhật trạng thái booking
            var result = await _bookingRepository.UpdateAsync(booking);
            if (result == 0)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy buổi đặt lịch");

            return ServiceResponse<string>.SuccessResponse("Buổi đặt lịch đã được hủy bởi chuyên gia tư vấn");
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
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy buổi đặt lịch");

            booking.ProblemSummary = request.ProblemSummary;
            booking.ProblemAnalysis = request.ProblemAnalysis;
            booking.Guides = request.Guides;

            var result = await _bookingRepository.UpdateAsync(booking);
            if (result == 0)
                return ServiceResponse<string>.ErrorResponse("Không thể cập nhật ghi chú đặt lịch");

            return ServiceResponse<string>.SuccessResponse("Ghi chú đặt lịch đã được cập nhật thành công");
        }
        public async Task<ServiceResponse<string>> RateBookingAsync(BookingRatingRequest request)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy buổi đặt lịch");

            if (booking.Status == 1 || booking.Status == 3 || booking.Status == 4 || booking.Status == 6)
                return ServiceResponse<string>.ErrorResponse("Chỉ những buổi đặt lịch đã hoàn tất mới có thể được đánh giá.");


            booking.Rating = request.Rating;
            booking.Feedback = request.Feedback;
            await _bookingRepository.UpdateAsync(booking);

            // Cập nhật Counselor.Rating trung bình & số lượng đánh giá
            var counselor = await _counselorRepository.GetByIdAsync(booking.CounselorId);
            if (counselor == null)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy chuyên viên tư vấn");

            var (average, count) = await _bookingRepository.GetRatingStatsByCounselorIdAsync(counselor.Id);
            counselor.Rating = average;
            counselor.Reviews = count;

            await _counselorRepository.UpdateAsync(counselor);


            return ServiceResponse<string>.SuccessResponse("Đánh giá đã được gửi thành công");
        }
        public async Task<ServiceResponse<List<BookingRatingFeedbackDto>>> GetRatingFeedbackByCounselorAsync(string counselorId)
        {
            var bookings = await _bookingRepository.GetRatedBookingsByCounselorAsync(counselorId);

            var result = bookings.Select(b => new BookingRatingFeedbackDto
            {
                Rating = b.Rating.Value,
                Feedback = b.Feedback,
                TimeEnd = b.TimeEnd,
                MemberFullName = b.Member?.Fullname ?? "Ẩn danh"
            }).ToList();

            return ServiceResponse<List<BookingRatingFeedbackDto>>.SuccessResponse(result);
        }
        public async Task<ServiceResponse<int>> GetMaxBookingDiscountByMemberWrappedAsync(string memberId)
        {
            var discount = await _memberShipService.GetMaxBookingDiscountByMemberAsync(memberId); 
            return ServiceResponse<int>.SuccessResponse(discount);
        }

        public async Task<ServiceResponse<string>> UpdateMember2Async(string bookingId, string memberCode)
        {
            var fullMemberId = $"Member_{memberCode}";

            var isMemberValid = await _memberRepository.IsMemberExistsAsync(fullMemberId);
            if (!isMemberValid)
            {
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy người dùng");
            }

            try
            {
                await _bookingRepository.UpdateMember2Async(bookingId, memberCode);
                return ServiceResponse<string>.SuccessResponse("Đã cập nhật Người dùng 2 thành công");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.ErrorResponse(ex.Message);
            }
        }

        public async Task<ServiceResponse<List<BookingDto>>> GetInvitationsForMemberAsync(string memberId)
        {
            var bookings = await _bookingRepository.GetInvitationsForMemberAsync(memberId);
            var bookingDtos = _mapper.Map<List<BookingDto>>(bookings);
            return ServiceResponse<List<BookingDto>>.SuccessResponse(bookingDtos);
        }

        public async Task<ServiceResponse<string>> AcceptInvitationAsync(string bookingId, string memberId)
        {
            var success = await _bookingRepository.AcceptInvitationAsync(bookingId, memberId);
            if (!success)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy lời mời");

            return ServiceResponse<string>.SuccessResponse("Chấp nhận lời mời thành công");
        }

        public async Task<ServiceResponse<string>> DeclineInvitationAsync(string bookingId, string memberId)
        {
            var success = await _bookingRepository.DeclineInvitationAsync(bookingId, memberId);
            if (!success)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy lời mời");

            return ServiceResponse<string>.SuccessResponse("Từ chối lời mời thành công");
        }

        public async Task<ServiceResponse<string>> CancelInvitationAsync(string bookingId, string creatorMemberId)
        {
            var success = await _bookingRepository.CancelInvitationAsync(bookingId, creatorMemberId);
            if (!success)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy lời mời");

            return ServiceResponse<string>.SuccessResponse("Lời mời đã bị hủy");
        }


        public async Task<ServiceResponse<DashboardDto>> GetMyDashboardAsync(string counselorId)
        {
            var now = Utils.Utils.GetTimeNow();
            var startOfWeek = StartOfWeek(now, DayOfWeek.Monday).Date; // Fixed: Correctly call the method StartOfWeek
            var endOfWeek = startOfWeek.AddDays(7);

            var startOfYear = new DateTime(now.Year, 1, 1);
            var startOfNextYear = new DateTime(now.Year + 1, 1, 1);

            // Lấy dữ liệu năm hiện tại cho biểu đồ/thống kê theo tuần
            var bookingsYear = await _bookingRepository.GetByCounselorAsync(counselorId, startOfYear, startOfNextYear);
            var bookingsWeek = bookingsYear.Where(b => b.TimeStart >= startOfWeek && b.TimeStart < endOfWeek).ToList();

            // >>> NEW: Lấy toàn bộ booking để tính "Tổng thu nhập"
            var allBookings = await _bookingRepository.GetByCounselorAsync(counselorId);

            var mappedYear = _mapper.Map<List<BookingDashboardDto>>(bookingsYear);
            var mappedWeek = _mapper.Map<List<BookingDashboardDto>>(bookingsWeek);
            var mappedAll = _mapper.Map<List<BookingDashboardDto>>(allBookings);

            // >>> Đổi rule: tổng thu nhập = tổng Price của TẤT CẢ booking (không loại trừ trạng thái nào)
            double totalIncome = mappedAll.Sum(b => b.Price ?? 0);

            // (giữ nguyên các phần còn lại)
            int apptThisWeek = mappedWeek.Count(b => !(b.Status.HasValue && CancellableNoCount.Contains(b.Status.Value)));
            int completedSessions = mappedYear.Count(b => b.Status.HasValue && CompletedStatuses.Contains(b.Status.Value));
            var rated = mappedYear.Where(b => b.Rating.HasValue).Select(b => b.Rating.Value).ToList();
            double avgRating = rated.Count == 0 ? 0 : Math.Round(rated.Average(), 1);

            var monthlyIncome = Enumerable.Range(1, 12).Select(m => new MonthlyIncomePointDto
            {
                Month = m,
                Income = mappedYear
                    .Where(b => b.TimeStart.HasValue &&
                                b.TimeStart.Value.Month == m &&
                                b.TimeStart.Value.Year == now.Year &&
                                b.Status.HasValue && CompletedStatuses.Contains(b.Status.Value))
                    .Sum(b => b.Price ?? 0)
            }).ToList();

            var weeklyAppointments = Enumerable.Range(1, 7).Select(d => new WeekdayCountDto
            {
                DayOfWeek = d,
                Count = mappedWeek.Count(b =>
                    b.TimeStart.HasValue &&
                    ToIsoDayOfWeek(b.TimeStart.Value.DayOfWeek) == d &&
                    !(b.Status.HasValue && CancellableNoCount.Contains(b.Status.Value)))
            }).ToList();

            var dto = new DashboardDto
            {
                TotalIncome = totalIncome,
                AppointmentsThisWeek = apptThisWeek,
                CompletedSessions = completedSessions,
                AverageRating = avgRating,
                MonthlyIncome = monthlyIncome,
                WeeklyAppointments = weeklyAppointments
            };

            return ServiceResponse<DashboardDto>.SuccessResponse(dto);
        }

        private DateTime StartOfWeek(DateTime date, DayOfWeek startDay)
        {
            int diff = (7 + (date.DayOfWeek - startDay)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        private static int ToIsoDayOfWeek(DayOfWeek d)
        {
            // ISO: Monday=1 ... Sunday=7
            return d == DayOfWeek.Sunday ? 7 : (int)d;
        }
    }
}
