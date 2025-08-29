using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface IDashboardRepository
    {
        Task<double> GetWalletRemainingByAccountIdAsync(string accountId);
        Task<double> GetThisMonthIncomeByCounselorAsync(string counselorId);
        Task<double> GetPendingPaymentByCounselorAsync(string counselorId);
        Task<double> GetWithdrawnTotalByAccountIdAsync(string accountId);
        Task<double> GetPendingDepositByAccountIdAsync(string accountId);
        Task<(double currentBalance, double thisMonthIncome, double pendingPayment, double withdrawnTotal, double pendingDeposit,   int totalBooking, int completedBooking, double revenue, double avgRating)> GetDashboardDataAsync();
    }
}
