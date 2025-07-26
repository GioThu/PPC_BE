using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.CourseResponse
{
    public class LectureDto
    {
        public string Id { get; set; }
        public string LectureMetadata { get; set; }
    }

    public class VideoDto
    {
        public string Id { get; set; }
        public TimeOnly? TimeVideo { get; set; }
        public string VideoUrl { get; set; }
    }
}
