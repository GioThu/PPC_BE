using PPC.Service.ModelRequest.PostRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.PostResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface IPostService
    {
        Task<ServiceResponse<string>> CreatePostAsync(string userId, PostCreateRequest request);
        Task<ServiceResponse<List<PostDto>>> GetAllPostsAsync();
        Task<ServiceResponse<PostDto>> GetPostByIdAsync(string id);
        Task<ServiceResponse<string>> UpdatePostAsync(PostUpdateRequest request); 
        Task<ServiceResponse<string>> DeletePostAsync(string id);
    }
}
