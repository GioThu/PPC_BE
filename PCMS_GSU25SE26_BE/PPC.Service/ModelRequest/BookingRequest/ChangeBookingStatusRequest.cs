using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.BookingRequest
{
    public class ChangeBookingStatusRequest
    {
        public string BookingId { get; set; }
        public int Status { get; set; } 
    }
}
