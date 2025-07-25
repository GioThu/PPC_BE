using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.CourseResponse
{
    public class EnrollCourseResultDto
    {
        public string EnrollCourseId { get; set; }
        public double PaidAmount { get; set; }
        public double? Remaining { get; set; }
        public string TransactionId { get; set; }
        public string Message { get; set; }
    }
}
