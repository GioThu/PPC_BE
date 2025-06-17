using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.CategoryRequest
{
    public class SubCategoryUpdateRequest
    {
        public string Id { get; set; } 
        public string Name { get; set; } 
        public int Status { get; set; } 
        public string CategoryId { get; set; } 
    }
}
