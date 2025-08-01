using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface IMemberRepository : IGenericRepository<Member>
    {
        Task<(List<Member>, int)> GetAllPagingAsync(int pageNumber, int pageSize, int? status);
        Task<Member> GetByAccountIdAsync(string accountId);
        Task<Member> GetByIdWithWalletAsync(string memberId);
        Task<Account> GetAccountWithWalletAndMemberAsync(string accountId);
        Task<bool> IsMemberExistsAsync(string memberId);
    }
}
