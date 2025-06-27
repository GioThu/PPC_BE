using PPC.DAO.Models;
using PPC.Service.ModelRequest.DepositRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Mappers
{
    public static class DepositMappers
    {
        public static Deposit ToCreateDeposit(this DepositCreateRequest request, string walletId)
        {
            return new Deposit
            {
                Id = Utils.Utils.GenerateIdModel("Deposit"),
                WalletId = walletId,
                Total = request.Total,
                CreateDate = Utils.Utils.GetTimeNow(),
                Status = 0 
            };
        }

        public static Deposit ToCreateWithdraw(this WithdrawCreateRequest request, string walletId)
        {
            return new Deposit
            {
                Id = Utils.Utils.GenerateIdModel("Withdraw"),
                WalletId = walletId,
                Total =  request.Total,
                Stk = request.Stk,
                BankName = request.BankName,
                AccountName = request.AccountName,
                CreateDate = Utils.Utils.GetTimeNow(),
                Status = 1 
            };
        }
    }
}
