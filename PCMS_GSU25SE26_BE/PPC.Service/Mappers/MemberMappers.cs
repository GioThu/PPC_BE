using PPC.DAO.Models;
using PPC.Service.ModelResponse.MemberResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Mappers
{
    public static class MemberMappers
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

        public static MemberProfileDto ToMemberProfileDto(this Member member)
        {
            return new MemberProfileDto
            {
                Fullname = member.Fullname,
                Avatar = member.Avatar,
                Phone = member.Phone,
                Dob = member.Dob,
                Mbti = member.Mbti,
                Disc = member.Disc,
                LoveLanguage = member.LoveLanguage,
                BigFive = member.BigFive,
                Gender = member.Gender
            };
        }
    }
}
