using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.CategoryRequest
{
    public class SubCategoryCreateRequest
    {
        [Required]
        public string CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }

}
