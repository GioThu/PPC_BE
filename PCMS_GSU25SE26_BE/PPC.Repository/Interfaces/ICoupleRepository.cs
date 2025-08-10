using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface ICoupleRepository : IGenericRepository<Couple>
    {
        Task<List<Couple>> GetCouplesByMemberIdWithMembersAsync(string memberId);
        Task<Couple> GetByAccessCodeAsync(string accessCode);
        Task<Couple> GetCoupleByIdWithMembersAsync(string coupleId);
        Task<bool> HasActiveCoupleAsync(string memberId);
        Task<Couple> GetLatestCoupleByMemberIdAsync(string memberId);
        Task<Couple> GetLatestCoupleByMemberIdWithMembersAsync(string memberId);
        Task<int?> GetLatestCoupleStatusByMemberIdAsync(string memberId);
        Task<List<Couple>> GetCouplesByMemberIdAsync(string memberId);
        Task<Couple> GetCoupleWithMembersByIdAsync(string id);
        Task<Couple> GetLatestCoupleByMembersWithIncludesAsync(string memberA, string memberB, int status);
    }
}
