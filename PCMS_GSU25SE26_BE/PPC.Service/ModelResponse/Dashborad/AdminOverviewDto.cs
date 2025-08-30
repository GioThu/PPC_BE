using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.Dashborad
{
    public class AdminOverviewDto
    {
        // Member
        public int TotalMembers { get; set; }
        public int NewMembersThisMonth { get; set; }

        // Counselor
        public int TotalCounselors { get; set; }
        public int NewCounselorsThisMonth { get; set; }

        // Booking (quy tắc tiền: status=6 bỏ, status=4 tính 1/2, còn lại full)
        public int TotalBookings { get; set; }
        public int BookingsThisMonth { get; set; }
        public double BookingRevenue { get; set; }
        public double BookingRevenueThisMonth { get; set; }

        // Course (EnrollCourse)
        public int TotalCoursesPurchased { get; set; }
        public int CoursesPurchasedThisMonth { get; set; }
        public double CourseRevenue { get; set; }
        public double CourseRevenueThisMonth { get; set; }

        // Membership (MemberMemberShip)
        public int TotalMemberships { get; set; }
        public int MembershipsThisMonth { get; set; }
        public double MembershipRevenue { get; set; }
        public double MembershipRevenueThisMonth { get; set; }
    }
}
