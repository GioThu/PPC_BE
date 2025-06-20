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

        public async Task<(List<Member>, int)> GetAllPagingAsync(int pageNumber, int pageSize)
        {
            var query = _context.Members.AsQueryable(); // Lấy tất cả, không lọc status
            var total = await query.CountAsync();

            var members = await query
                .OrderBy(m => m.Fullname)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (members, total);
        }
    }
}
