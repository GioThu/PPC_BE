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
            _context.Questions.Remove(question);
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

        public async Task<List<Question>> GetRandomBalancedQuestionsAsync(string surveyId, int count)
        {
            Dictionary<string, List<string>> tagMap = new()
            {
                ["SV001"] = new() { "I", "E", "N", "S", "T", "F", "J", "P" },
                ["SV002"] = new() { "Dominance", "Influence", "Steadiness", "Conscientiousness" },
                ["SV003"] = new() { "Words of Affirmation", "Acts of Service", "Receiving Gifts", "Quality Time", "Physical Touch" },
                ["SV004"] = new() { "Openness", "Conscientiousness", "Extraversion", "Agreeableness", "Neuroticism" }
            };

            if (!tagMap.ContainsKey(surveyId)) return new List<Question>();

            var tags = tagMap[surveyId];

            // Lấy toàn bộ câu hỏi hợp lệ
            var allQuestions = await _context.Questions
                .Include(q => q.Answers)
                .Where(q => q.SurveyId == surveyId && q.Status == 1 &&
                            q.Answers.Any(a => tags.Contains(a.Tag) && a.Score.HasValue))
                .ToListAsync();

            if (allQuestions.Count <= count)
                return allQuestions.OrderBy(_ => Guid.NewGuid()).ToList();

            // Chuẩn bị
            var result = new List<Question>();
            var usedIds = new HashSet<string>();
            var tagScoreTotals = tags.ToDictionary(t => t, t => 0);
            int idealScorePerTag = (int)Math.Round((double)(count * 2) / tags.Count); 

            var shuffled = allQuestions.OrderBy(_ => Guid.NewGuid()).ToList();

            foreach (var q in shuffled)
            {
                if (result.Count >= count) break;
                if (usedIds.Contains(q.Id)) continue;

                // Chuẩn bị cộng điểm giả định
                var tempTagScores = new Dictionary<string, int>(tagScoreTotals);
                bool isAcceptable = false;

                foreach (var ans in q.Answers)
                {
                    if (tags.Contains(ans.Tag) && ans.Score.HasValue)
                    {
                        var currentScore = tempTagScores[ans.Tag];
                        tempTagScores[ans.Tag] = currentScore + ans.Score.Value;
                    }
                }

                // Kiểm tra sau khi cộng, không tag nào vượt ngưỡng
                isAcceptable = tempTagScores.All(kvp => kvp.Value <= idealScorePerTag + 1);

                if (!isAcceptable) continue;

                // Chấp nhận câu hỏi này
                result.Add(q);
                usedIds.Add(q.Id);

                foreach (var ans in q.Answers)
                {
                    if (tags.Contains(ans.Tag) && ans.Score.HasValue)
                    {
                        tagScoreTotals[ans.Tag] += ans.Score.Value;
                    }
                }
            }

            // Nếu chưa đủ, bù thêm câu ngẫu nhiên
            if (result.Count < count)
            {
                var remaining = allQuestions
                    .Where(q => !usedIds.Contains(q.Id))
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(count - result.Count)
                    .ToList();

                result.AddRange(remaining);
            }

            return result.OrderBy(_ => Guid.NewGuid()).Take(count).ToList();
        }

        public async Task<List<Question>> GetQuestionsByQuizIdAsync(string quizId)
        {
            return await _context.Questions
                .Include(q => q.Answers)
                .Where(q => q.QuizId == quizId && q.Status == 1)
                .ToListAsync();
        }
    }
}
