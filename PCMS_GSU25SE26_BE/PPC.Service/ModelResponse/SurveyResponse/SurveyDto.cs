using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.SurveyResponse
{
    public class SurveyDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Descriptione { get; set; }
        public string Image { get; set; }
        public DateTime? CreateAt { get; set; }
        public int? Status { get; set; }
    }
}
