using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.BookingResponse
{
    public class BookingRatingFeedbackDto
    {
        public int Rating { get; set; }
        public string Feedback { get; set; }
        public DateTime? TimeEnd { get; set; }
        public string MemberFullName { get; set; }
    }

}
