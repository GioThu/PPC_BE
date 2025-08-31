using Microsoft.EntityFrameworkCore;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly CCPContext _context;
        public DashboardRepository(CCPContext context)
        {
            _context = context;
        }

        public async Task<double> GetWalletRemainingByAccountIdAsync(string accountId)
        {
            // Lấy Remaining từ ví gắn với Account
            return await _context.Accounts
                .Where(a => a.Id == accountId)
                .Select(a => (double?)(a.Wallet.Remaining ?? 0))
                .FirstOrDefaultAsync() ?? 0;
        }

        public async Task<double> GetThisMonthIncomeByCounselorAsync(string counselorId)
        {
            var now = DateTime.UtcNow;
            var firstDay = new DateTime(now.Year, now.Month, 1);
            var nextMonth = firstDay.AddMonths(1);

            return await _context.Bookings
                .Where(b => b.CounselorId == counselorId
                            && b.Status == 7
                            && b.TimeStart >= firstDay
                            && b.TimeStart < nextMonth)
                .SumAsync(b => (double?)(b.Price ?? 0)) ?? 0;
        }

        public async Task<double> GetPendingPaymentByCounselorAsync(string counselorId)
        {
            return await _context.Bookings
                .Where(b => b.CounselorId == counselorId && b.Status == 2)
                .SumAsync(b => (double?)(b.Price ?? 0)) ?? 0;
        }

        public async Task<double> GetWithdrawnTotalByAccountIdAsync(string accountId)
        {
            var walletId = await _context.Accounts
                .Where(a => a.Id == accountId)
                .Select(a => a.WalletId)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(walletId)) return 0;

            return await _context.Deposits
                .Where(d => d.WalletId == walletId && d.Status == 2)
                .SumAsync(d => (double?)(d.Total ?? 0)) ?? 0;
        }

        public async Task<double> GetPendingDepositByAccountIdAsync(string accountId)
        {
            var walletId = await _context.Accounts
                .Where(a => a.Id == accountId)
                .Select(a => a.WalletId)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(walletId)) return 0;

            return await _context.Deposits
                .Where(d => d.WalletId == walletId && d.Status == 1)
                .SumAsync(d => (double?)(d.Total ?? 0)) ?? 0;
        }

        public async Task<(double currentBalance, double thisMonthIncome, double pendingPayment, double withdrawnTotal, double pendingDeposit,
                   int totalBooking, int completedBooking, double revenue, double avgRating)> GetDashboardDataAsync()
        {
            // Ví
            var currentBalance = await _context.Wallets.SumAsync(w => (double?)(w.Remaining ?? 0)) ?? 0;

            // Thời gian hiện tại
            var now = DateTime.UtcNow;
            var firstDay = new DateTime(now.Year, now.Month, 1);
            var nextMonth = firstDay.AddMonths(1);

            // Booking query gộp
            var bookingQuery = _context.Bookings.AsQueryable();

            var thisMonthIncome = await bookingQuery
                .Where(b => b.TimeStart >= firstDay && b.TimeStart < nextMonth)
                .SumAsync(b =>
                    b.Status == 6 ? 0 :
                    b.Status == 4 ? (double?)(b.Price ?? 0) / 2 :
                    (double?)(b.Price ?? 0)) ?? 0;

            var pendingPayment = await bookingQuery
                .Where(b => b.Status == 2)
                .SumAsync(b => (double?)(b.Price ?? 0)) ?? 0;

            var totalBooking = await bookingQuery.CountAsync();
            var completedBooking = await bookingQuery.CountAsync(b => b.Status == 2 || b.Status == 7);

            var revenue = await bookingQuery
                .Where(b => b.Status == 2 || b.Status == 7)
                .SumAsync(b => (double?)(b.Price ?? 0)) ?? 0;

            var avgRating = await bookingQuery
                .Where(b => b.Rating != null)
                .AverageAsync(b => (double?)b.Rating) ?? 0;

            // Deposit query gộp
            var depositQuery = _context.Deposits.AsQueryable();

            var withdrawnTotal = await depositQuery
                .Where(d => d.Status == 2)
                .SumAsync(d => (double?)(d.Total ?? 0)) ?? 0;

            var pendingDeposit = await depositQuery
                .Where(d => d.Status == 0)
                .SumAsync(d => (double?)(d.Total ?? 0)) ?? 0;

            return (currentBalance, thisMonthIncome, pendingPayment, withdrawnTotal, pendingDeposit,
                    totalBooking, completedBooking, revenue, avgRating);
        }



        public async Task<AdminOverviewRawEnvelope> GetOverviewRawOnceAsync(DateTime rangeStart, DateTime rangeEnd)
        {
            // ===== All-time totals (aggregate) =====
            // Members / Counselors
            var totalMembers = await _context.Members.AsNoTracking().CountAsync();
            var totalCounselors = await _context.Counselors.AsNoTracking().CountAsync();

            // Bookings revenue with rule (6=0, 4=1/2, else full)
            var bookingAgg = await _context.Bookings.AsNoTracking()
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Revenue = g.Sum(b =>
                        b.Status == 6 ? (double?)0 :
                        b.Status == 4 ? ((double?)(b.Price ?? 0) / 2) :
                                        ((double?)(b.Price ?? 0)*3/10)
                    ) ?? 0
                })
                .FirstOrDefaultAsync() ?? new { Total = 0, Revenue = 0.0 };

            // Courses
            var courseAgg = await _context.EnrollCourses.AsNoTracking()
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Revenue = g.Sum(e => (double?)(e.Price ?? 0)) ?? 0
                })
                .FirstOrDefaultAsync() ?? new { Total = 0, Revenue = 0.0 };

            // Memberships
            var membershipAgg = await _context.MemberMemberShips.AsNoTracking()
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Revenue = g.Sum(m => (double?)(m.Price ?? 0)) ?? 0
                })
                .FirstOrDefaultAsync() ?? new { Total = 0, Revenue = 0.0 };

            var totals = new AdminTotalsRaw
            {
                TotalMembers = totalMembers,
                TotalCounselors = totalCounselors,
                TotalBookings = bookingAgg.Total,
                BookingRevenue = bookingAgg.Revenue,
                TotalCoursesPurchased = courseAgg.Total,
                CourseRevenue = courseAgg.Revenue,
                TotalMemberships = membershipAgg.Total,
                MembershipRevenue = membershipAgg.Revenue
            };

            // ===== Items trong khoảng [rangeStart, rangeEnd) =====
            var membersInRange = await _context.Members.AsNoTracking()
                .Where(m => m.Account.CreateAt >= rangeStart && m.Account.CreateAt < rangeEnd)
                .Select(m => new MemberMeta { CreateAt = m.Account.CreateAt })
                .ToListAsync();

            var counselorsInRange = await _context.Counselors.AsNoTracking()
                .Where(c => c.Account.CreateAt >= rangeStart && c.Account.CreateAt < rangeEnd)
                .Select(c => new CounselorMeta { CreateAt = c.Account.CreateAt })
                .ToListAsync();

            var bookingsInRange = await _context.Bookings.AsNoTracking()
                .Where(b => b.CreateAt >= rangeStart && b.CreateAt < rangeEnd)
                .Select(b => new BookingMeta
                {
                    CreateAt = b.CreateAt,
                    Status = b.Status,
                    Price = b.Price
                })
                .ToListAsync();

            var coursesInRange = await _context.EnrollCourses.AsNoTracking()
                .Where(e => e.CreateDate >= rangeStart && e.CreateDate < rangeEnd)
                .Select(e => new EnrollCourseMeta
                {
                    CreateDate = e.CreateDate,
                    Price = e.Price
                })
                .ToListAsync();

            var membershipsInRange = await _context.MemberMemberShips.AsNoTracking()
                .Where(m => m.CreateDate >= rangeStart && m.CreateDate < rangeEnd)
                .Select(m => new MembershipMeta
                {
                    CreateDate = m.CreateDate,
                    Price = m.Price
                })
                .ToListAsync();

            return new AdminOverviewRawEnvelope
            {
                Totals = totals,
                MembersInRange = membersInRange,
                CounselorsInRange = counselorsInRange,
                BookingsInRange = bookingsInRange,
                CoursesInRange = coursesInRange,
                MembershipsInRange = membershipsInRange
            };
        }

    }
}
