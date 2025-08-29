using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.Dashborad
{
    public class AdminDashboardSummaryDto
    {
        public double CurrentBalance { get; set; }   // Tổng Remaining tất cả ví
        public double ThisMonthIncome { get; set; }  // SUM(Booking.Price) status=7 trong tháng hiện tại
        public double PendingPayment { get; set; }   // SUM(Booking.Price) status=2
        public double WithdrawnTotal { get; set; }   // SUM(Deposit.Total) status=2
        public double PendingDeposit { get; set; }   // SUM(Deposit.Total) status=0
    }

    public class AdminBookingStatisticDto
    {
        public int TotalBooking { get; set; }        // COUNT(*)
        public int CompletedBooking { get; set; }    // COUNT(*) status in (2,7)
        public double Revenue { get; set; }          // SUM(Price) status in (2,7)
        public double CompletionRate { get; set; }   // %
        public double AverageRating { get; set; }    // AVG(Rating)
    }

    public class AdminDashboardDto
    {
        public AdminDashboardSummaryDto Summary { get; set; }
        public AdminBookingStatisticDto BookingStatistic { get; set; }
    }

    public class AdminDashboardSummary
    {
        public double CurrentBalance { get; set; }
        public double ThisMonthIncome { get; set; }
        public double PendingPayment { get; set; }
        public double WithdrawnTotal { get; set; }
        public double PendingDeposit { get; set; }
    }

    public class AdminBookingStatistic
    {
        public int TotalBooking { get; set; }
        public int CompletedBooking { get; set; }
        public double Revenue { get; set; }
        public double CompletionRate { get; set; }
        public double AverageRating { get; set; }
    }

    public class AdminDashboard
    {
        public AdminDashboardSummary Summary { get; set; }
        public AdminBookingStatistic BookingStatistic { get; set; }
    }
}
