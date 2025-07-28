using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.CourseRequest
{
    public class RateCourseRequest
    {
        public int Rating { get; set; }  // Rating out of 5 (example)
        public string Feedback { get; set; }  // Feedback text
    }
}
