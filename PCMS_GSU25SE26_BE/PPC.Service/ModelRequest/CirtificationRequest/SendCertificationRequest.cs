using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest.CirtificationRequest
{
    public class SendCertificationRequest
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Image { get; set; }

        public string Description { get; set; }

        public DateTime? Time { get; set; }

        [Required]
        public List<string> SubCategoryIds { get; set; }
    }
}
