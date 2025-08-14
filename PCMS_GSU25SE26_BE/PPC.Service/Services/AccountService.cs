using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.Mappers;
using PPC.Service.ModelRequest.AccountRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.CounselorResponse;
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
        private readonly IMapper _mapper;


        public AccountService(
            IAccountRepository accountRepository,
            ICounselorRepository counselorRepository,
            IWalletRepository walletRepository,
            IJwtService jwtService,
            IMemberRepository memberRepository,
            IMapper mapper)
        {
            _accountRepository = accountRepository;
            _counselorRepository = counselorRepository;
            _walletRepository = walletRepository;
            _jwtService = jwtService;
            _memberRepository = memberRepository;
            _mapper = mapper;
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
                    return ServiceResponse<string>.ErrorResponse("Sai tài khoản hoặc mật khẩu");
                }

                var counselor = account.Counselors.First();
                var token = _jwtService.GenerateCounselorToken(account.Id, counselor.Id, counselor.Fullname, account.Role, counselor.Avatar);
                return ServiceResponse<string>.SuccessResponse(token);
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.ErrorResponse("Đăng nhập thất bại: " + ex.Message);
            }
        }

        public async Task<ServiceResponse<int>> RegisterCounselorAsync(AccountRegister accountRegister)
        {
            try
            {
                if (await _accountRepository.IsEmailExistAsync(accountRegister.Email))
                {
                    return ServiceResponse<int>.ErrorResponse("Email đã tồn tại");
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
                    return ServiceResponse<int>.ErrorResponse("Email đã tồn tại");
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
                    return ServiceResponse<string>.ErrorResponse("Sai tài khoản hoặc mật khẩu");
                }

                var member = account.Members.First();
                var token = _jwtService.GenerateMemberToken(account.Id, member.Id, member.Fullname, account.Role, member.Avatar);
                return ServiceResponse<string>.SuccessResponse(token);
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.ErrorResponse("Đăng nhập thất bại: " + ex.Message);
            }
        }

        public async Task<ServiceResponse<string>> AdminLogin(LoginRequest loginRequest)
        {
            try
            {
                var account = await _accountRepository.AdminLogin(loginRequest.Email, loginRequest.Password);
                if (account == null || account.Role != 1)
                {
                    return ServiceResponse<string>.ErrorResponse("Sai tài khoản hoặc mất khẩu");
                }

                var token = _jwtService.GenerateAdminToken(account.Id, account.Role);
                return ServiceResponse<string>.SuccessResponse(token);
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.ErrorResponse("Đăng nhập thất bại: " + ex.Message);
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

        public async Task<ServiceResponse<CounselorDto>> CounselorGetMyProfileAsync(string counselorId)
        {
            var counselor = await _counselorRepository.GetByIdAsync(counselorId);
            if (counselor == null)
            {
                return ServiceResponse<CounselorDto>.ErrorResponse("Không tìm thấy Tư Vấn Viên");
            }

            var counselorDto = _mapper.Map<CounselorDto>(counselor);
            return ServiceResponse<CounselorDto>.SuccessResponse(counselorDto);
        }

        public async Task<ServiceResponse<string>> CounselorEditMyProfileAsync(string counselorId, CounselorEditRequest request)
        {
            var counselor = await _counselorRepository.GetByIdAsync(counselorId);
            if (counselor == null)
            {
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy tư vấn viên");
            }

            // Update fields based on the request
            counselor.Fullname = request.Fullname ?? counselor.Fullname;
            counselor.Description = request.Description ?? counselor.Description;
            counselor.Price = request.Price ?? counselor.Price;
            counselor.Phone = request.Phone ?? counselor.Phone;
            counselor.Avatar = request.Avatar ?? counselor.Avatar;
            counselor.YearOfJob = request.YearOfJob ?? counselor.YearOfJob;

            await _counselorRepository.UpdateAsync(counselor);

            return ServiceResponse<string>.SuccessResponse("Hồ sơ đã được cập nhật thành công");
        }

        public async Task<ServiceResponse<WalletBalanceDto>> GetWalletBalanceAsync(string accountId)
        {
            var remaining = await _accountRepository.GetRemainingBalanceByAccountIdAsync(accountId);

            if (remaining == null)
                return ServiceResponse<WalletBalanceDto>.ErrorResponse("Không tìm thấy ví hoặc không có dữ liệu số dư");

            return ServiceResponse<WalletBalanceDto>.SuccessResponse(new WalletBalanceDto { Remaining = remaining });
        }

        public async Task<ServiceResponse<string>> ChangePasswordAsync(string accountId, ChangePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                return ServiceResponse<string>.ErrorResponse("Không xác định được tài khoản.");

            if (request.NewPassword == request.CurrentPassword)
                return ServiceResponse<string>.ErrorResponse("Mật khẩu mới phải khác mật khẩu hiện tại.");

            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null)
                return ServiceResponse<string>.ErrorResponse("Tài khoản không tồn tại.");

            if (account.Status == 0)
                return ServiceResponse<string>.ErrorResponse("Tài khoản đang bị vô hiệu.");

            // So sánh chuỗi thuần (theo yêu cầu của bạn)
            if (account.Password != request.CurrentPassword)
                return ServiceResponse<string>.ErrorResponse("Mật khẩu hiện tại không đúng.");

            await _accountRepository.UpdatePasswordAsync(accountId, request.NewPassword);
            return ServiceResponse<string>.SuccessResponse("Đổi mật khẩu thành công.");
        }
    }
}
