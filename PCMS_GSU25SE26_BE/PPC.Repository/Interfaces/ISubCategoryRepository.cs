using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface ISubCategoryRepository : IGenericRepository<SubCategory>
    {
        Task<bool> IsNameExistInCategoryAsync(string name, string categoryId);
        Task<List<SubCategory>> GetByIdsAsync(List<string> ids);

    }
}
