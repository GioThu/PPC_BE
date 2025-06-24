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
    public class SysTransactionRepository : GenericRepository<SysTransaction>, ISysTransactionRepository
    {
        public SysTransactionRepository(CCPContext context) : base(context) { }

        public async Task<(List<SysTransaction> Items, int TotalCount)>GetTransactionsByAccountAsync(string accountId, string? transactionType, int pageNumber, int pageSize)
        {
            var query = _context.SysTransactions
                .Where(t => t.CreateBy == accountId);

            if (!string.IsNullOrEmpty(transactionType))
            {
                query = query.Where(t => t.TransactionType == transactionType);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.CreateDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}