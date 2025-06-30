using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface IResultHistoryRepository : IGenericRepository<ResultHistory>
    {
        Task<ResultHistory> GetLatestResultAsync(string memberId, string surveyId);
        Task<List<ResultHistory>> GetResultHistoriesByMemberAndSurveyAsync(string memberId, string surveyId);
    }

}
