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
    }
}
