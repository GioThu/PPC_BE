using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public class LectureRepository : GenericRepository<Lecture>, ILectureRepository
    {
        public LectureRepository(CCPContext context) : base(context) { }

    }
}
