using PPC.Service.ModelRequest.CourseRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.CourseResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface ICourseService
    {
        Task<ServiceResponse<string>> CreateCourseAsync(string creatorId, CourseCreateRequest request);
        Task<ServiceResponse<List<CourseDto>>> GetAllCoursesAsync();
        Task<ServiceResponse<string>> SetCourseCategoryAsync(CourseCategoryRequest request);
    }
}
