using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface IQuestionRepository : IGenericRepository<Question>
    {
        Task CreateQuestionWithAnswersAsync(Question question, List<Answer> answers);
        Task<(List<Question> items, int total)> GetPagingBySurveyAsync(string surveyId, int page, int pageSize);
        Task<bool> DeleteWithAnswersAsync(string questionId);
        Task<Question> GetQuestionWithAnswersAsync(string questionId);
        Task UpdateQuestionAndAnswersAsync(Question question, List<Answer> newAnswers);
        Task<List<Question>> GetRandomBalancedQuestionsAsync(string surveyId, int count);
        Task<List<Question>> GetQuestionsByQuizIdAsync(string quizId);
    }
}
