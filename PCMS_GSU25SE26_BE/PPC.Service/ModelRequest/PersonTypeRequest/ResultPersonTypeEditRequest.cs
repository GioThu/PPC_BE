using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.PersonTypeRequest
{
    public class ResultPersonTypeEditRequest
    {
        public string Id { get; set; }
        public string CategoryId { get; set; }
        public string Description { get; set; }
        public string Detail { get; set; }
        public string? Weaknesses { get; set; }
        public string? StrongPoints { get; set; }
        public int Compatibility { get; set; }
        public string Image { get; set; }
    }
}
