using PPC.Service.ModelResponse.CategoryResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.CounselorResponse
{
    public class CounselorWithSubDto
    {
        public string Id { get; set; }
        public string Fullname { get; set; }
        public string Avatar { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public double Rating { get; set; }
        public int Reviews { get; set; }
        public int YearOfJob  { get; set; }
        public string Phone { get; set; }
        public int? Status { get; set; }
        public bool IsBookingAvailible { get; set; } = true;
        public List<SubCategoryDto> SubCategories { get; set; }
    }
}
