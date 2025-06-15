using PPC.Service.ModelRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface IJwtService
    {
        string GenerateCounselorToken(string accountId, string counselorId, string fullname, int? role, string avartar);
        string GenerateMemberToken(string accountId, string memberId, string fullname, int? role, string avartar);
        string GenerateAdminToken(string accountId,int? role);
    }
}
