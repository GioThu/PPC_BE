using PPC.DAO.Models;
using PPC.Service.ModelRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Mappers
{
    public static class AccountMappers
    {
        public static Account ToCreateAccount(this AccountRegister accountRegister)
        {
            return new Account
            {
                Id = Utils.Utils.GenerateIdModel("Account"),
                Email = accountRegister.Email,
                Role = 2,
                Password = accountRegister.Password,
                CreateAt = Utils.Utils.GetTimeNow(),
                Status = 0,
            };
        }
    }
}
