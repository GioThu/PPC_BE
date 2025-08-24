using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse
{
    public class NotificationDto
    {
        public string? Id { get; set; }
        public string? NotiType { get; set; }
        public string? Description { get; set; }
        public bool? IsRead { get; set; }
        public bool? IsOpen { get; set; }
        public string? DocNo { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? Status { get; set; }
    }

    public class NotificationSummaryDto
    {
        public int UnreadCount { get; set; }
        public int UnopenedCount { get; set; }
        public bool HasUnopenedInLast3Seconds { get; set; }
        public List<NotificationDto> UnopenedList { get; set; }
    }
}
