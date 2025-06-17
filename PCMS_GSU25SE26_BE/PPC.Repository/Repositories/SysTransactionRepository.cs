using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using PPC.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Repositories
{
    public class SysTransactionRepository : GenericRepository<SysTransaction>, ISysTransactionRepository
    {
        public SysTransactionRepository(CCPContext context) : base(context) { }
    }
}
