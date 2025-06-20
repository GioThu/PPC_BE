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

        public async Task<(List<Counselor>, int)> GetAllPagingAsync(int pageNumber, int pageSize)
        {
            var query = _context.Counselors.AsQueryable();
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.Fullname)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
