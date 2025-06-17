using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.BookingResponse
{
    public class BookingResultDto
    {
        public string BookingId { get; set; }
        public double Price { get; set; }
        public double? Remaining { get; set; }
        public string TransactionId { get; set; }
        public string Message { get; set; }
    }
}
