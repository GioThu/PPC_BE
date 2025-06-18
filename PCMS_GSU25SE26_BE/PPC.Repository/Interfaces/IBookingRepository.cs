using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface IBookingRepository : IGenericRepository<Booking>
    {
        Task<List<Booking>> GetConfirmedBookingsByDateAsync(string counselorId, DateTime workDate);
        Task<Booking> GetByIdWithCounselor(string bookingId);
        Task<List<Booking>> GetConfirmedBookingsBetweenDatesAsync(string counselorId, DateTime from, DateTime to);
        Task<List<Booking>> GetBookingsByCounselorIdAsync(string counselorId);
        Task<List<Booking>> GetBookingsByMemberIdAsync(string memberId);
        Task<Booking> GetDtoByIdAsync(string bookingId);

    }
}
