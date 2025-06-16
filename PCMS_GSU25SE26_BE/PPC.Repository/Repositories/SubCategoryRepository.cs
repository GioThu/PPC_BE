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
    public class SubCategoryRepository : GenericRepository<SubCategory>, ISubCategoryRepository
    {
        public SubCategoryRepository(CCPContext context) : base(context) { }

        public async Task<bool> IsNameExistInCategoryAsync(string name, string categoryId)
        {
            return await _context.SubCategories
                .AnyAsync(sc => sc.Name == name && sc.CategoryId == categoryId);
        }

        public async Task<List<SubCategory>> GetByIdsAsync(List<string> ids)
        {
            return await _context.SubCategories
                .Where(sc => ids.Contains(sc.Id))
                .ToListAsync();
        }
    }
}
