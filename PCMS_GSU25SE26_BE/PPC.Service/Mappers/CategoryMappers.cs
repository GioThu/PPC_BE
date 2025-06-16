using PPC.DAO.Models;
using PPC.Service.ModelRequest.CategoryRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Mappers
{
    public static class CategoryMappers
    {
        public static Category ToCreateCategory(this CategoryCreateRequest request)
        {
            return new Category
            {
                Id = Utils.Utils.GenerateIdModel("Category"),
                Name = request.Name,
                Status = 1
            };
        }
    }
}
