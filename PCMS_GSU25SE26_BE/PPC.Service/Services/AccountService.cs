using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.Mappers;
using PPC.Service.ModelRequest;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


namespace PPC.Service.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ICounselorRepository _counselorRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IJwtService _jwtService;


        public AccountService(IAccountRepository accountRepository, ICounselorRepository counselorRepository, IWalletRepository walletRepository, IJwtService jwtService)
        {
            _accountRepository = accountRepository;
            _counselorRepository = counselorRepository;
            _walletRepository = walletRepository;
            _jwtService = jwtService;
        }

        public async Task<string> CounselorLogin(LoginRequest loginRequest)
        {
            try
            {
                var account = await _accountRepository.CounselorLogin(loginRequest.Email, loginRequest.Password);
                if (account == null || account.Counselors == null || !account.Counselors.Any())
                {
                    throw new Exception("Invalid login or counselor not found.");
                }

                var counselor = account.Counselors.First();
                var token = _jwtService.GenerateCounselorToken(account.Id, counselor.Id, counselor.Fullname, account.Role, counselor.Avatar);
                return token;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Login failed: " + ex.Message, ex);
            }
        }

        public async Task<int> RegisterCounselorAsync(AccountRegister accountRegister)
        {
            var wallet = WalletMappers.ToCreateWallet();
            await _walletRepository.CreateAsync(wallet);

            var account = accountRegister.ToCreateAccount();
            account.WalletId = wallet.Id;
            await _accountRepository.CreateAsync(account);

            var counselor = CounselorMappers.ToCreateCounselor(accountRegister.FullName, account.Id);

            return await _counselorRepository.CreateAsyncNoRequest(counselor); ;
        }

        public async Task<IEnumerable<Account>> GetAllAccountsAsync()
        {
            return await _accountRepository.GetAllAsync();
        }
    }
}
