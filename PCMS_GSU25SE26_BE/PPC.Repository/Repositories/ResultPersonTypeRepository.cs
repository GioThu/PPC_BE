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

        public async Task<ResultPersonType> FindResultAsync(string surveyId, string type1Name, string type2Name)
        {
            return await _context.ResultPersonTypes
                .Include(r => r.PersonType)
                .Include(r => r.PersonType2)
                .Where(r => r.SurveyId == surveyId &&
                       (
                           (r.PersonType.Name == type1Name && r.PersonType2.Name == type2Name) ||
                           (r.PersonType.Name == type2Name && r.PersonType2.Name == type1Name)
                       ))
                .FirstOrDefaultAsync();
        }

        public async Task<ResultPersonType> GetByIdWithIncludesAsync(string id)
        {
            return await _context.ResultPersonTypes
                .Include(x => x.Category)
                .Include(x => x.PersonType)
                .Include(x => x.PersonType2)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<ResultPersonType>> GetByPersonTypeIdAsync(string personTypeId)
        {
            return await _context.ResultPersonTypes
                .Include(r => r.PersonType)
                .Include(r => r.PersonType2)
                .Include(r => r.Category)
                .Where(r => r.PersonTypeId == personTypeId || r.PersonType2Id == personTypeId)
                .ToListAsync();
        }

    }
}
