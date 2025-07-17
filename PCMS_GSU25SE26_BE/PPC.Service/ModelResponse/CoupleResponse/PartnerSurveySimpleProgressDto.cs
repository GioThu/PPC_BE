using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.CoupleResponse
{
    public class PartnerSurveySimpleProgressDto
    {
        public int PartnerTotalDone { get; set; }
        public int PartnerTotalSurveys { get; set; }
        public bool IsPartnerDoneAll { get; set; }

        public int SelfTotalDone { get; set; }
        public int SelfTotalSurveys { get; set; }
        public bool IsSelfDoneAll { get; set; }
    }

}
