using PPC.DAO.Models;
using PPC.Service.ModelRequest.CirtificationRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Mappers
{
    public static class CertificationMappers
    {
        public static Certification ToCertification(this SendCertificationRequest request, string counselorId)
        {
            return new Certification
            {
                Id = Utils.Utils.GenerateIdModel("Certification"),
                CounselorId = counselorId,
                Name = request.Name,
                Image = request.Image,
                Description = request.Description,
                Time = request.Time,
                CreateAt = Utils.Utils.GetTimeNow(),
                Status = 0
            };
        }

        public static List<CounselorSubCategory> ToCounselorSubCategories(this SendCertificationRequest request, string counselorId, string certificationId)
        {
            return request.SubCategoryIds.Select(subId => new CounselorSubCategory
            {
                Id = Utils.Utils.GenerateIdModel("CounselorSubCategory"),
                CounselorId = counselorId,
                CertifivationId = certificationId,
                SubCategoryId = subId,
                Status = 0
            }).ToList();
        }
    }
}
