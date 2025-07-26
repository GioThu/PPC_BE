using PPC.DAO.Models;
using PPC.Service.ModelRequest.CourseRequest;
using PPC.Service.ModelResponse.CategoryResponse;
using PPC.Service.ModelResponse.CourseResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Mappers
{
    public static class CourseMapper
    {
        public static Course ToCreateCourse(this CourseCreateRequest request)
        {
            return new Course
            {
                Id = Utils.Utils.GenerateIdModel("Course"),
                Name = request.Name,
                Status = 0,
                CreateAt = Utils.Utils.GetTimeNow()
            };
        }

        public static CourseDto ToDto(this Course course)
        {
            return new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Thumble = course.Thumble,
                Description = course.Description,
                Price = course.Price,
                Rank = course.Rank,
                Rating = course.Rating,
                ChapterCount = course.Chapters?.Count ?? 0,
                Chapters = course.Chapters?.Select(c => new ChapterDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ChapNo = c.ChapNo,
                    Description = c.Description,
                    ChapterType = c.ChapterType,
                }).OrderBy(c => c.ChapNo).ToList(),

                SubCategories = course.CourseSubCategories?.Select(cs => new SubCategoryDto
                {
                    Id = cs.SubCategory.Id,
                    Name = cs.SubCategory.Name
                }).ToList(),
                Status = course.Status ?? 0

            };
        }
    }
}
