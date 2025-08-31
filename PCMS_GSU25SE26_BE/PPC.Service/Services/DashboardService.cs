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

        private static double MoneyRule(int? status, double? price)
        {
            var p = price ?? 0;
            if (status == 6) return 0;
            if (status == 4) return p / 2.0;
            return p*3 / 7.0;
        }

        // ===== Trả về 3 tháng gần nhất theo DTO bạn đã có (mỗi tháng 1 AdminOverviewDto)
        public async Task<ServiceResponse<List<AdminOverviewDto>>> GetOverviewLast3MonthsAsync()
        {
            var now = DateTime.UtcNow; // hoặc Now
            var m3Start = new DateTime(now.Year, now.Month, 1);
            var m2Start = m3Start.AddMonths(-1);
            var m1Start = m3Start.AddMonths(-2);
            var rangeStart = m1Start;
            var rangeEnd = m3Start.AddMonths(1); // exclusive

            // 1) ONE repo call
            var env = await _dashboardRepository.GetOverviewRawOnceAsync(rangeStart, rangeEnd);

            // 2) Buckets theo tháng
            var buckets = new[]
            {
            (Start: m1Start, End: m2Start),
            (Start: m2Start, End: m3Start),
            (Start: m3Start, End: m3Start.AddMonths(1))
        };

            var result = new List<AdminOverviewDto>(3);

            foreach (var (start, end) in buckets)
            {
                // Members: "mới trong tháng" = count trong bucket, "tổng" = all-time totals
                var newMembers = env.MembersInRange.Count(x => x.CreateAt >= start && x.CreateAt < end);
                var newCounselors = env.CounselorsInRange.Count(x => x.CreateAt >= start && x.CreateAt < end);

                // Bookings
                var bookingsInMonth = env.BookingsInRange.Where(b => b.CreateAt >= start && b.CreateAt < end).ToList();
                var bookingsCount = bookingsInMonth.Count;
                var bookingsRevenueMonth = bookingsInMonth.Sum(b => MoneyRule(b.Status, b.Price));

                // Courses
                var coursesInMonth = env.CoursesInRange.Where(e => e.CreateDate >= start && e.CreateDate < end).ToList();
                var coursesCount = coursesInMonth.Count;
                var coursesRevenueMonth = coursesInMonth.Sum(e => e.Price ?? 0);

                // Memberships
                var membershipsInMonth = env.MembershipsInRange.Where(m => m.CreateDate >= start && m.CreateDate < end).ToList();
                var membershipsCount = membershipsInMonth.Count;
                var membershipsRevenueMonth = membershipsInMonth.Sum(m => m.Price ?? 0);

                // Lưu ý: "TotalXxx" bạn đang dùng là all-time → lấy từ env.Totals
                result.Add(new AdminOverviewDto
                {
                    // Member
                    TotalMembers = env.Totals.TotalMembers,
                    NewMembersThisMonth = newMembers,

                    // Counselor
                    TotalCounselors = env.Totals.TotalCounselors,
                    NewCounselorsThisMonth = newCounselors,

                    // Booking
                    TotalBookings = env.Totals.TotalBookings,
                    BookingsThisMonth = bookingsCount,
                    BookingRevenue = env.Totals.BookingRevenue,
                    BookingRevenueThisMonth = bookingsRevenueMonth,

                    // Course
                    TotalCoursesPurchased = env.Totals.TotalCoursesPurchased,
                    CoursesPurchasedThisMonth = coursesCount,
                    CourseRevenue = env.Totals.CourseRevenue,
                    CourseRevenueThisMonth = coursesRevenueMonth,

                    // Membership
                    TotalMemberships = env.Totals.TotalMemberships,
                    MembershipsThisMonth = membershipsCount,
                    MembershipRevenue = env.Totals.MembershipRevenue,
                    MembershipRevenueThisMonth = membershipsRevenueMonth
                });
            }

            return ServiceResponse<List<AdminOverviewDto>>.SuccessResponse(result);
        }

        // ===== Thống kê theo tuần của 1 tháng bất kỳ (đếm số lượng)
        public async Task<ServiceResponse<WeeklyCountsDto>> GetWeeklyCountsAsync(int year, int month)
        {
            // Tính ranh giới tháng
            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1);

            // ONE repo call lấy data trong tháng (đủ cho 4-6 tuần)
            var env = await _dashboardRepository.GetOverviewRawOnceAsync(monthStart, monthEnd);

            // Cắt tuần ISO (Mon-Sun) đè lên khoảng của tháng
            var firstMonday = monthStart.AddDays(((int)DayOfWeek.Monday - (int)monthStart.DayOfWeek + 7) % 7);
            if (firstMonday > monthStart) firstMonday = firstMonday.AddDays(-7); // lùi về Monday gần nhất

            var weeks = new List<(DateTime s, DateTime e)>();
            var cur = firstMonday;
            while (cur < monthEnd)
            {
                var s = cur < monthStart ? monthStart : cur;
                var e = cur.AddDays(7);
                if (e > monthEnd) e = monthEnd;
                weeks.Add((s, e));
                cur = cur.AddDays(7);
            }

            string Label(DateTime s, DateTime e) => $"{s:dd/MM} - {e.AddDays(-1):dd/MM}";

            int CountIn<T>(IEnumerable<T> src, Func<T, DateTime?> getter, DateTime s, DateTime e)
                => src.Count(x => {
                    var d = getter(x);
                    return d >= s && d < e;
                });

            var labels = weeks.Select(w => Label(w.s, w.e)).ToArray();

            var bookingCounts = weeks.Select(w => CountIn(env.BookingsInRange, b => b.CreateAt, w.s, w.e)).ToArray();
            var courseCounts = weeks.Select(w => CountIn(env.CoursesInRange, c => c.CreateDate, w.s, w.e)).ToArray();
            var memberCounts = weeks.Select(w => CountIn(env.MembershipsInRange, m => m.CreateDate, w.s, w.e)).ToArray();

            var dto = new WeeklyCountsDto
            {
                Labels = labels,
                Bookings = bookingCounts,
                Courses = courseCounts,
                Memberships = memberCounts
            };

            return ServiceResponse<WeeklyCountsDto>.SuccessResponse(dto);
        }

    }
}
