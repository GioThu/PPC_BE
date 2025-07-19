using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface ICourseSubCategoryRepository : IGenericRepository<CourseSubCategory>
    {
        Task<CourseSubCategory> GetCourseCategoryAsync(string courseId);
        Task<bool> ExistsAsync(string courseId, string subCategoryId);
        Task<CourseSubCategory> GetAsync(string courseId, string subCategoryId);
    }
}
