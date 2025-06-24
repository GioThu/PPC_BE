using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface ISysTransactionRepository : IGenericRepository<SysTransaction>
    {
        Task<(List<SysTransaction> Items, int TotalCount)> GetTransactionsByAccountAsync(string accountId, string? transactionType, int pageNumber, int pageSize);
    }
}
