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
            return await _context.Courses
                .Include(c => c.Chapters)
                .Include(c => c.CourseSubCategories)
                    .ThenInclude(cs => cs.SubCategory)
                .ToListAsync();
        }

        public async Task<Course> GetCourseWithAllDetailsAsync(string courseId)
        {
            return await _context.Courses
                .Include(c => c.Chapters)
                .Include(c => c.CourseSubCategories)
                    .ThenInclude(cs => cs.SubCategory)
                .FirstOrDefaultAsync(c => c.Id == courseId);
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
    }
}
