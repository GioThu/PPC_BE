using AutoMapper;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.Mappers;
using PPC.Service.ModelRequest.CourseRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.CourseResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;

        public CourseService(ICourseRepository courseRepository, IMapper mapper)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<string>> CreateCourseAsync(string creatorId, CourseCreateRequest request)
        {
            if (await _courseRepository.IsCourseNameExistAsync(request.Name))
            {
                return ServiceResponse<string>.ErrorResponse("Course name already exists.");
            }

            var course = request.ToCreateCourse();
            course.CreateBy = creatorId;

            await _courseRepository.CreateAsync(course);
            return ServiceResponse<string>.SuccessResponse("Course created successfully.");
        }

        public async Task<ServiceResponse<List<CourseDto>>> GetAllCoursesAsync()
        {
            var courses = await _courseRepository.GetAllCoursesWithDetailsAsync();
            var courseDtos = courses.Select(c => c.ToDto()).ToList();
            return ServiceResponse<List<CourseDto>>.SuccessResponse(courseDtos);
        }

        public Task<ServiceResponse<string>> SetCourseCategoryAsync(CourseCategoryRequest request)
        {
            throw new NotImplementedException();
        }
    }

}
