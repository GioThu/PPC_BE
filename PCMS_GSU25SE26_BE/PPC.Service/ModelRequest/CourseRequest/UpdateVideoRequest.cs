using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.CourseRequest
{
    public class UpdateVideoRequest
    {
        public string ChapterId { get; set; }
        public string ChapterName { get; set; }
        public string ChapterDescription { get; set; }
        public TimeOnly? TimeVideo { get; set; }
        public string VideoUrl { get; set; }
    }
}
