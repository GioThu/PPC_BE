using PPC.Service.ModelResponse.CategoryResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.CourseResponse
{
    public class CourseWithSubCategoryDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Thumble { get; set; }
        public string Description { get; set; }
        public double? Price { get; set; }
        public double? Rating { get; set; }
        public int? Reviews { get; set; }
        public List<SubCategoryDto> SubCategories { get; set; }
    }
}
