using Microsoft.IdentityModel.Tokens;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.Mappers;
using PPC.Service.ModelRequest.AccountRequest;
using PPC.Service.ModelResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ICounselorRepository _counselorRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IJwtService _jwtService;

        public AccountService(
            IAccountRepository accountRepository,
            ICounselorRepository counselorRepository,
            IWalletRepository walletRepository,
            IJwtService jwtService,
            IMemberRepository memberRepository)
        {
            _accountRepository = accountRepository;
            _counselorRepository = counselorRepository;
            _walletRepository = walletRepository;
            _jwtService = jwtService;
            _memberRepository = memberRepository;
        }

        public async Task<ServiceResponse<string>> CounselorLogin(LoginRequest loginRequest)
        {
            try
            {
                var account = await _accountRepository.CounselorLogin(loginRequest.Email, loginRequest.Password);
                if (account == null ||
                    account.Counselors == null ||
                    !account.Counselors.Any())
                {
                    return ServiceResponse<string>.ErrorResponse("Invalid login or counselor not found.");
                }

                var counselor = account.Counselors.First();
                var token = _jwtService.GenerateCounselorToken(account.Id, counselor.Id, counselor.Fullname, account.Role, counselor.Avatar);
                return ServiceResponse<string>.SuccessResponse(token);
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.ErrorResponse("Login failed: " + ex.Message);
            }
        }

        public async Task<ServiceResponse<int>> RegisterCounselorAsync(AccountRegister accountRegister)
        {
            try
            {
                if (await _accountRepository.IsEmailExistAsync(accountRegister.Email))
                {
                    return ServiceResponse<int>.ErrorResponse("Email already exists.");
                }

                var wallet = WalletMappers.ToCreateWallet();
                await _walletRepository.CreateAsync(wallet);

                var account = accountRegister.ToCreateCounselorAccount();
                account.WalletId = wallet.Id;
                await _accountRepository.CreateAsync(account);

                var counselor = CounselorMappers.ToCreateCounselor(accountRegister.FullName, account.Id);
                var resultId = await _counselorRepository.CreateAsyncNoRequest(counselor);

                return ServiceResponse<int>.SuccessResponse(resultId);
            }
            catch (Exception ex)
            {
                return ServiceResponse<int>.ErrorResponse(ex.Message);
            }
        }

        public async Task<ServiceResponse<int>> RegisterMemberAsync(AccountRegister accountRegister)
        {
            try
            {
                if (await _accountRepository.IsEmailExistAsync(accountRegister.Email))
                {
                    return ServiceResponse<int>.ErrorResponse("Email already exists.");
                }

                var wallet = WalletMappers.ToCreateWallet();
                await _walletRepository.CreateAsync(wallet);

                var account = accountRegister.ToCreateMemberAccount();
                account.WalletId = wallet.Id;
                await _accountRepository.CreateAsync(account);

                var member = MemberMappers.ToCreateMember(accountRegister.FullName, account.Id);
                var resultId = await _memberRepository.CreateAsyncNoRequest(member);

                return ServiceResponse<int>.SuccessResponse(resultId);
            }
            catch (Exception ex)
            {
                return ServiceResponse<int>.ErrorResponse(ex.Message);
            }
        }

        public async Task<ServiceResponse<string>> MemberLogin(LoginRequest loginRequest)
        {
            try
            {
                var account = await _accountRepository.MemberLogin(loginRequest.Email, loginRequest.Password);
                if (account == null ||
                    account.Members == null ||
                    !account.Members.Any())
                {
                    return ServiceResponse<string>.ErrorResponse("Invalid login or member not found.");
                }

                var member = account.Members.First();
                var token = _jwtService.GenerateMemberToken(account.Id, member.Id, member.Fullname, account.Role, member.Avatar);
                return ServiceResponse<string>.SuccessResponse(token);
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.ErrorResponse("Login failed: " + ex.Message);
            }
        }

        public async Task<ServiceResponse<string>> AdminLogin(LoginRequest loginRequest)
        {
            try
            {
                var account = await _accountRepository.AdminLogin(loginRequest.Email, loginRequest.Password);
                if (account == null || account.Role != 1)
                {
                    return ServiceResponse<string>.ErrorResponse("Invalid login or admin not found.");
                }

                var token = _jwtService.GenerateAdminToken(account.Id, account.Role);
                return ServiceResponse<string>.SuccessResponse(token);
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.ErrorResponse("Login failed: " + ex.Message);
            }
        }

        public async Task<ServiceResponse<IEnumerable<Account>>> GetAllAccountsAsync()
        {
            try
            {
                var accounts = await _accountRepository.GetAllAsync();
                return ServiceResponse<IEnumerable<Account>>.SuccessResponse(accounts);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<Account>>.ErrorResponse(ex.Message);
            }
        }
    }
}
