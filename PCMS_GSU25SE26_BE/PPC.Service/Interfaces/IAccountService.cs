using PPC.DAO.Models;
using PPC.Service.ModelRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface IAccountService
    {
        Task<int> RegisterCounselorAsync(AccountRegister accountRegister);
        Task<string> CounselorLogin (LoginRequest loginRequest);

        Task<IEnumerable<Account>> GetAllAccountsAsync();

    }
}
