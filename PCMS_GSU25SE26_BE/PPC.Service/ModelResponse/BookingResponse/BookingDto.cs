using PPC.Service.ModelResponse.CategoryResponse;
using PPC.Service.ModelResponse.CounselorResponse;
using PPC.Service.ModelResponse.MemberResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.ModelResponse.BookingResponse
{
    public class BookingDto
    {
        public string Id { get; set; }

        public string MemberId { get; set; }

        public string Member2Id { get; set; }

        public string CounselorId { get; set; }

        public string Note { get; set; }

        public DateTime? TimeStart { get; set; }

        public DateTime? TimeEnd { get; set; }

        public double? Price { get; set; }

        public string CancelReason { get; set; }

        public DateTime? CreateAt { get; set; }

        public int? Rating { get; set; }

        public string Feedback { get; set; }

        public bool? IsCouple { get; set; }

        public string ProblemSummary { get; set; }

        public string ProblemAnalysis { get; set; }

        public string Guides { get; set; }

        public bool? IsReport { get; set; }

        public string ReportMessage { get; set; }

        public int? Status { get; set; }
        public MemberDto Member { get; set; }
        public MemberDto Member2 { get; set; }
        public CounselorDto Counselor { get; set; }
        public List<SubCategoryDto> SubCategories { get; set; }
    }
}
