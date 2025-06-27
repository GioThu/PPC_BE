using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.PersonTypeRequest
{
    public class PersonTypeUpdateRequest
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Detail { get; set; }
        public string Image { get; set; }
        public string CategoryId { get; set; }

    }
}
