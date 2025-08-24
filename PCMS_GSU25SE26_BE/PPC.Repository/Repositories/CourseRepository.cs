using Microsoft.EntityFrameworkCore;
using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using PPC.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Repositories
{
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        public CourseRepository(CCPContext context) : base(context) { }

        public async Task<bool> IsCourseNameExistAsync(string courseName)
        {
            return await _context.Courses.AnyAsync(c => c.Name.ToLower() == courseName.ToLower());
        }

        public async Task<List<Course>> GetAllCoursesWithDetailsAsync()
        {
            var courses = await _context.Courses
                .Include(c => c.CourseSubCategories)
                    .ThenInclude(cs => cs.SubCategory)
                .Include(c => c.Chapters)
                .ToListAsync();

            foreach (var course in courses)
            {
                course.Chapters = course.Chapters?.Where(ch => ch.Status == 1).ToList();
                course.CourseSubCategories = course.CourseSubCategories?
                    .Where(cs => cs.SubCategory != null).ToList();
            }

            return courses;
        }

        public async Task<Course> GetCourseWithAllDetailsAsync(string courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Chapters)
                .Include(c => c.CourseSubCategories)
                    .ThenInclude(cs => cs.SubCategory)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course != null)
            {
                course.Chapters = course.Chapters?
                    .Where(ch => ch.Status == 1)
                    .ToList();

                course.CourseSubCategories = course.CourseSubCategories?
                    .Where(cs => cs.SubCategory != null)
                    .ToList();
            }

            return course;
        }


        public async Task<List<string>> GetEnrolledCourseIdsAsync(string accountId)
        {
            return await _context.EnrollCourses
                .Where(e => e.MemberId == accountId && e.Status == 1)
                .Select(e => e.CourseId)
                .ToListAsync();
        }

        public async Task<List<Course>> GetAllActiveCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.Chapters)
                .Include(c => c.CourseSubCategories)
                    .ThenInclude(cs => cs.SubCategory)
                .Where(c => c.Status == 1)
                .ToListAsync();
        }

        public async Task<List<EnrollCourse>> GetEnrollCoursesByAccountIdAsync(string memberId)
        {
            return await _context.EnrollCourses
                .Include(e => e.Course)
                .Where(e => e.MemberId == memberId && e.Status != -1)
                .ToListAsync();
        }

        public async Task<bool> RateCourseAsync(string courseId, string memberId, int rating, string feedback)
        {
            var enrollCourse = await _context.EnrollCourses
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.MemberId == memberId && e.Status == 1);

            if (enrollCourse == null)
                return false;

            enrollCourse.Rating = rating;
            enrollCourse.Feedback = feedback;
            await _context.SaveChangesAsync();

            // Update course rating and reviews count
            var course = await _context.Courses
                .Include(c => c.EnrollCourses)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return false;

            // Calculate average rating
            var totalReviews = course.EnrollCourses.Count(e => e.Rating.HasValue);
            var averageRating = course.EnrollCourses
                .Where(e => e.Rating.HasValue)
                .Average(e => e.Rating);

            course.Rating = averageRating;
            course.Reviews = totalReviews;

            await _context.SaveChangesAsync();
            return true;
        }

        // Get all reviews for a course
        public async Task<List<EnrollCourse>> GetEnrollCoursesByCourseIdAsync(string courseId)
        {
            return await _context.EnrollCourses
                .Where(e => e.CourseId == courseId && e.Status == 1 && e.Rating.HasValue)
                .Include(e => e.Member)  // Include Member for MemberName
                .ToListAsync();
        }

        public async Task<List<Course>> GetCoursesByCategoriesAsync(List<string> categoryIds)
        {
            return await _context.Courses
                .Where(c => c.CourseSubCategories.Any(cs => categoryIds.Contains(cs.SubCategory.CategoryId)) && c.Status == 1)
                .Include(c => c.CourseSubCategories)
                    .ThenInclude(cs => cs.SubCategory)
                .ToListAsync();
        }

        // Get top-rated courses
        public async Task<List<Course>> GetTopRatedCoursesAsync(int topN)
        {
            return await _context.Courses
                .Where(c => c.Status == 1)
                .OrderByDescending(c => c.Rating)
                .Take(topN)
                .ToListAsync();
        }
    }
}
