using PPC.Service.ModelResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface INotificationService
    {
        Task<ServiceResponse<List<NotificationDto>>> GetNotificationsAsync(string id);
        Task<NotificationSummaryDto> GetNotificationSummaryAsync(string creatorId);
    }
}
