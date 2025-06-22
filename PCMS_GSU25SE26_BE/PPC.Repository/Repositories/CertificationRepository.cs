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
        public async Task<(List<Certification>, int)> GetPagedCertificationsAsync(int pageNumber, int pageSize, int? status)
        {
            var query = _context.Certifications
                .Include(c => c.Counselor)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(c => c.Counselor.Status == status.Value);

            var totalCount = await query.CountAsync();

            var pagedData = await query
                .OrderByDescending(c => c.Id) 
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (pagedData, totalCount);
        }


    }
}
