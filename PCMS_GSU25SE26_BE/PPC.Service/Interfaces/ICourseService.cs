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
        Task<ServiceResponse<string>> AddSubCategoryAsync(CourseSubCategoryRequest request);
        Task<ServiceResponse<string>> RemoveSubCategoryAsync(CourseSubCategoryRequest request);
        Task<ServiceResponse<string>> CreateLectureWithChapterAsync(LectureWithChapterCreateRequest request);
        Task<ServiceResponse<string>> CreateVideoWithChapterAsync(VideoWithChapterCreateRequest request);
        Task<ServiceResponse<string>> CreateQuizWithChapterAsync(QuizWithChapterCreateRequest request);
        Task<ServiceResponse<CourseDto>> GetCourseDetailByIdAsync(string courseId);
        Task<ServiceResponse<ChapterDetailDto>> GetChapterDetailAsync(string chapterId);
        Task<ServiceResponse<List<CourseListDto>>> GetAllCoursesAsync(string accountId, string memberId);
        Task<ServiceResponse<EnrollCourseResultDto>> EnrollCourseAsync(string courseId, string accountId);
        Task<ServiceResponse<List<MyCourseDto>>> GetEnrolledCoursesWithProgressAsync(string memberId);
        Task<ServiceResponse<string>> UpdateCourseAsync(string courseId, CourseUpdateRequest request);
        Task<ServiceResponse<MemberCourseDto>> GetMemberCourseDetailAsync(string courseId, string accountId);
        Task<ServiceResponse<string>> UpdateLectureByChapterIdAsync(UpdateLectureRequest request);
        Task<ServiceResponse<string>> UpdateVideoByChapterIdAsync(UpdateVideoRequest request);
        Task<ServiceResponse<string>> UpdateQuizByChapterIdAsync(UpdateQuizRequest request);
        Task<ServiceResponse<string>> OpenCourseAsync(string courseId, string memberId);
        Task<ServiceResponse<string>> ChangeCourseStatusAsync(string courseId, int newStatus);
        Task<ServiceResponse<string>> DeleteChapterAsync(string chapterId);
        Task<ServiceResponse<string>> RateCourseAsync(string courseId, string memberId, int rating, string feedback);
        Task<ServiceResponse<List<ReviewDto>>> GetCourseReviewsAsync(string courseId);
        Task<ServiceResponse<List<CourseWithSubCategoryDto>>> GetRecommendedCoursesAsync(string memberId);
        Task<ServiceResponse<List<CourseWithSubCategoryDto>>> GetRecommendedCoursesByCoupleIdAsync(string coupleId);
        Task<ServiceResponse<int>> GetMaxCourseDiscountByMemberWrappedAsync(string memberId);
    }
}
