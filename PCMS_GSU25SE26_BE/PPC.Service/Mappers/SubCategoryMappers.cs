using PPC.DAO.Models;
using PPC.Service.ModelRequest.CategoryRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Mappers
{
    public static class SubCategoryMappers
    {
        public static SubCategory ToCreateSubCategory(this SubCategoryCreateRequest request)
        {
            return new SubCategory
            {
                Id = Utils.Utils.GenerateIdModel("SubCategory"),
                CategoryId = request.CategoryId,
                Name = request.Name,
                Status = 1
            };
        }
    }
}
