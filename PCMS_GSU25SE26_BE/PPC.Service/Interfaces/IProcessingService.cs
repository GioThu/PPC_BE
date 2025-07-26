using PPC.Service.ModelResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface IProcessingService
    {
        Task<ServiceResponse<string>> CreateProcessingAsync(string chapterId, string memberId);
    }
}
