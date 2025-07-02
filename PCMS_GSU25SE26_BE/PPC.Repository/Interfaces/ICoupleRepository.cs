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
    }
}
