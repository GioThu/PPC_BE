using AutoMapper;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.ModelResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;

        public NotificationService(INotificationRepository notificationRepository, IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<List<NotificationDto>>> GetNotificationsAsync(string id)
        {
            var notifications = await _notificationRepository.GetNotificationsByMemberIdAsync(id);

            foreach (var notification in notifications)
            {
                notification.IsOpen = true;
            }

            await _notificationRepository.UpdateRangeAsync(notifications);

            var notificationDtos = _mapper.Map<List<NotificationDto>>(notifications);
            return ServiceResponse<List<NotificationDto>>.SuccessResponse(notificationDtos);
        }


        public async Task<NotificationSummaryDto> GetNotificationSummaryAsync(string creatorId)
        {
            // 1) Lấy tất cả theo CreateBy
            var all = await _notificationRepository.GetNotificationsByMemberIdAsync(creatorId);

            // 2) Tính toàn bộ (không giới hạn 3s)
            var unreadCount = all.Count(n => n.IsRead == null || n.IsRead == false);
            var unopenedAll = all.Where(n => n.IsOpen == null || n.IsOpen == false).ToList();
            var unopenedCount = unopenedAll.Count;

            // 3) Lọc list chưa open trong 3 giây gần nhất
            var now = Utils.Utils.GetTimeNow();
            var threshold = now.AddSeconds(-3);

            var unopenedRecent = unopenedAll
                .Where(n => n.CreateDate.HasValue && n.CreateDate.Value >= threshold && n.CreateDate.Value <= now)
                .OrderByDescending(n => n.CreateDate)
                .ToList();

            var hasNewUnopenedInLast3Seconds = unopenedRecent.Count > 0;

            var unopenedDtos = _mapper.Map<List<NotificationDto>>(unopenedRecent);

            return new NotificationSummaryDto
            {
                UnreadCount = unreadCount,                     // tổng chưa read
                UnopenedCount = unopenedCount,                 // tổng chưa open
                UnopenedList = unopenedDtos,                   // chỉ các noti chưa open trong 3s
                HasUnopenedInLast3Seconds = hasNewUnopenedInLast3Seconds
            };
        }
    }
}
