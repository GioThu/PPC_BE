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
    public class AccountRepository : GenericRepository<Account>, IAccountRepository
    {
        public AccountRepository(CCPContext context) : base(context)
        {
        }

        public async Task<Account> CounselorLogin(string email, string password)
        {
            var account = await _context.Accounts
                .Include(a => a.Counselors)
                .Where(a => a.Email == email && a.Password == password && a.Role == 2 && a.Status==1 && a.Counselors.Any())
                .Select(a => new Account
                {
                    Id = a.Id,
                    Email = a.Email,
                    Password = a.Password,
                    Role = a.Role,
                    CreateAt = a.CreateAt,
                    Status = a.Status,
                    WalletId = a.WalletId,
                    Counselors = new List<Counselor> { a.Counselors.FirstOrDefault() }
                })
                .FirstOrDefaultAsync();
            if (account == null || account.Counselors == null || !account.Counselors.Any() || account.Counselors.First() == null)
            {
                return null;
            }

            return account;
        }

        public async Task<Account> MemberLogin(string email, string password)
        {
            var account = await _context.Accounts
                .Include(a => a.Counselors)
                .Where(a => a.Email == email && a.Password == password && a.Role == 3  && a.Status == 1 && a.Members.Any())
                .Select(a => new Account
                {
                    Id = a.Id,
                    Email = a.Email,
                    Password = a.Password,
                    Role = a.Role,
                    CreateAt = a.CreateAt,
                    Status = a.Status,
                    WalletId = a.WalletId,
                    Members = new List<Member> { a.Members.FirstOrDefault() }
                })
                .FirstOrDefaultAsync();

            if (account == null || account.Members == null || !account.Members.Any() || account.Members.First() == null)
            {
                return null;
            }

            return account;
        }

        public async Task<bool> IsEmailExistAsync(string email)
        {
            return await _context.Accounts.AnyAsync(a => a.Email == email);
        }

        public async Task<Account> AdminLogin(string email, string password)
        {
            return await _context.Accounts
                .Where(a => a.Email == email && a.Password == password && a.Role == 1 && a.Status == 1)
                .FirstOrDefaultAsync();
        }

        public async Task<double?> GetRemainingBalanceByAccountIdAsync(string accountId)
        {
            var account = await _context.Accounts
                .Include(a => a.Wallet)
                .FirstOrDefaultAsync(a => a.Id == accountId);

            return account?.Wallet?.Remaining;
        }

        public async Task<(string walletId, double? remaining)> GetWalletInfoByAccountIdAsync(string accountId)
        {
            var account = await _context.Accounts
                .Include(a => a.Wallet)
                .FirstOrDefaultAsync(a => a.Id == accountId);

            return (account?.WalletId, account?.Wallet?.Remaining);
        }

        public async Task<Account> GetAccountByWalletIdAsync(string walletId)
        {
            return await _context.Accounts
                .Include(a => a.Counselors)
                .FirstOrDefaultAsync(a => a.WalletId == walletId);
        }

        public async Task<Account> GetAccountWithWalletAsync(string accountId)
        {
            return await _context.Accounts
                .Include(a => a.Wallet)
                .FirstOrDefaultAsync(a => a.Id == accountId && a.Status == 1);
        }

        public async Task UpdatePasswordAsync(string accountId, string newPassword)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
            if (account == null) return;

            account.Password = newPassword;
            await _context.SaveChangesAsync();
        }
    }
}
