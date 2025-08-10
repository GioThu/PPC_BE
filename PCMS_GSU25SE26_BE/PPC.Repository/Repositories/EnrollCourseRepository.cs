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
                .Where(e => e.MemberId == memberId)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Chapters.Where(ch => ch.Status == 1))
                        .ThenInclude(ch => ch.Processings)
                .Include(e => e.Course)
                    .ThenInclude(c => c.CourseSubCategories)
                        .ThenInclude(cs => cs.SubCategory)
                .Include(e => e.Processings)
                .ToListAsync();
        }

        public async Task<EnrollCourse> GetEnrollByCourseAndMemberAsync(string courseId, string memberId)
        {
            return await _context.EnrollCourses
                .FirstOrDefaultAsync(e =>
                    e.CourseId == courseId &&
                    e.MemberId == memberId &&
                    e.Status == 1); 
        }

        public async Task<bool> OpenCourseForMemberAsync(string courseId, string memberId)
        {
            var enroll = await _context.EnrollCourses
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.MemberId == memberId && e.Status == 1);

            if (enroll == null)
                return false;

            enroll.IsOpen = true;
            _context.EnrollCourses.Update(enroll);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<EnrollCourse> GetByIdWithCourseAsync(string enrollCourseId)
        {
            return await _context.EnrollCourses
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == enrollCourseId);
        }
    }
}
