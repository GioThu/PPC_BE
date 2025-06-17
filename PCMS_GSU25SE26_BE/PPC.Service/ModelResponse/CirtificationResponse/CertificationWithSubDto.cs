using PPC.DAO.Models;
using PPC.Service.ModelResponse.CategoryResponse;
using PPC.Service.ModelResponse.CounselorResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.CirtificationResponse
{
    public class CertificationWithSubDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public string RejectReason { get; set; }
        public DateTime? Time { get; set; }
        public int? Status { get; set; }
        public CounselorDto Counselor { get; set; }
        public List<CategoryWithSubDto> Categories { get; set; }
    }
    public class CategoryWithSubDto
    {
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<SubCategoryDto> SubCategories { get; set; }
    }
}
