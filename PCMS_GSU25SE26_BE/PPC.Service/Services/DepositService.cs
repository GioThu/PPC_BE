using AutoMapper;
using PPC.Repository.Interfaces;
using PPC.Repository.Repositories;
using PPC.Service.Interfaces;
using PPC.Service.Mappers;
using PPC.Service.ModelRequest.DepositRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.CounselorResponse;
using PPC.Service.ModelResponse.DepositResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class DepositService : IDepositService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IDepositRepository _depositRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IMapper _mapper;

        public DepositService(
            IAccountRepository accountRepository,
            IDepositRepository depositRepository,
            IWalletRepository walletRepository,
            IMapper mapper)
        {
            _accountRepository = accountRepository;
            _depositRepository = depositRepository;
            _walletRepository = walletRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<string>> CreateDepositAsync(string accountId, DepositCreateRequest request)
        {
            var (walletId, remaining) = await _accountRepository.GetWalletInfoByAccountIdAsync(accountId);
            if (walletId == null)
            {
                return ServiceResponse<string>.ErrorResponse("Account does not have a wallet.");
            }

            var deposit = request.ToCreateDeposit(walletId);
            await _depositRepository.CreateAsync(deposit);

            var wallet = await _walletRepository.GetByIdAsync(walletId);
            if (wallet == null)
            {
                return ServiceResponse<string>.ErrorResponse("Wallet not found.");
            }

            if (wallet.Remaining == null)
            {
                wallet.Remaining = 0;
            }

            wallet.Remaining += request.Total;
            await _walletRepository.UpdateAsync(wallet);

            return ServiceResponse<string>.SuccessResponse("Deposit created successfully.");
        }

        public async Task<ServiceResponse<string>> CreateWithdrawAsync(string accountId, WithdrawCreateRequest request)
        {
            var (walletId, remaining) = await _accountRepository.GetWalletInfoByAccountIdAsync(accountId);
            if (walletId == null)
            {
                return ServiceResponse<string>.ErrorResponse("Account does not have a wallet.");
            }

            if (remaining == null || remaining < request.Total)
            {
                return ServiceResponse<string>.ErrorResponse("Insufficient balance for withdrawal.");
            }

            var withdraw = request.ToCreateWithdraw(walletId);
            await _depositRepository.CreateAsync(withdraw);

            return ServiceResponse<string>.SuccessResponse("Withdrawal request created successfully.");
        }

        public async Task<ServiceResponse<List<DepositDto>>> GetDepositsByStatusAsync(int status)
        {
            var deposits = await _depositRepository.GetDepositsByStatusAsync(status);
            var depositDtos = new List<DepositDto>();

            foreach (var deposit in deposits)
            {
                var depositDto = _mapper.Map<DepositDto>(deposit);

                var account = await _accountRepository.GetAccountByWalletIdAsync(deposit.WalletId);
                if (account != null && account.Counselors.Any())
                {
                    var counselor = account.Counselors.FirstOrDefault();
                    if (counselor != null)
                    {
                        var counselorDto = _mapper.Map<CounselorDto>(counselor);
                        depositDto.Counselor = counselorDto;
                    }
                }

                depositDtos.Add(depositDto);
            }

            return ServiceResponse<List<DepositDto>>.SuccessResponse(depositDtos);
        }
    }
}
