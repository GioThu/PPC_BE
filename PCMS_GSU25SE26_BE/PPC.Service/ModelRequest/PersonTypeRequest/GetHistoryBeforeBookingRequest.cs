using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.PersonTypeRequest
{
    public class GetHistoryBeforeBookingRequest
    {
        public string MemberId { get; set; }
        public string SurveyId { get; set; }
        public string BookingId { get; set; }
    }
}
