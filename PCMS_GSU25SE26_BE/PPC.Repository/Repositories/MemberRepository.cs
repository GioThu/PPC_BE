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

        public async Task<List<Member>> GetAllWithMemberShipsAsync(string memberId)
        {
            return await _context.Members
                .Where(m => m.Id == memberId)
                .Include(m => m.MemberMemberShips)
                    .ThenInclude(mms => mms.MemberShip)
                .ToListAsync();
        }
    }
}
