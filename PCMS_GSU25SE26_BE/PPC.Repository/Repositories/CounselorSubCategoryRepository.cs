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
    public class CounselorSubCategoryRepository : GenericRepository<CounselorSubCategory>, ICounselorSubCategoryRepository
    {
        public CounselorSubCategoryRepository(CCPContext context) : base(context) { }

        public async Task<List<CounselorSubCategory>> GetByCertificationIdAsync(string certificationId)
        {
            return await _context.CounselorSubCategories
                .Where(csc => csc.CertifivationId == certificationId)
                .ToListAsync();
        }

        public async Task<List<SubCategory>> GetSubCategoriesByCertificationIdAsync(string certificationId)
        {
            return await _context.CounselorSubCategories
                .Where(csc => csc.CertifivationId == certificationId)
                .Include(csc => csc.SubCategory)        
                .ThenInclude(sc => sc.Category)         
                .Select(csc => csc.SubCategory)
                .ToListAsync();
        }

        public async Task<bool> RemoveByCertificationIdAsync(string certificationId)
        {
            var entities = await _context.CounselorSubCategories
                .Where(csc => csc.CertifivationId == certificationId)
                .ToListAsync();

            if (!entities.Any())
                return true;

            _context.CounselorSubCategories.RemoveRange(entities);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasAnyApprovedSubCategoryAsync(string counselorId)
        {
            return await _context.CounselorSubCategories
                .AnyAsync(csc => csc.CounselorId == counselorId && csc.Status == 1);
        }

        public async Task<List<SubCategory>> GetApprovedSubCategoriesByCounselorAsync(string counselorId)
        {
            return await _context.CounselorSubCategories
                .Where(csc => csc.CounselorId == counselorId && csc.Status == 1)
                .Include(csc => csc.SubCategory)
                .Select(csc => csc.SubCategory)
                .ToListAsync();
        }
    }
}
