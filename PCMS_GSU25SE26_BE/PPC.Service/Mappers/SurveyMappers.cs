using PPC.DAO.Models;
using PPC.Service.ModelResponse.SurveyResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Mappers
{
    public static class SurveyMapper
    {
        public static SurveyDto ToSurveyDto(this Survey survey)
        {
            return new SurveyDto
            {
                Id = survey.Id,
                Name = survey.Name,
                Description = survey.Description,
                Image = survey.Image,
                CreateAt = survey.CreateAt,
                Status = survey.Status
            };
        }
    }
}
