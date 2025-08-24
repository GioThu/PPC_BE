using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse
{
    public class NotificationCreateItem
    {
        public string CreatorId { get; set; }      
        public string NotiType { get; set; }       
        public string DocNo { get; set; }         
        public string Description { get; set; }    
    }
}
