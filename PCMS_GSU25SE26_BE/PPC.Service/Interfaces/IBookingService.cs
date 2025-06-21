using PPC.Service.ModelRequest.BookingRequest;
using PPC.Service.ModelRequest.CategoryRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.BookingResponse;
using PPC.Service.ModelResponse.CategoryResponse;
using PPC.Service.ModelResponse.RoomResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface IBookingService
    {
        Task<ServiceResponse<BookingResultDto>> BookCounselingAsync(string memberId, string accountId, BookingRequest request);
        Task<ServiceResponse<List<BookingDto>>> GetBookingsByCounselorAsync(string counselorId);
        Task<ServiceResponse<List<BookingDto>>> GetBookingsByMemberAsync(string memberId);
        Task<ServiceResponse<PagingResponse<BookingDto>>> GetBookingsByMemberAsync(string memberId, int pageNumber, int pageSize);
        Task<ServiceResponse<PagingResponse<BookingDto>>> GetBookingsByCounselorAsync(string counselorId, int pageNumber, int pageSize);
        Task<ServiceResponse<TokenLivekit>> GetLiveKitToken(string accountId, string bookingId, int role);
        Task<ServiceResponse<BookingDto>> GetBookingByIdAsync(string bookingId);
        Task<bool> CheckIfCounselorCanAccessBooking(string bookingId, string counselorId);
        Task<bool> CheckIfMemberCanAccessBooking(string bookingId, string counselorId);
        Task<ServiceResponse<RoomResponse>> CreateDailyRoomAsync(string accountId, string bookingId, int role);
        Task<ServiceResponse<string>> ChangeStatusBookingAsync(string bookingId, int status);
        Task<ServiceResponse<string>> ReportBookingAsync(BookingReportRequest request);
        Task AutoCompleteBookingIfStillPending(string bookingId);
        Task<ServiceResponse<string>> CancelByCounselorAsync(CancelBookingByCounselorRequest request);
        Task<ServiceResponse<PagingResponse<BookingAdminResponse>>> GetAllAdminPagingAsync(BookingPagingRequest request);
        Task<ServiceResponse<string>> UpdateBookingNoteAsync(BookingNoteUpdateRequest request);


    }
}
