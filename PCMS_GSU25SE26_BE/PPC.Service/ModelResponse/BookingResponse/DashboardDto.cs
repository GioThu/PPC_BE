using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.BookingResponse
{
    public class DashboardDto
    {
        public double TotalIncome { get; set; }
        public int AppointmentsThisWeek { get; set; }
        public int CompletedSessions { get; set; }
        public double AverageRating { get; set; }
        public List<MonthlyIncomePointDto> MonthlyIncome { get; set; } = new();
        public List<WeekdayCountDto> WeeklyAppointments { get; set; } = new();
    }

    public class MonthlyIncomePointDto
    {
        public int Month { get; set; }        // 1..12
        public double Income { get; set; }    // VND
    }

    public class WeekdayCountDto
    {
        public int DayOfWeek { get; set; }    // 1=Mon .. 7=Sun
        public int Count { get; set; }
    }

    public class BookingDashboardDto
    {
        public DateTime? TimeStart { get; set; }
        public DateTime? TimeEnd { get; set; }
        public double? Price { get; set; }
        public int? Status { get; set; }
        public int? Rating { get; set; }
    }
}
