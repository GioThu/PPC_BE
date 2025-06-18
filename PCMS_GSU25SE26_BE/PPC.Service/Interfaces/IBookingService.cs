using PPC.Service.ModelRequest.BookingRequest;
using PPC.Service.ModelRequest.CategoryRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.BookingResponse;
using PPC.Service.ModelResponse.CategoryResponse;
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
        Task<ServiceResponse<string>> GetLiveKitToken(string accountId, string bookingId, int role);
    }
}
