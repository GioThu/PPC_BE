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
    public class PersonTypeRepository : GenericRepository<PersonType>, IPersonTypeRepository
    {
        public PersonTypeRepository(CCPContext context) : base(context)
        {
        }
        public async Task<List<PersonType>> GetAllPersonTypesAsync()
        {
            return await _context.PersonTypes
                .Where(pt => pt.Status == 1)
                .ToListAsync();
        }


        public async Task<List<PersonType>> GetPersonTypesBySurveyAsync(string surveyId)
        {
            return await _context.PersonTypes
                .Include(pt => pt.Category)
                .Where(pt => pt.SurveyId == surveyId && pt.Status == 1)
                .ToListAsync();
        }

        public async Task<PersonType> GetPersonTypeByIdAsync(string id)
        {
            return await _context.PersonTypes
                .Include(pt => pt.Category)
                .FirstOrDefaultAsync(pt => pt.Id == id && pt.Status == 1);
        }

        public async Task UpdatePersonTypeAsync(PersonType personType)
        {
            _context.PersonTypes.Update(personType);
            await _context.SaveChangesAsync();
        }

        public async Task<PersonType> GetByNameAndSurveyIdAsync(string name, string surveyId)
        {
            return await _context.PersonTypes
                .Include(pt => pt.Category)
                .FirstOrDefaultAsync(pt => pt.Name == name && pt.SurveyId == surveyId);
        }
    }
}
