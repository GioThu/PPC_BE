using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface IMemberMemberShipRepository : IGenericRepository<MemberMemberShip>
    {
        Task<bool> MemberHasActiveMemberShipAsync(string memberId, string memberShipId);
        Task<DateTime?> GetMemberShipExpireDateAsync(string memberId, string memberShipId);
        Task<List<MemberMemberShip>> GetActiveMemberShipsByMemberIdAsync(string memberId);
        Task<MemberMemberShip> GetByIdWithMemberShipAsync(string id);


    }
}
