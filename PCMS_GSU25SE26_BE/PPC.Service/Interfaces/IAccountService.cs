using PPC.DAO.Models;
using PPC.Service.ModelRequest.AccountRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.CounselorResponse;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PPC.Service.Interfaces
{
    public interface IAccountService
    {
        Task<ServiceResponse<int>> RegisterCounselorAsync(AccountRegister accountRegister);
        Task<ServiceResponse<string>> CounselorLogin(LoginRequest loginRequest);
        Task<ServiceResponse<int>> RegisterMemberAsync(AccountRegister accountRegister);
        Task<ServiceResponse<string>> MemberLogin(LoginRequest loginRequest);
        Task<ServiceResponse<IEnumerable<Account>>> GetAllAccountsAsync();
        Task<ServiceResponse<string>> AdminLogin(LoginRequest loginRequest);
        Task<ServiceResponse<CounselorDto>> CounselorGetMyProfileAsync(string counselorId);
        Task<ServiceResponse<string>> CounselorEditMyProfileAsync(string counselorId, CounselorEditRequest request);
        Task<ServiceResponse<WalletBalanceDto>> GetWalletBalanceAsync(string accountId);
        Task<ServiceResponse<string>> ChangePasswordAsync(string accountId, ChangePasswordRequest request);


    }
}
