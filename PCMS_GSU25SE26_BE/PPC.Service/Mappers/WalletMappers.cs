using Microsoft.Identity.Client;
using PPC.DAO.Models;
using PPC.Service.ModelRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Mappers
{
    public static class WalletMappers
    {
        public static Wallet ToCreateWallet()
        {
            return new Wallet
            {
                Id = Utils.Utils.GenerateIdModel("Wallet"),
                Remaining = 0,
                Status = 1,
            };
        }
    }
}
