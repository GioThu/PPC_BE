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
                Status = 0,
            };
        }
    }
}
