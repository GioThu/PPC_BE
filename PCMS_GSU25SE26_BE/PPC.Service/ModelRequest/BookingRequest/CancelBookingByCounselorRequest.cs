using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.BookingRequest
{
    public class CancelBookingByCounselorRequest
    {
        public string BookingId { get; set; }
        public string CancelReason { get; set; }
    }
}
