using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.BookingRequest
{
    public class BookingNoteUpdateRequest
    {
        public string BookingId { get; set; }
        public string ProblemSummary { get; set; }
        public string ProblemAnalysis { get; set; }
        public string Guides { get; set; }
    }
}
