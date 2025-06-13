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
                .Where(a => a.Email == email && a.Password == password && a.Role == 2 && a.Counselors.Any())
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
            //if (account == null || account.Counselors == null || !account.Counselors.Any() || account.Counselors.First() == null)
            //{
            //    return null;
            //}

            return account;
        }

    }
}
