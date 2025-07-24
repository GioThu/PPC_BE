using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface IPersonTypeRepository : IGenericRepository<PersonType>
    {
        Task<List<PersonType>> GetAllPersonTypesAsync();
        Task<List<PersonType>> GetPersonTypesBySurveyAsync(string surveyId);
        Task UpdatePersonTypeAsync(PersonType personType);
        Task<PersonType> GetPersonTypeByIdAsync(string id);
        Task<PersonType> GetByNameAndSurveyIdAsync(string name, string surveyId);
    }
}
