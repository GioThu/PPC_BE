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
    public class ResultHistoryRepository : GenericRepository<ResultHistory>, IResultHistoryRepository
    {
        public ResultHistoryRepository(CCPContext context) : base(context) { }

        public async Task<ResultHistory> GetLatestResultAsync(string memberId, string surveyId)
        {
            return await _context.ResultHistories
                .Where(r => r.MemberId == memberId && r.Type == surveyId)
                .OrderByDescending(r => r.CreateAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<ResultHistory>> GetResultHistoriesByMemberAndSurveyAsync(string memberId, string surveyId)
        {
            return await _context.ResultHistories
                .Where(r => r.MemberId == memberId && r.Type == surveyId)
                .OrderByDescending(r => r.CreateAt)
                .ToListAsync();
        }
    }
}
