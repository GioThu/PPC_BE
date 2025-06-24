using PPC.DAO.Models;
using PPC.Service.ModelResponse.CounselorResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Mappers
{
    public static class CounselorMappers
    {
        public static Counselor ToCreateCounselor(string fullname, string accountId)
        {
            return new Counselor
            {
                Id = Utils.Utils.GenerateIdModel("Counselor"),
                AccountId = accountId,
                Fullname = fullname,
                Avatar = "https://hoseiki.vn/wp-content/uploads/2025/03/avatar-mac-dinh-4.jpg",
                Description = "Một Counselor tuyệt vời",
                YearOfJob = 0,
                Price = 0,
                Rating = 0,
                Status = 0,
            };
        }

        public static CounselorDto ToCounselorDto(this Counselor counselor)
        {
            return new CounselorDto
            {
                Id = counselor.Id,
                Fullname = counselor.Fullname,
                Description = counselor.Description,
                YearOfJob = counselor.YearOfJob,
                Price = counselor.Price,
                Avatar = counselor.Avatar,
                Phone = counselor.Phone,
            };
        }
    }
}
