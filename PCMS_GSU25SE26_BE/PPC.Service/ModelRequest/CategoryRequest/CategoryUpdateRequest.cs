using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.CategoryRequest
{
    public class CategoryUpdateRequest
    {
        [Required]
        public string Id { get; set; }

        public string? Name { get; set; }

        [Range(0, 1)]
        public int Status { get; set; }
    }
}
