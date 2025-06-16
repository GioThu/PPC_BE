using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface ICounselorSubCategoryRepository : IGenericRepository<CounselorSubCategory>
    {
        Task<List<CounselorSubCategory>> GetByCertificationIdAsync(string certificationId);
        Task<List<SubCategory>> GetSubCategoriesByCertificationIdAsync(string certificationId);
        Task<bool> RemoveByCertificationIdAsync(string certificationId);
        Task<bool> HasAnyApprovedSubCategoryAsync(string counselorId);
        Task<List<SubCategory>> GetApprovedSubCategoriesByCounselorAsync(string counselorId);


    }

}
