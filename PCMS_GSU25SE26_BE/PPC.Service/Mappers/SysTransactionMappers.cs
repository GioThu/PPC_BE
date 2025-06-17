using PPC.DAO.Models;
using PPC.Service.ModelRequest.TransactionRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Mappers
{
    public static class SysTransactionMapper
    {
        public static SysTransaction ToCreateSysTransaction(this SysTransactionCreateRequest request)
        {
            return new SysTransaction
            {
                Id = Utils.Utils.GenerateIdModel("SysTrans"),
                TransactionType = request.TransactionType,
                DocNo = request.DocNo,
                CreateBy = request.CreateBy,
                CreateDate = Utils.Utils.GetTimeNow()
            };
        }
    }

}
