using Microsoft.EntityFrameworkCore;
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
        Task<double?> GetRemainingBalanceByAccountIdAsync(string accountId);

        Task<(string walletId, double? remaining)> GetWalletInfoByAccountIdAsync(string accountId);
        Task<Account> GetAccountByWalletIdAsync(string walletId);
        Task<Account> GetAccountWithWalletAsync(string accountId);
        Task UpdatePasswordAsync(string accountId, string newPassword);


    }
}
