using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.BookingRequest
{
    public class UpdateReportMetadataRequest
    {
        public string BookingId { get; set; }
        public string ReportMetadata { get; set; }
    }
}
