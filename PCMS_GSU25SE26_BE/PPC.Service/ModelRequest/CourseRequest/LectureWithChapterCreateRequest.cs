using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.CourseRequest
{
    public class LectureWithChapterCreateRequest
    {
        public string CourseId { get; set; }
        public string Name { get; set; }                  
        public string Description { get; set; }
        public string LectureMetadata { get; set; }
    }


    public class VideoWithChapterCreateRequest
    {
        public string CourseId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TimeOnly? TimeVideo { get; set; }
        public string VideoUrl { get; set; }
    }
}
