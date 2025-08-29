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
    }
}
