using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface IResultPersonTypeRepository : IGenericRepository<ResultPersonType>
    {
        Task BulkInsertAsync(List<ResultPersonType> entities);
        Task<ResultPersonType> FindResultAsync(string surveyId, string type1Name, string type2Name);
        Task<ResultPersonType> GetByIdWithIncludesAsync(string id);
        Task<List<ResultPersonType>> GetByPersonTypeIdAsync(string personTypeId);
    }
}
