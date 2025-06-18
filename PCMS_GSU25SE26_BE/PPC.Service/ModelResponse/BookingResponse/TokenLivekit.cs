using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.BookingResponse
{
    public class TokenLivekit
    {
        public string token { get; set; }
        public string serverUrl { get; set; }

        public TokenLivekit(string Inputtoken)
        {
            token = Inputtoken;
            serverUrl = "wss://bookingtarot-nri9ra2n.livekit.cloud"; ;
        }
    }
}
