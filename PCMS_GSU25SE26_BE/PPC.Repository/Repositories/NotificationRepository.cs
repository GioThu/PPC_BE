using Microsoft.EntityFrameworkCore;
using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using PPC.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Repositories
{


    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(CCPContext context) : base(context)
        {
        }

        public async Task<List<Notification>> GetNotificationsByMemberIdAsync(string id)
        {
            return await _context.Notifications
                .Where(n => n.CreateBy == id) 
                .OrderByDescending(n => n.CreateDate)
                .ToListAsync();
        }

        public async Task CreateRangeAsync(IEnumerable<Notification> entities)
        {
            await _context.Notifications.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(List<Notification> notifications)
        {
            _context.Notifications.UpdateRange(notifications);
            await _context.SaveChangesAsync();
        }
    }
}
