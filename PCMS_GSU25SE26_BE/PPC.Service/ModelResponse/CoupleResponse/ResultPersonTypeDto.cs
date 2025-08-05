using PPC.DAO.Models;
using PPC.Service.ModelResponse.CategoryResponse;
using PPC.Service.ModelResponse.PersonTypeResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.CoupleResponse
{
    public class ResultPersonTypeDto
    {
        public string Id { get; set; }

        public string SurveyId { get; set; }

        public string CategoryId { get; set; }

        public string PersonTypeId { get; set; }

        public string PersonType2Id { get; set; }

        public string Description { get; set; }

        public string Detail { get; set; }

        public int Compatibility { get; set; }

        public string Image { get; set; }
        public string? Weaknesses { get; set; }
        public string? StrongPoints { get; set; }

        public DateTime? CreateAt { get; set; }

        public int? Status { get; set; }

        public  CategoryDto Category { get; set; }

        public  PersonTypeDto PersonType { get; set; }

        public  PersonTypeDto PersonType2 { get; set; }
    }
}
