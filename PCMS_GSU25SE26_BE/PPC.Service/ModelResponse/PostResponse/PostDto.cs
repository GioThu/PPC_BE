using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.PostResponse
{
    public class PostDto
    {
        public string Id { get; set; }
        public string CreateBy { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public DateTime? CreateAt { get; set; }
        public int? Status { get; set; }
    }
}
