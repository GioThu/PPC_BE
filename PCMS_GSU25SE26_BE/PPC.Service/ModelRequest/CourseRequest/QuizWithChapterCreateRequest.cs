using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.CourseRequest
{
    public class QuizWithChapterCreateRequest
    {
        public string CourseId { get; set; }
        public string Name { get; set; } 
        public string Description { get; set; }
    }
}
