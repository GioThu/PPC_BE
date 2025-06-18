using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface ILiveKitService
    {
        string GenerateLiveKitToken(string room, string id, string name, DateTime startTime, DateTime endTime);
        Task<bool> HandleWebhookAsync(string rawBody, string authorizationHeader);
    }
}
