using PPC.Service.ModelResponse.CategoryResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.CourseResponse
{
    public class MyCourseDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Thumble { get; set; }
        public string Description { get; set; }
        public double? Price { get; set; }
        public int? Rank { get; set; }
        public double? Rating { get; set; }
        public int ChapterCount { get; set; }
        public int ProcessingCount { get; set; }
        public bool? IsOpen { get; set; }
        public List<ChapterDto> Chapters { get; set; }
        public List<SubCategoryDto> SubCategories { get; set; }
    }
}
