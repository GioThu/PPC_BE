using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse
{
    public class CounselorDto
    {
        public string Id { get; set; }
        public string? Fullname { get; set; }
        public string? Avatar { get; set; }
        public string? Description { get; set; }
        public string? Phone { get; set; }
        public int? Status { get; set; }
    }
}
