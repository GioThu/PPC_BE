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
    public class SurveyRepository : GenericRepository<Survey>, ISurveyRepository
    {
        public SurveyRepository(CCPContext context) : base(context) { }

        public async Task<List<Survey>> GetAllSurveysAsync()
        {
            return await _context.Surveys
                .OrderByDescending(s => s.CreateAt)
                .ToListAsync();
        }
    }
}
