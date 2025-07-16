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
    public class ResultPersonTypeRepository : GenericRepository<ResultPersonType>, IResultPersonTypeRepository
    {
        public ResultPersonTypeRepository(CCPContext context) : base(context)
        {
        }

        public async Task BulkInsertAsync(List<ResultPersonType> entities)
        {
            await _context.ResultPersonTypes.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }
    }
}
