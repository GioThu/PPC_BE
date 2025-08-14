using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.AccountResponse
{
    public class AccountDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public int? Role { get; set; }
        public int? Status { get; set; }
        public DateTime? CreateAt { get; set; }
        public string WalletId { get; set; }
    }
}
