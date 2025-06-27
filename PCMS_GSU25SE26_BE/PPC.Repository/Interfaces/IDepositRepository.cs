using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface IDepositRepository : IGenericRepository<Deposit>
    {
        Task<List<Deposit>> GetDepositsByStatusAsync(int status);
        Task<List<Deposit>> GetDepositsByWalletIdAsync(string walletId);

    }
}