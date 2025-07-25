using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.CourseResponse
{
    public class ChapterDetailDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int? ChapNum { get; set; }
        public string Description { get; set; }
        public string ChapterType { get; set; }

        public LectureDto Lecture { get; set; }
        public QuizDto Quiz { get; set; }
        public VideoDto Video { get; set; }
    }
}
