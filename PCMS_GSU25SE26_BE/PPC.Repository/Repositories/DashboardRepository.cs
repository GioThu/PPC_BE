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



        public async Task<AdminOverviewRaw> GetOverviewAsync(DateTime firstDay, DateTime nextMonth)
        {
            // MEMBERS (tuần tự)
            var members = await _context.Members
                .AsNoTracking()
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    NewThisMonth = g.Count(m => m.Account.CreateAt >= firstDay && m.Account.CreateAt < nextMonth)
                })
                .FirstOrDefaultAsync()
                ?? new { Total = 0, NewThisMonth = 0 };

            // COUNSELORS (tuần tự)
            var counselors = await _context.Counselors
                .AsNoTracking()
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    NewThisMonth = g.Count(c => c.Account.CreateAt >= firstDay && c.Account.CreateAt < nextMonth)
                })
                .FirstOrDefaultAsync()
                ?? new { Total = 0, NewThisMonth = 0 };

            // BOOKINGS (tuần tự) — status=6 bỏ, status=4 tính 1/2
            var bookings = await _context.Bookings
                .AsNoTracking()
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    ThisMonth = g.Count(b => b.CreateAt >= firstDay && b.CreateAt < nextMonth),
                    Revenue = g.Sum(b =>
                        b.Status == 6 ? (double?)0 :
                        b.Status == 4 ? ((double?)(b.Price ?? 0) / 2) :
                                        (double?)(b.Price ?? 0)
                    ) ?? 0,
                    RevenueThisMonth = g.Sum(b =>
                        (b.CreateAt >= firstDay && b.CreateAt < nextMonth)
                            ? (b.Status == 6 ? (double?)0 :
                               b.Status == 4 ? ((double?)(b.Price ?? 0) / 2) :
                                               (double?)(b.Price ?? 0))
                            : 0
                    ) ?? 0
                })
                .FirstOrDefaultAsync()
                ?? new { Total = 0, ThisMonth = 0, Revenue = 0.0, RevenueThisMonth = 0.0 };

            // ENROLL COURSE (tuần tự)
            var courses = await _context.EnrollCourses
                .AsNoTracking()
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    ThisMonth = g.Count(e => e.CreateDate >= firstDay && e.CreateDate < nextMonth),
                    Revenue = g.Sum(e => (double?)(e.Price ?? 0)) ?? 0,
                    RevenueThisMonth = g.Sum(e =>
                        (e.CreateDate >= firstDay && e.CreateDate < nextMonth) ? (double?)(e.Price ?? 0) : 0
                    ) ?? 0
                })
                .FirstOrDefaultAsync()
                ?? new { Total = 0, ThisMonth = 0, Revenue = 0.0, RevenueThisMonth = 0.0 };

            // MEMBERSHIP (tuần tự)
            var memberships = await _context.MemberMemberShips
                .AsNoTracking()
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    ThisMonth = g.Count(m => m.CreateDate >= firstDay && m.CreateDate < nextMonth),
                    Revenue = g.Sum(m => (double?)(m.Price ?? 0)) ?? 0,
                    RevenueThisMonth = g.Sum(m =>
                        (m.CreateDate >= firstDay && m.CreateDate < nextMonth) ? (double?)(m.Price ?? 0) : 0
                    ) ?? 0
                })
                .FirstOrDefaultAsync()
                ?? new { Total = 0, ThisMonth = 0, Revenue = 0.0, RevenueThisMonth = 0.0 };

            return new AdminOverviewRaw
            {
                TotalMembers = members.Total,
                NewMembersThisMonth = members.NewThisMonth,

                TotalCounselors = counselors.Total,
                NewCounselorsThisMonth = counselors.NewThisMonth,

                TotalBookings = bookings.Total,
                BookingsThisMonth = bookings.ThisMonth,
                BookingRevenue = bookings.Revenue,
                BookingRevenueThisMonth = bookings.RevenueThisMonth,

                TotalCoursesPurchased = courses.Total,
                CoursesPurchasedThisMonth = courses.ThisMonth,
                CourseRevenue = courses.Revenue,
                CourseRevenueThisMonth = courses.RevenueThisMonth,

                TotalMemberships = memberships.Total,
                MembershipsThisMonth = memberships.ThisMonth,
                MembershipRevenue = memberships.Revenue,
                MembershipRevenueThisMonth = memberships.RevenueThisMonth
            };
        }

    }
}
