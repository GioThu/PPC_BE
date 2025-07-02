using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface ICounselorRepository : IGenericRepository<Counselor>
    {
        Task<List<Counselor>> GetActiveWithApprovedSubCategoriesAsync();
        Task<(List<Counselor>, int)> GetAllPagingAsync(int pageNumber, int pageSize, int? status);
        Task<Counselor> GetByIdWithWalletAsync(string counselorId);
        Task<List<Counselor>> GetCounselorsByCategoriesAsync(List<string> categoryIds);
        Task<List<Counselor>> GetTopCounselorsAsync(int topN);
    }
}
