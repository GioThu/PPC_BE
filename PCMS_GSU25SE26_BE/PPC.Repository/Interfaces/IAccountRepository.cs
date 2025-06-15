using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface IAccountRepository : IGenericRepository<Account>
    {
        Task<Account> CounselorLogin(string email, string password);
        Task<Account> MemberLogin(string email, string password);
        Task<bool> IsEmailExistAsync(string email);
        Task<Account> AdminLogin(string email, string password);

    }
}
