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
    }
}
