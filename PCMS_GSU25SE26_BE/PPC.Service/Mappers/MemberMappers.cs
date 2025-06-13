using PPC.DAO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Mappers
{
    public class MemberMappers
    {
        public static Member ToCreateMember(string fullname, string accountId)
        {
            return new Member
            {
                Id = Utils.Utils.GenerateIdModel("Member"),
                AccountId = accountId,
                Fullname = fullname,
                Status = 1,
            };
        }
    }
}
