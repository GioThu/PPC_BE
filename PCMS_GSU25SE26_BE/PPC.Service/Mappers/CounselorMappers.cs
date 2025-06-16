using PPC.DAO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Mappers
{
    public class CounselorMappers
    {
        public static Counselor ToCreateCounselor(string fullname, string accountId)
        {
            return new Counselor
            {
                Id = Utils.Utils.GenerateIdModel("Counselor"),
                AccountId = accountId,
                Fullname = fullname,
                Avatar = "https://trilieutaman.vn/wp-content/uploads/2025/02/TAM-AN-WEB-BAI-1.png",
                Description = "Một Counselor tuyệt vời",
                YearOfJob = 0,
                Price = 0,
                Rating = 0,
                Status = 0,
            };
        }
    }
}
