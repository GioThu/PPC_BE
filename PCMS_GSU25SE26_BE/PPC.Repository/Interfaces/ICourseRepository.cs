using Microsoft.EntityFrameworkCore;
using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface ICourseRepository : IGenericRepository<Course>
    {
        Task<bool> IsCourseNameExistAsync(string courseName);
        Task<List<Course>> GetAllCoursesWithDetailsAsync();
        Task<Course> GetCourseWithAllDetailsAsync(string courseId);
        Task<List<EnrollCourse>> GetEnrollCoursesByAccountIdAsync(string memberId);
        Task<List<Course>> GetAllActiveCoursesAsync();
        Task<List<string>> GetEnrolledCourseIdsAsync(string accountId);
        Task<bool> RateCourseAsync(string courseId, string memberId, int rating, string feedback);
        Task<List<EnrollCourse>> GetEnrollCoursesByCourseIdAsync(string courseId);
        Task<List<Course>> GetCoursesByCategoriesAsync(List<string> categoryIds);
        Task<List<Course>> GetTopRatedCoursesAsync(int topN);
    }
}
