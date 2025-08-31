using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.Dashborad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface IDashboardService
    {
        Task<ServiceResponse<DashboardSummaryDto>> GetSummaryAsync(string counselorId, string accountId);
        Task<ServiceResponse<AdminDashboardDto>> GetDashboardAsync();
        Task<ServiceResponse<List<AdminOverviewDto>>> GetOverviewLast3MonthsAsync();
        Task<ServiceResponse<WeeklyCountsDto>> GetWeeklyCountsAsync(int year, int month);
    }
}
