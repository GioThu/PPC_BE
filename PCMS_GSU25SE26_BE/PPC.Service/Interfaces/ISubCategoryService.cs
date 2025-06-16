using PPC.Service.ModelRequest;
using PPC.Service.ModelResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface ISubCategoryService
    {
        Task<ServiceResponse<string>> CreateSubCategoryAsync(SubCategoryCreateRequest request);
    }
}
