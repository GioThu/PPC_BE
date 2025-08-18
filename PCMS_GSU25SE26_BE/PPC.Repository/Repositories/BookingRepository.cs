using Microsoft.EntityFrameworkCore;
using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using PPC.Repository.Interfaces;


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
                .Where(b => b.MemberId == memberId || (b.Member2Id == memberId && b.IsCouple == true))
                .Include(b => b.Member)
        .Include(b => b.Member2)
        .Include(b => b.Counselor)
        .Include(b => b.BookingSubCategories)
            .ThenInclude(bsc => bsc.SubCategory)
        .OrderByDescending(b => b.TimeStart)
        .ToListAsync();
        }

        public async Task<List<Booking>> GetBookingsByCounselorIdAsync(string counselorId)
        {
            return await _context.Bookings
        .Where(b => b.CounselorId == counselorId)
        .Include(b => b.Member)
        .Include(b => b.Member2)
        .Include(b => b.Counselor)
        .Include(b => b.BookingSubCategories)
            .ThenInclude(bsc => bsc.SubCategory)
        .OrderByDescending(b => b.TimeStart)
        .ToListAsync();
        }

        public async Task<Booking> GetDtoByIdAsync(string bookingId)
        {
            return await _context.Bookings
       .Include(b => b.Member)
       .Include(b => b.Member2)
       .Include(b => b.Counselor)
       .Include(b => b.BookingSubCategories)
           .ThenInclude(bsc => bsc.SubCategory)
       .FirstOrDefaultAsync(b => b.Id == bookingId);
        }

        public async Task<(List<Booking>, int)> GetAllPagingIncludeAsync(int page, int size, int? status)
        {
            var query = _context.Bookings
                .Include(b => b.Member)
                .Include(b => b.Member2)
                .Include(b => b.Counselor)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(b => b.Status == status);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(b => b.CreateAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return (items, total);
        }

        public async Task<(List<Booking>, int)> GetBookingsByCounselorPagingAsync(string counselorId, int pageNumber, int pageSize)
        {
            var query = _context.Bookings
                .Where(b => b.CounselorId == counselorId)
                .Include(b => b.Member)
                .Include(b => b.Member2)
                .Include(b => b.Counselor)
                .Include(b => b.BookingSubCategories)
                    .ThenInclude(bsc => bsc.SubCategory)
                .OrderByDescending(b => b.TimeStart);

            var totalCount = await query.CountAsync();

            var paged = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (paged, totalCount);
        }

        public async Task<(List<Booking>, int)> GetBookingsByMemberPagingAsync(string memberId, int pageNumber, int pageSize)
        {
            var query = _context.Bookings
                .Where(b => b.MemberId == memberId || b.Member2Id == memberId && b.IsCouple == true)
                .Include(b => b.Member)
                .Include(b => b.Member2)
                .Include(b => b.Counselor)
                .Include(b => b.BookingSubCategories)
                    .ThenInclude(bsc => bsc.SubCategory)
                .OrderByDescending(b => b.TimeStart);

            var totalCount = await query.CountAsync();

            var paged = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (paged, totalCount);
        }

        public async Task<(double average, int count)> GetRatingStatsByCounselorIdAsync(string counselorId)
        {
            var query = _context.Bookings
                .Where(b => b.CounselorId == counselorId && b.Rating.HasValue);

            var count = await query.CountAsync();
            var average = count > 0 ? await query.AverageAsync(b => b.Rating.Value) : 0;

            return (average, count);
        }

        public async Task<(List<Booking>, int)> GetBookingsByMemberPagingAsync(string memberId, int pageNumber, int pageSize, int? status)
        {
            var query = _context.Bookings
                .Where(b => (b.MemberId == memberId || b.Member2Id == memberId && b.IsCouple == true));

            if (status.HasValue)
                query = query.Where(b => b.Status == status);

            query = query
                .Include(b => b.Member)
                .Include(b => b.Member2)
                .Include(b => b.Counselor)
                .Include(b => b.BookingSubCategories)
                    .ThenInclude(bsc => bsc.SubCategory)
                .OrderByDescending(b => b.TimeStart);

            var totalCount = await query.CountAsync();

            var paged = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (paged, totalCount);
        }

        public async Task<(List<Booking>, int)> GetBookingsByCounselorPagingAsync(string counselorId, int pageNumber, int pageSize, int? status)
        {
            var query = _context.Bookings
                .Where(b => b.CounselorId == counselorId);

            if (status.HasValue)
                query = query.Where(b => b.Status == status);

            query = query
                .Include(b => b.Member)
                .Include(b => b.Member2)
                .Include(b => b.Counselor)
                .Include(b => b.BookingSubCategories)
                    .ThenInclude(bsc => bsc.SubCategory)
                .OrderByDescending(b => b.TimeStart);

            var totalCount = await query.CountAsync();

            var paged = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (paged, totalCount);
        }

        public async Task<List<Booking>> GetRatedBookingsByCounselorAsync(string counselorId)
        {
            return await _context.Bookings
                .Where(b => b.CounselorId == counselorId && b.Rating.HasValue) // hoặc Status nào bạn xác định là đã hoàn tất
                .Include(b => b.Member)
                .OrderByDescending(b => b.TimeEnd)
                .ToListAsync();
        }

        public async Task<Booking> GetByIdWithMember(string bookingId)
        {
            return await _context.Bookings
                .Include(b => b.Member)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
        }

        public async Task UpdateMember2Async(string bookingId, string memberCode)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null)
                throw new Exception("Không tìm thấy đặt chỗ");

            if (!string.IsNullOrEmpty(booking.Member2Id))
                throw new Exception("Đặt chỗ này đã có thành viên được mời");

            booking.Member2Id = $"Member_{memberCode}";
            booking.IsCouple = false;

            await _context.SaveChangesAsync();
        }

        public async Task<List<Booking>> GetInvitationsForMemberAsync(string memberId)
        {
            return await _context.Bookings
                .Where(b => b.Member2Id == memberId && b.IsCouple == false)
                .Include(b => b.Member)
                .Include(b => b.Member2)
                .Include(b => b.Counselor)
                .Include(b => b.BookingSubCategories)
                .OrderByDescending(b => b.CreateAt)
                .ToListAsync();
        }

        public async Task<bool> AcceptInvitationAsync(string bookingId, string memberId)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b =>
                b.Id == bookingId && b.Member2Id == memberId && b.IsCouple == false);

            if (booking == null)
                return false;

            booking.IsCouple = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeclineInvitationAsync(string bookingId, string memberId)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b =>
                b.Id == bookingId && b.Member2Id == memberId && b.IsCouple == false);

            if (booking == null)
                return false;

            booking.Member2Id = null;
            booking.IsCouple = null;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelInvitationAsync(string bookingId, string creatorMemberId)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b =>
                b.Id == bookingId &&
                b.MemberId == creatorMemberId &&
                b.Member2Id != null &&
                b.IsCouple == false);

            if (booking == null)
                return false;

            booking.Member2Id = null;
            booking.IsCouple = null;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<Booking>> GetByCounselorAsync(string counselorId, DateTime? from = null, DateTime? to = null)
        {
            var q = _context.Bookings.AsQueryable()
                     .Where(b => b.CounselorId == counselorId);

            if (from.HasValue) q = q.Where(b => b.TimeStart >= from.Value);
            if (to.HasValue) q = q.Where(b => b.TimeStart < to.Value);

            return await q.ToListAsync();
        }
    }
}
