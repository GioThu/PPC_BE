using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.CourseResponse
{
    public class ReviewDto
    {
        public int? Rating { get; set; }
        public string Feedback { get; set; }
        public string MemberName { get; set; }
    }
}
