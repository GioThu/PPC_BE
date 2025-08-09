using PPC.DAO.Models;
using PPC.Service.ModelRequest.PostRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Mappers
{
    public static class PostMappers
    {
        public static Post ToCreatePost(this PostCreateRequest request, string userId)
        {
            return new Post
            {
                Id = Utils.Utils.GenerateIdModel("Post"),
                Title = request.Title,
                Description = request.Description,
                CreateAt = Utils.Utils.GetTimeNow(),
                Image = request.Image,
                CreateBy = userId,
                Status = 1
            };
        }

        public static void MapUpdate(this Post post, PostUpdateRequest request)
        {
            post.Title = request.Title ?? post.Title;
            post.Description = request.Description ?? post.Description;
            post.Status = request.Status ?? post.Status;
            post.Image = request.Image ?? post.Image;
        }
    }
}
