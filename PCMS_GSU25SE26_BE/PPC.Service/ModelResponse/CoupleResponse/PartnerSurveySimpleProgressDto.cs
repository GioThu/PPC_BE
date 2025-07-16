using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.CoupleResponse
{
    public class PartnerSurveySimpleProgressDto
    {
        public int TotalDone { get; set; }
        public int TotalSurveys { get; set; }
        public bool IsDoneAll { get; set; }
    }
}
