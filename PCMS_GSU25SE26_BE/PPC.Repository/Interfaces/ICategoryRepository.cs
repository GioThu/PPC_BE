using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<bool> IsCategoryNameExistsAsync(string name);
        Task<List<Category>> GetAllWithSubCategoriesAsync();
        Task<List<Category>> GetActiveCategoriesWithActiveSubCategoriesAsync();
    }
}
