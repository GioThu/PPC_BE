using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse
{
    public class ServiceResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }

        public static ServiceResponse<T> SuccessResponse(T data)
        {
            return new ServiceResponse<T>
            {
                Success = true,
                Data = data,
                Error = null
            };
        }

        public static ServiceResponse<T> ErrorResponse(string error)
        {
            return new ServiceResponse<T>
            {
                Success = false,
                Error = error
            };
        }
    }

}
