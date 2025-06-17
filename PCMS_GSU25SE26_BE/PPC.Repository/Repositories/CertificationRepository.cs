using Microsoft.EntityFrameworkCore;
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
    public class CertificationRepository : GenericRepository<Certification>, ICertificationRepository
    {
        public CertificationRepository(CCPContext context) : base(context) { }

        public async Task<List<Certification>> GetByCounselorIdAsync(string counselorId)
        {
            return await _context.Certifications
                .Where(c => c.CounselorId == counselorId)
                .Include(c => c.Counselor)  
                .OrderByDescending(c => c.Time)  
                .ToListAsync();
        }

        public async Task<List<Certification>> GetAllCertificationsAsync()
        {
            return await _context.Certifications
                .Include(c => c.Counselor)  
                .OrderByDescending(c => c.Time)  
                .ToListAsync();
        }

        public async Task<Certification> GetCertificationByIdAsync(string certificationId)
        {
            return await _context.Certifications
                .Where(c => c.Id == certificationId)
                .Include(c => c.Counselor)  
                .FirstOrDefaultAsync(); 
        }


    }
}
