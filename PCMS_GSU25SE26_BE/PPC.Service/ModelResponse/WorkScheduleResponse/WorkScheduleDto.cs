using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.WorkScheduleResponse
{
    public class WorkScheduleDto
    {
        public string Id { get; set; }
        public string CounselorId { get; set; }
        public DateTime WorkDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime? CreateAt { get; set; }
        public string Description { get; set; }
        public int? Status { get; set; }
    }
}
