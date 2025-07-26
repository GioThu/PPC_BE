using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.CourseRequest
{
    public class UpdateLectureRequest
    {
        public string ChapterId { get; set; }
        public string ChapterName { get; set; }
        public string ChapterDescription { get; set; }
        public string LectureMetadata { get; set; }
    }
}
