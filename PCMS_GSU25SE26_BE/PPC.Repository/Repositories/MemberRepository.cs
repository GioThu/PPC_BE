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
    public class MemberRepository : GenericRepository<Member>, IMemberRepository
    {
        public MemberRepository(CCPContext context) : base(context)
        {
        }

        public async Task<(List<Member>, int)> GetAllPagingAsync(int pageNumber, int pageSize, int? status)
        {
            var query = _context.Members.AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(m => m.Status == status.Value);
            }

            var totalCount = await query.CountAsync();

            var members = await query
                .OrderByDescending(m => m.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (members, totalCount);
        }

        public async Task<Member> GetByAccountIdAsync(string accountId)
        {
            return await _context.Members.FirstOrDefaultAsync(m => m.AccountId == accountId);
        }

        public async Task<Member> GetByIdWithWalletAsync(string memberId)
        {
            return await _context.Members
                .Include(m => m.Account)
                    .ThenInclude(a => a.Wallet)
                .FirstOrDefaultAsync(m => m.Id == memberId);
        }
        public async Task<Account> GetAccountWithWalletAndMemberAsync(string accountId)
        {
            return await _context.Accounts
                .Include(a => a.Wallet)
                .Include(a => a.Members)
                .FirstOrDefaultAsync(a => a.Id == accountId && a.Status == 1);
        }

        public async Task<bool> IsMemberExistsAsync(string memberId)
        {
            return await _context.Members.AnyAsync(m => m.Id == memberId && m.Status == 1);
        }


    }
}
