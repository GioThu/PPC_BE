using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.CourseRequest
{
    public class ChangeCourseStatusRequest
    {
        public string CourseId { get; set; }
        public int NewStatus { get; set; }
    }
}
