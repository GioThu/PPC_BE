using AutoMapper;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.Mappers;
using PPC.Service.ModelRequest.PostRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.PostResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;

        public PostService(IPostRepository postRepository, IMapper mapper)
        {
            _postRepository = postRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<string>> CreatePostAsync(string userId, PostCreateRequest request)
        {
            var post = request.ToCreatePost(userId);
            await _postRepository.CreateAsync(post);
            return ServiceResponse<string>.SuccessResponse("Bài đăng đã được tạo thành công");
        }

        public async Task<ServiceResponse<List<PostDto>>> GetAllPostsAsync()
        {
            var posts = await _postRepository.GetAllAsync();
            var dtos = _mapper.Map<List<PostDto>>(posts);
            return ServiceResponse<List<PostDto>>.SuccessResponse(dtos);
        }

        public async Task<ServiceResponse<PostDto>> GetPostByIdAsync(string id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
                return ServiceResponse<PostDto>.ErrorResponse("Không tìm thấy bài đăng");

            var dto = _mapper.Map<PostDto>(post);
            return ServiceResponse<PostDto>.SuccessResponse(dto);
        }

        public async Task<ServiceResponse<string>> UpdatePostAsync(PostUpdateRequest request)
        {
            var post = await _postRepository.GetByIdAsync(request.Id);
            if (post == null)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy bài đăng");

            post.MapUpdate(request);
            await _postRepository.UpdateAsync(post);

            return ServiceResponse<string>.SuccessResponse("Bài đăng đã được cập nhật thành công");
        }

        public async Task<ServiceResponse<string>> DeletePostAsync(string id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy bài đăng");

            await _postRepository.RemoveAsync(post);
            return ServiceResponse<string>.SuccessResponse("Bài đăng đã được xóa thành công");
        }
    }
}
