using AutoMapper;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.Dashborad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IMapper _mapper;

        public DashboardService(IDashboardRepository dashboardRepository, IMapper mapper)
        {
            _dashboardRepository = dashboardRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<DashboardSummaryDto>> GetSummaryAsync(string counselorId, string accountId)
        {
            var summary = new DashboardSummary
            {
                CurrentBalance = await _dashboardRepository.GetWalletRemainingByAccountIdAsync(accountId),
                ThisMonthIncome = await _dashboardRepository.GetThisMonthIncomeByCounselorAsync(counselorId),
                PendingPayment = await _dashboardRepository.GetPendingPaymentByCounselorAsync(counselorId),
                WithdrawnTotal = await _dashboardRepository.GetWithdrawnTotalByAccountIdAsync(accountId),
                PendingDeposit = await _dashboardRepository.GetPendingDepositByAccountIdAsync(accountId),
            };

            var dto = _mapper.Map<DashboardSummaryDto>(summary);
            return ServiceResponse<DashboardSummaryDto>.SuccessResponse(dto);
        }

        public async Task<ServiceResponse<AdminDashboardDto>> GetDashboardAsync()
        {
            var data = await _dashboardRepository.GetDashboardDataAsync();

            var summary = new AdminDashboardSummary
            {
                CurrentBalance = data.currentBalance,
                ThisMonthIncome = data.thisMonthIncome,
                PendingPayment = data.pendingPayment,
                WithdrawnTotal = data.withdrawnTotal,
                PendingDeposit = data.pendingDeposit,
            };

            var statistic = new AdminBookingStatistic
            {
                TotalBooking = data.totalBooking,
                CompletedBooking = data.completedBooking,
                Revenue = data.revenue,
                CompletionRate = data.totalBooking == 0 ? 0 : (data.completedBooking * 100.0 / data.totalBooking),
                AverageRating = data.avgRating
            };

            var domain = new AdminDashboard { Summary = summary, BookingStatistic = statistic };
            var dto = _mapper.Map<AdminDashboardDto>(domain);

            return ServiceResponse<AdminDashboardDto>.SuccessResponse(dto);
        }
    }
}
