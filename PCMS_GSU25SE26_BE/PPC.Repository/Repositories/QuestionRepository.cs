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
    public class QuestionRepository : GenericRepository<Question>, IQuestionRepository
    {
        public QuestionRepository(CCPContext context) : base(context) { }

        public async Task CreateQuestionWithAnswersAsync(Question question, List<Answer> answers)
        {
            await _context.Questions.AddAsync(question);
            await _context.Answers.AddRangeAsync(answers);
            await _context.SaveChangesAsync();
        }

        public async Task<(List<Question> items, int total)> GetPagingBySurveyAsync(string surveyId, int page, int pageSize)
        {
            var query = _context.Questions
                .Include(q => q.Answers)
                .Where(q => q.SurveyId == surveyId && q.Status == 1);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(q => q.CreateAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<bool> DeleteWithAnswersAsync(string questionId)
        {
            var question = await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question == null) return false;

            _context.Answers.RemoveRange(question.Answers);
            question.Status = 0;
            _context.Questions.Update(question);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Question> GetQuestionWithAnswersAsync(string questionId)
        {
            return await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == questionId && q.Status == 1);
        }

        public async Task UpdateQuestionAndAnswersAsync(Question question, List<Answer> newAnswers)
        {
            var oldAnswers = _context.Answers.Where(a => a.QuestionId == question.Id);
            _context.Answers.RemoveRange(oldAnswers);

            await _context.Answers.AddRangeAsync(newAnswers);
            _context.Questions.Update(question);
            await _context.SaveChangesAsync();
        }
    }
}
