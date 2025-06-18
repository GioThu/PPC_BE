using PPC.Service.ModelRequest.RoomRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.RoomResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface IRoomService
    {
        Task<ServiceResponse<RoomResponse>> CreateRoomAsync(CreateRoomRequest2 request);
    }
}
