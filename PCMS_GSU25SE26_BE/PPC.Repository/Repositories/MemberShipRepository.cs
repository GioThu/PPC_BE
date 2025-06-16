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
    public class MemberShipRepository : GenericRepository<MemberShip>, IMemberShipRepository
    {
        public MemberShipRepository(CCPContext context) : base(context) { }

        public async Task<bool> IsNameDuplicatedAsync(string name)
        {
            return await _context.MemberShips.AnyAsync(ms => ms.MemberShipName == name && ms.Status==1);
        }

        public async Task<List<MemberShip>> GetAllActiveAsync()
        {
            return await _context.MemberShips
                .Where(ms => ms.Status == 1)
                .OrderBy(ms => ms.Rank)
                .ToListAsync();
        }
    }
}
