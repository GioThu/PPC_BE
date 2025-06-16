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
    }
}
