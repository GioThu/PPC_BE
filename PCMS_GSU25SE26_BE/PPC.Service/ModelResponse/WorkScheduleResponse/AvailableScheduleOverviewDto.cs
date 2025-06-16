using PPC.Service.ModelResponse.CategoryResponse;
using PPC.Service.ModelResponse.CounselorResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.WorkScheduleResponse
{
    public class AvailableScheduleOverviewDto
    {
        public string CounselorId { get; set; }
        public CounselorDto Counselor { get; set; }
        public List<SubCategoryDto> SubCategories { get; set; }
        public List<DailyAvailableSlotDto> DailyAvailableSchedules { get; set; }
    }

    public class DailyAvailableSlotDto
    {
        public DateTime WorkDate { get; set; }
        public List<AvailableTimeSlotDto> AvailableSlots { get; set; }
    }

}
