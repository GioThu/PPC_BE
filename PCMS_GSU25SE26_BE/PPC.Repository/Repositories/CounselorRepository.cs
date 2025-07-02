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
    public class CounselorRepository : GenericRepository<Counselor>, ICounselorRepository
    {
        public CounselorRepository(CCPContext context) : base(context)
        {
        }
        public async Task<List<Counselor>> GetActiveWithApprovedSubCategoriesAsync()
        {
            return await _context.Counselors
                .Where(c => c.Status == 1)
                .Include(c => c.CounselorSubCategories.Where(csc => csc.Status == 1))
                    .ThenInclude(csc => csc.SubCategory)
                .OrderBy(c => c.Fullname)
                .ToListAsync();
        }

        public async Task<(List<Counselor>, int)> GetAllPagingAsync(int pageNumber, int pageSize, int? status)
        {
            var query = _context.Counselors.AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            var totalCount = await query.CountAsync();

            var entities = await query
                .OrderByDescending(c => c.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (entities, totalCount);
        }

        public async Task<Counselor> GetByIdWithWalletAsync(string counselorId)
        {
            return await _context.Counselors
                .Include(c => c.Account)
                    .ThenInclude(a => a.Wallet)
                .FirstOrDefaultAsync(c => c.Id == counselorId);
        }

        public async Task<List<Counselor>> GetCounselorsByCategoriesAsync(List<string> categoryIds)
        {
            return await _context.Counselors
        .Where(c => c.Status == 1)
        .Include(c => c.CounselorSubCategories)
            .ThenInclude(cs => cs.SubCategory)
            .ThenInclude(sc => sc.Category)
        .Where(c => c.CounselorSubCategories.Any(cs =>
            categoryIds.Contains(cs.SubCategory.CategoryId)
            && cs.SubCategory.Status == 1
            && cs.SubCategory.Category.Status == 1
        ))
        .ToListAsync();
        }

        public async Task<List<Counselor>> GetTopCounselorsAsync(int topN)
        {
            var query = _context.Counselors
                .Where(c => c.Status == 1)
                .Include(c => c.CounselorSubCategories)
                    .ThenInclude(cs => cs.SubCategory)
                        .ThenInclude(s => s.Category)
                .OrderByDescending(c => c.Rating)
                .ThenByDescending(c => c.Reviews)
                .Take(topN);

            return await query.ToListAsync();
        }


    }
}
