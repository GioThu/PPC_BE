using Microsoft.EntityFrameworkCore;
using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using PPC.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Repositories
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(CCPContext context) : base(context) { }

        public async Task<Booking> GetByIdWithCounselor(string bookingId)
        {
            if (string.IsNullOrWhiteSpace(bookingId))
                return null;

            var booking = await _context.Bookings
                .Include(b => b.Counselor)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null || booking.Counselor == null)
                return null;

            return booking;
        }

        public async Task<List<Booking>> GetConfirmedBookingsByDateAsync(string counselorId, DateTime workDate)
        {
            return await _context.Bookings
                .Where(b =>
                    b.CounselorId == counselorId &&
                    b.Status == 1 &&
                    b.TimeStart.HasValue &&
                    b.TimeStart.Value.Date == workDate.Date)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetConfirmedBookingsBetweenDatesAsync(string counselorId, DateTime from, DateTime to)
        {
            return await _context.Bookings
                .Where(b => b.CounselorId == counselorId
                            && b.Status == 1 
                            && b.TimeStart.HasValue
                            && b.TimeEnd.HasValue
                            && b.TimeStart.Value.Date >= from
                            && b.TimeStart.Value.Date <= to)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetBookingsByMemberIdAsync(string memberId)
        {
            return await _context.Bookings
                .Where(b => b.MemberId == memberId || b.Member2Id == memberId) 
                .Include(b => b.BookingSubCategories)  
                .ThenInclude(bsc => bsc.SubCategory)  
                .OrderByDescending(b => b.TimeStart)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetBookingsByCounselorIdAsync(string counselorId)
        {
            return await _context.Bookings
                .Where(b => b.CounselorId == counselorId)
                .Include(b => b.BookingSubCategories) 
                .ThenInclude(bsc => bsc.SubCategory)  
                .OrderByDescending(b => b.TimeStart)
                .ToListAsync();
        }
    }
}
