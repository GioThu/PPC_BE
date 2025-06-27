using PPC.Service.ModelResponse.CategoryResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.PersonTypeResponse
{
    public class PersonTypeDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Detail { get; set; }
        public string Image { get; set; }
        public string SurveyId { get; set; }
        public string CategoryId { get; set; }
        public CategoryDto Category { get; set; }
    }
}
