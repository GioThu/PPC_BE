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
    public class QuizRepository : GenericRepository<Quiz>, IQuizRepository
    {
        public QuizRepository(CCPContext context) : base(context) { }

        public async Task<Quiz> GetByIdWithDetailsAsync(string quizId)
        {
            return await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qs => qs.Answers)
                .FirstOrDefaultAsync(q => q.Id == quizId && q.Status == 1);
        }
    }
}
