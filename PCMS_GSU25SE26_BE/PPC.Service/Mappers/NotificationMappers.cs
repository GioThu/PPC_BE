using PPC.DAO.Models;
using PPC.Service.ModelResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Mappers
{
    public static class NotificationMappers
    {
        public static NotificationDto ToNotificationDto(this Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                NotiType = notification.NotiType,
                Description = notification.Description,
                IsRead = notification.IsRead,
                IsOpen = notification.IsOpen,
                DocNo = notification.DocNo,
                CreateBy = notification.CreateBy,
                CreateDate = notification.CreateDate,
                Status = notification.Status
            };
        }

        public static Notification ToNewNotification(NotificationCreateItem item)
        {
            return new Notification
            {
                Id = Utils.Utils.GenerateIdModel("Notification"),
                NotiType = item.NotiType,
                Description = item.Description,
                IsRead = false,
                IsOpen = false,
                DocNo = item.DocNo,
                CreateBy = item.CreatorId,                
                CreateDate = Utils.Utils.GetTimeNow(),
                Status = 1
            };
        }

        public static List<Notification> ToNewNotifications(IEnumerable<NotificationCreateItem> items)
            => items.Select(ToNewNotification).ToList();
    }
}
