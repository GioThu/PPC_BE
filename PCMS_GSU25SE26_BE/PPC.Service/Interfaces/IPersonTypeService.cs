using PPC.Service.ModelRequest.PersonTypeRequest;
using PPC.Service.ModelResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface IPersonTypeService
    {
        Task<ServiceResponse<string>> CreatePersonTypeAsync(CreatePersonTypeRequest request);
    }
}
