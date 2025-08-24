using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<List<Notification>> GetNotificationsByMemberIdAsync(string id);
        Task CreateRangeAsync(IEnumerable<Notification> entities);
        Task UpdateRangeAsync(List<Notification> notifications);
    }
}
