using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.CourseResponse
{
    public class ChapterDto
    {
        public string Id { get; set; }

        public int? ChapNum { get; set; }

        public string CourseId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ChapterType { get; set; }

        public string ChapNo { get; set; }

        public DateTime? CreateAt { get; set; }

        public int? Status { get; set; }
    }

    public class MemberChapterDto
    {
        public string Id { get; set; }

        public int? ChapNum { get; set; }

        public string CourseId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ChapterType { get; set; }

        public string ChapNo { get; set; }

        public DateTime? CreateAt { get; set; }

        public int? Status { get; set; }

        public bool IsDone { get; set; }
    }
}
