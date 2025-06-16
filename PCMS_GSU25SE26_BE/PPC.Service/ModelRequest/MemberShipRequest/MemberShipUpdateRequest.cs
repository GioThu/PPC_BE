using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.MemberShipRequest
{
    public class MemberShipUpdateRequest
    {
        public string Id { get; set; }
        public string MemberShipName { get; set; }
        public int? Rank { get; set; }
        public int? DiscountCourse { get; set; }
        public int? DiscountBooking { get; set; }
        public double? Price { get; set; }
        public int? ExpiryDate { get; set; }
    }
}
