using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PPC.Repository.Interfaces;
using PPC.Service.Mappers;
using PPC.Service.ModelResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public static class NotificationBackground
    {
        public static void FireAndForgetCreate(
            IServiceScopeFactory scopeFactory,
            NotificationCreateItem item,
            ILogger logger = null)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.CreatorId)
                || string.IsNullOrWhiteSpace(item.NotiType)
                || string.IsNullOrWhiteSpace(item.DocNo)
                || string.IsNullOrWhiteSpace(item.Description))
            {
                logger?.LogWarning("Notification input invalid.");
                return;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

                    var noti = NotificationMappers.ToNewNotification(item);
                    await repo.CreateAsync(noti);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "FireAndForgetCreate Notification failed.");
                }
            });
        }

        public static void FireAndForgetCreateMany(
            IServiceScopeFactory scopeFactory,
            IEnumerable<NotificationCreateItem> items,
            ILogger logger = null)
        {
            var list = items?.Where(x =>
                x != null &&
                !string.IsNullOrWhiteSpace(x.CreatorId) &&
                !string.IsNullOrWhiteSpace(x.NotiType) &&
                !string.IsNullOrWhiteSpace(x.DocNo) &&
                !string.IsNullOrWhiteSpace(x.Description)).ToList();

            if (list == null || list.Count == 0)
            {
                logger?.LogWarning("Notification batch input empty/invalid.");
                return;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

                    var notis = NotificationMappers.ToNewNotifications(list);
                    await repo.CreateRangeAsync(notis);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "FireAndForgetCreateMany Notification failed.");
                }
            });
        }
    }
}
