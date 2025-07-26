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
    public class MemberMemberShipRepository : GenericRepository<MemberMemberShip>, IMemberMemberShipRepository
    {
        public MemberMemberShipRepository(CCPContext context) : base(context)
        {
        }

        public async Task<bool> MemberHasActiveMemberShipAsync(string memberId, string memberShipId)
        {
            var existing = await _context.MemberMemberShips
                .FirstOrDefaultAsync(m =>
                    m.MemberId == memberId &&
                    m.MemberShipId == memberShipId &&
                    m.Status == 1 &&
                    m.ExpiryDate > GetTimeNow());

            return existing != null;
        }

        public async Task<DateTime?> GetMemberShipExpireDateAsync(string memberId, string memberShipId)
        {
            var expire = await _context.MemberMemberShips
                .Where(mms => mms.MemberId == memberId &&
                               mms.MemberShipId == memberShipId &&
                               mms.Status == 1)
                .OrderByDescending(mms => mms.ExpiryDate)
                .Select(mms => mms.ExpiryDate)
                .FirstOrDefaultAsync();

            return expire;
        }

        public async Task<List<MemberMemberShip>> GetActiveMemberShipsByMemberIdAsync(string memberId)
        {
            var now = GetTimeNow();

            return await _context.MemberMemberShips
                .Include(mms => mms.MemberShip)
                .Where(mms =>
                    mms.MemberId == memberId &&
                    mms.Status == 1 &&
                    mms.ExpiryDate > now)
                .ToListAsync();
        }

        public async Task<MemberMemberShip> GetByIdWithMemberShipAsync(string id)
        {
            return await _context.MemberMemberShips
                .Include(m => m.MemberShip)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public DateTime GetTimeNow()
        {
            TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            return vietnamTime;
        }
    }
}
