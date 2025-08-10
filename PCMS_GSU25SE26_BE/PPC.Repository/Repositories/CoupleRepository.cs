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
    public class CoupleRepository : GenericRepository<Couple>, ICoupleRepository
    {
        public CoupleRepository(CCPContext context) : base(context)
        {
        }

        public async Task<List<Couple>> GetCouplesByMemberIdWithMembersAsync(string memberId)
        {
            return await _context.Couples
                .Include(c => c.MemberNavigation)
                .Include(c => c.Member1Navigation)
                .Where(c => c.Member == memberId || c.Member1 == memberId)
                .OrderByDescending(c => c.CreateAt)
                .ToListAsync();
        }

        public async Task<Couple> GetByAccessCodeAsync(string accessCode)
        {
            return await _context.Couples
                .FirstOrDefaultAsync(c => c.AccessCode == accessCode);
        }

        public async Task<Couple> GetCoupleByIdWithMembersAsync(string coupleId)
        {
            return await _context.Couples
                .Include(c => c.MemberNavigation)
                .Include(c => c.Member1Navigation)
                .FirstOrDefaultAsync(c => c.Id == coupleId);
        }

        public async Task<Couple> GetLatestCoupleByMemberIdAsync(string memberId)
        {
            return await _context.Couples
                .Where(c => c.Member == memberId || c.Member1 == memberId)
                .OrderByDescending(c => c.CreateAt)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasActiveCoupleAsync(string memberId)
        {
            var couple = await _context.Couples
        .Where(c => c.Member == memberId || c.Member1 == memberId)
        .OrderByDescending(c => c.CreateAt)
        .FirstOrDefaultAsync();

            return couple != null && couple.Status == 1;
        }

        public async Task<Couple> GetLatestCoupleByMemberIdWithMembersAsync(string memberId)
        {
            return await _context.Couples
                .Include(c => c.MemberNavigation)
                .Include(c => c.Member1Navigation)
                .Where(c => c.Member == memberId || c.Member1 == memberId)
                .OrderByDescending(c => c.CreateAt)
                .FirstOrDefaultAsync();
        }

        public async Task<int?> GetLatestCoupleStatusByMemberIdAsync(string memberId)
        {
            return await _context.Couples
                .Where(c => c.Member == memberId || c.Member1 == memberId)
                .OrderByDescending(c => c.CreateAt)
                .Select(c => (int?)c.Status)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Couple>> GetCouplesByMemberIdAsync(string memberId)
        {
            return await _context.Couples
                .Include(c => c.MemberNavigation)
                .Include(c => c.Member1Navigation)
                .Where(c => c.Member == memberId || c.Member1 == memberId)
                .OrderByDescending(c => c.CreateAt)
                .ToListAsync();
        }

        public async Task<Couple> GetCoupleWithMembersByIdAsync(string id)
        {
            return await _context.Couples
                .Include(c => c.MemberNavigation)
                .Include(c => c.Member1Navigation)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Couple> GetLatestCoupleByMembersWithIncludesAsync(string memberA, string memberB, int status)
        {
            return await _context.Couples
                .Include(c => c.MemberNavigation)
                .Include(c => c.Member1Navigation)
                .Where(c => c.Status == status &&
                            ((c.Member == memberA && c.Member1 == memberB) ||
                             (c.Member == memberB && c.Member1 == memberA)))
                .OrderByDescending(c => c.CreateAt ?? DateTime.MinValue)
                .ThenByDescending(c => c.Id)
                .FirstOrDefaultAsync();
        }
    }
}
