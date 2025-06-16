using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse
{
    public class AvailableScheduleDto
    {
        public string CounselorId { get; set; }
        public DateTime WorkDate { get; set; }
        public List<AvailableTimeSlotDto> AvailableSlots { get; set; }
        public List<SubCategoryDto> SubCategories { get; set; }
    }
}
