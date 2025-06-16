using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.WorkScheduleResponse
{
    public class AvailableTimeSlotDto
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
    }
}
