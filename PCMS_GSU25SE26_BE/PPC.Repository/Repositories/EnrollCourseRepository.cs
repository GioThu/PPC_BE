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
    public class EnrollCourseRepository : GenericRepository<EnrollCourse>, IEnrollCourseRepository
    {
        public EnrollCourseRepository(CCPContext context) : base(context) { }


        public async Task<bool> IsEnrolledAsync(string memberId, string courseId)
        {
            return await _context.EnrollCourses
                .AnyAsync(ec => ec.MemberId == memberId
                             && ec.CourseId == courseId
                             && ec.Status == 1); 
        }

        public async Task<List<EnrollCourse>> GetEnrolledCoursesWithProcessingAsync(string memberId)
        {
            return await _context.EnrollCourses
                .Where(e => e.MemberId == memberId && e.Status == 1)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Chapters)
                .Include(e => e.Course)
                    .ThenInclude(c => c.CourseSubCategories)
                        .ThenInclude(cs => cs.SubCategory)
                .Include(e => e.Processings)
                .ToListAsync();
        }
    }
}
