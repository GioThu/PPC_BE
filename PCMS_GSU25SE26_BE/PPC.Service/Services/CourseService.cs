using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
        private readonly ILectureRepository _lectureRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly IQuizRepository _quizRepository;


        public CourseService(ICourseRepository courseRepository, IMapper mapper, ICourseSubCategoryRepository courseSubCategoryRepository, ILectureRepository lectureRepository, IChapterRepository chapterRepository, IQuizRepository quizRepository)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _courseSubCategoryRepository = courseSubCategoryRepository;
            _lectureRepository = lectureRepository;
            _chapterRepository = chapterRepository;
            _quizRepository = quizRepository;
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

        public async Task<ServiceResponse<string>> CreateLectureWithChapterAsync(LectureWithChapterCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return ServiceResponse<string>.ErrorResponse("Name is required.");

            var nextChapNum = await _chapterRepository.GetNextChapterNumberAsync(request.CourseId);

            var chapter = request.ToChapter(nextChapNum);
            await _chapterRepository.CreateAsync(chapter);
            

            var lecture = request.ToLecture(chapter.Id);
            await _lectureRepository.CreateAsync(lecture);
            chapter.ChapNo = lecture.Id; 
            
            await _chapterRepository.UpdateAsync(chapter);

            return ServiceResponse<string>.SuccessResponse("Lecture and chapter created successfully.");
        }

        public async Task<ServiceResponse<string>> CreateQuizWithChapterAsync(QuizWithChapterCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return ServiceResponse<string>.ErrorResponse("Name is required.");

            var nextChapNum = await _chapterRepository.GetNextChapterNumberAsync(request.CourseId);
            var chapter = request.ToChapter(nextChapNum);
            await _chapterRepository.CreateAsync(chapter);

            var quiz = request.ToQuiz(chapter.Id);
            await _quizRepository.CreateAsync(quiz);
            chapter.ChapNo = quiz.Id;
            await _chapterRepository.UpdateAsync(chapter);

            return ServiceResponse<string>.SuccessResponse("Quiz and Chapter created successfully.");
        }

        public async Task<ServiceResponse<CourseDto>> GetCourseDetailByIdAsync(string courseId)
        {
            var course = await _courseRepository.GetCourseWithAllDetailsAsync(courseId);
            if (course == null)
                return ServiceResponse<CourseDto>.ErrorResponse("Course not found.");

            var dto = _mapper.Map<CourseDto>(course);
            return ServiceResponse<CourseDto>.SuccessResponse(dto);
        }

        public async Task<ServiceResponse<ChapterDetailDto>> GetChapterDetailAsync(string chapterId)
        {
            var chapter = await _chapterRepository.GetByIdAsync(chapterId);
            if (chapter == null)
                return ServiceResponse<ChapterDetailDto>.ErrorResponse("Chapter not found.");

            var dto = _mapper.Map<ChapterDetailDto>(chapter);

            if (chapter.ChapterType == "Lecture")
            {
                var lecture = await _lectureRepository.GetByIdAsync(chapter.ChapNo);

                dto.Lecture = _mapper.Map<LectureDto>(lecture);
            }
            else if (chapter.ChapterType == "Quiz")
            {
                var quiz = await _quizRepository.GetByIdAsync(chapter.ChapNo);
                dto.Quiz = _mapper.Map<QuizDto>(quiz);
            }

            return ServiceResponse<ChapterDetailDto>.SuccessResponse(dto);
        }
    }

}
