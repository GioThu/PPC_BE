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
    public class DepositRepository : GenericRepository<Deposit>, IDepositRepository
    {
        public DepositRepository(CCPContext context) : base(context)
        {
        }

        public async Task<List<Deposit>> GetDepositsByStatusAsync(int status)
        {
            return await _context.Deposits
                .Where(d => d.Status == status)
                .ToListAsync();
        }

        public async Task<List<Deposit>> GetDepositsByWalletIdAsync(string walletId)
        {
            return await _context.Deposits
                .Where(d => d.WalletId == walletId)
                .OrderByDescending(d => d.CreateDate)
                .ToListAsync();
        }

        public async Task<Wallet> GetWithAccountByIdAsync(string walletId)
        {
            return await _context.Wallets
                .Include(w => w.Accounts)
                .FirstOrDefaultAsync(w => w.Id == walletId);
        }

    }
}