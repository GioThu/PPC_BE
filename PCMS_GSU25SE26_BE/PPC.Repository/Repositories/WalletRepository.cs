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
    public class WalletRepository : GenericRepository<Wallet>, IWalletRepository
    {
        public WalletRepository(CCPContext context) : base(context)
        {
        }

        public async Task<Wallet> GetWithAccountByIdAsync(string walletId)
        {
            return await _context.Wallets
                .Include(w => w.Accounts)
                .FirstOrDefaultAsync(w => w.Id == walletId);
        }
    }
}
