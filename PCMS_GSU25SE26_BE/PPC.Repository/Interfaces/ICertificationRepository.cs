using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Interfaces
{
    public interface ICertificationRepository : IGenericRepository<Certification> 
    {
        Task<List<Certification>> GetByCounselorIdAsync(string counselorId);
        Task<List<Certification>> GetAllCertificationsAsync();
        Task<Certification> GetCertificationByIdAsync(string certificationId);
        Task<(List<Certification> Certifications, int TotalCount)> GetPagedCertificationsAsync(int pageNumber, int pageSize, int? status);

    }
}
