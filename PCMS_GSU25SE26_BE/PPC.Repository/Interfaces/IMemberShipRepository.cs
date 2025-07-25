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
    public interface IMemberShipRepository : IGenericRepository<MemberShip>
    {
        Task<bool> IsNameDuplicatedAsync(string name);
        Task<List<MemberShip>> GetAllActiveAsync();
        Task<List<MemberMemberShip>> GetActiveMemberShipsByMemberIdAsync(string memberId);
    }
}
