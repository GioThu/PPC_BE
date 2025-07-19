using AutoMapper;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Repository.Repositories;
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
        private readonly ICourseSubCategoryRepository _courseSubCategoryRepository;
        private readonly IMapper _mapper;

        public CourseService(ICourseRepository courseRepository, IMapper mapper, ICourseSubCategoryRepository courseSubCategoryRepository)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _courseSubCategoryRepository = courseSubCategoryRepository;
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

        public async Task<ServiceResponse<string>> AddSubCategoryAsync(CourseSubCategoryRequest request)
        {
            if (await _courseSubCategoryRepository.ExistsAsync(request.CourseId, request.SubCategoryId))
            {
                return ServiceResponse<string>.ErrorResponse("Sub-category already exists in course.");
            }

            var entry = new CourseSubCategory
            {
                Id = Utils.Utils.GenerateIdModel("CourseSubCategory"),
                CourseId = request.CourseId,
                SubCategoryId = request.SubCategoryId
            };

            await _courseSubCategoryRepository.CreateAsync(entry);
            return ServiceResponse<string>.SuccessResponse("Sub-category added to course successfully.");
        }

        public async Task<ServiceResponse<string>> RemoveSubCategoryAsync(CourseSubCategoryRequest request)
        {
            var entry = await _courseSubCategoryRepository.GetAsync(request.CourseId, request.SubCategoryId);
            if (entry == null)
            {
                return ServiceResponse<string>.ErrorResponse("Sub-category not found in course.");
            }

            await _courseSubCategoryRepository.RemoveAsync(entry);
            return ServiceResponse<string>.SuccessResponse("Sub-category removed from course successfully.");
        }
    }

}
