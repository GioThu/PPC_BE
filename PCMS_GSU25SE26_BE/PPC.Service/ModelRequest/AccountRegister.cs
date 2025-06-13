using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelRequest
{
    public class AccountRegister
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
    }

}
