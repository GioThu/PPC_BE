using PPC.Service.ModelResponse.CategoryResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.CourseResponse
{
    public class CourseDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Thumble { get; set; }
        public string Description { get; set; }
        public double? Price { get; set; }
        public int? Rank { get; set; }
        public double? Rating { get; set; }
        public int ChapterCount { get; set; }
        public List<ChapterDto> Chapters { get; set; }
        public List<SubCategoryDto> SubCategories { get; set; }
        public int Status { get; set; }
    }

    public class MemberCourseDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Thumble { get; set; }
        public string Description { get; set; }
        public double? Price { get; set; }
        public int? Rank { get; set; }
        public double? Rating { get; set; }
        public int ProcessingCount { get; set; }
        public int ChapterCount { get; set; }
        public bool IsEnrolled { get; set; }
        public bool IsFree { get; set; }
        public bool IsBuy { get; set; }
        public string? FreeByMembershipName { get; set; }
        public List<MemberChapterDto> Chapters { get; set; }
        public List<SubCategoryDto> SubCategories { get; set; }
        public int Status { get; set; }
    }
}
