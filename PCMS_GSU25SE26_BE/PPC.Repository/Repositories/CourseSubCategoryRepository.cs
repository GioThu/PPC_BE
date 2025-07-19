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
    public class CourseSubCategoryRepository : GenericRepository<CourseSubCategory>, ICourseSubCategoryRepository
    {
        public CourseSubCategoryRepository(CCPContext context) : base(context) { }

        public async Task<CourseSubCategory> GetCourseCategoryAsync(string courseId)
        {
            return await _context.CourseSubCategories
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<bool> ExistsAsync(string courseId, string subCategoryId)
        {
            return await _context.CourseSubCategories
                .AnyAsync(c => c.CourseId == courseId && c.SubCategoryId == subCategoryId);
        }

        public async Task<CourseSubCategory> GetAsync(string courseId, string subCategoryId)
        {
            return await _context.CourseSubCategories
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.SubCategoryId == subCategoryId);
        }
    }
}
