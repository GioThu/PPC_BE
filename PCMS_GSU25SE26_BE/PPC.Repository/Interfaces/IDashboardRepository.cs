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
        Task<(double currentBalance, double thisMonthIncome, double pendingPayment, double withdrawnTotal, double pendingDeposit, int totalBooking, int completedBooking, double revenue, double avgRating)> GetDashboardDataAsync();
        Task<AdminOverviewRawEnvelope> GetOverviewRawOnceAsync(DateTime rangeStart, DateTime rangeEnd);

    }

    public class MemberMeta { public DateTime? CreateAt { get; set; } }        // Account.CreateAt
    public class CounselorMeta { public DateTime? CreateAt { get; set; } }     // Account.CreateAt

    public class BookingMeta
    {
        public DateTime? CreateAt { get; set; }
        public int? Status { get; set; }
        public double? Price { get; set; }
    }

    public class EnrollCourseMeta
    {
        public DateTime? CreateDate { get; set; }
        public double? Price { get; set; }
    }

    public class MembershipMeta
    {
        public DateTime? CreateDate { get; set; }
        public double? Price { get; set; }
    }

    // Tổng hợp all-time (để Service không phải tải hết DB về RAM)
    public class AdminTotalsRaw
    {
        public int TotalMembers { get; set; }
        public int TotalCounselors { get; set; }
        public int TotalBookings { get; set; }
        public double BookingRevenue { get; set; }      // rule: 6=0, 4=Price/2, else Price
        public int TotalCoursesPurchased { get; set; }
        public double CourseRevenue { get; set; }
        public int TotalMemberships { get; set; }
        public double MembershipRevenue { get; set; }
    }

    // Phong bì trả về duy nhất từ Repo
    public class AdminOverviewRawEnvelope
    {
        // All-time totals (đã tính ở repo để không phải kéo all records)
        public AdminTotalsRaw Totals { get; set; }

        // Dữ liệu thô CHỈ trong khoảng thời gian yêu cầu (Service sẽ tách tháng/tuần)
        public List<MemberMeta> MembersInRange { get; set; }
        public List<CounselorMeta> CounselorsInRange { get; set; }
        public List<BookingMeta> BookingsInRange { get; set; }
        public List<EnrollCourseMeta> CoursesInRange { get; set; }
        public List<MembershipMeta> MembershipsInRange { get; set; }
    }
}
