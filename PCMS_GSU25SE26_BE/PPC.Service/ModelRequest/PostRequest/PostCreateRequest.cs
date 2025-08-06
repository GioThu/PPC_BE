using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.PostRequest
{
    public class PostCreateRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string? Image { get; set; }
    }
}
