using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.BookingRequest
{
    public class BookingRatingRequest
    {
        public string BookingId { get; set; }
        public int Rating { get; set; } // từ 1 đến 5
        public string Feedback { get; set; }
    }
}
