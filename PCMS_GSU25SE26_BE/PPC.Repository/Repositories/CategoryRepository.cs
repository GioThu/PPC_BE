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
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(CCPContext context) : base(context) { }

        public async Task<bool> IsCategoryNameExistsAsync(string name)
        {
            return await _context.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<List<Category>> GetAllWithSubCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.SubCategories)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<Category>> GetActiveCategoriesWithActiveSubCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.Status == 1)
                .Include(c => c.SubCategories.Where(sc => sc.Status == 1)) 
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}
