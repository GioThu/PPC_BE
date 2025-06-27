using AutoMapper;
using Microsoft.Identity.Client;
using PPC.DAO.Models;
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
        private readonly ISysTransactionRepository _sysTransactionRepository;

        public DepositService(
            IAccountRepository accountRepository,
            IDepositRepository depositRepository,
            IWalletRepository walletRepository,
            IMapper mapper,
            ISysTransactionRepository sysTransactionRepository)
        {
            _accountRepository = accountRepository;
            _depositRepository = depositRepository;
            _walletRepository = walletRepository;
            _mapper = mapper;
            _sysTransactionRepository = sysTransactionRepository;
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

            var transaction = new SysTransaction
            {
                Id = Utils.Utils.GenerateIdModel("SysTransaction"),
                TransactionType = "9",
                DocNo = deposit.Id,
                CreateBy = accountId,
                CreateDate = Utils.Utils.GetTimeNow()
            };
            await _sysTransactionRepository.CreateAsync(transaction);

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

        public async Task<ServiceResponse<List<DepositDto>>> GetMyDepositsAsync(string accountId)
        {
            // Lấy account
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                return ServiceResponse<List<DepositDto>>.ErrorResponse("Account not found.");
            }

            if (string.IsNullOrEmpty(account.WalletId))
            {
                return ServiceResponse<List<DepositDto>>.ErrorResponse("Account does not have a wallet.");
            }

            var deposits = await _depositRepository.GetDepositsByWalletIdAsync(account.WalletId);
            var depositDtos = _mapper.Map<List<DepositDto>>(deposits);

            var counselor = account.Counselors.FirstOrDefault();
            CounselorDto counselorDto = null;
            if (counselor != null)
            {
                counselorDto = _mapper.Map<CounselorDto>(counselor);
            }

            foreach (var depositDto in depositDtos)
            {
                depositDto.Counselor = counselorDto;
            }

            return ServiceResponse<List<DepositDto>>.SuccessResponse(depositDtos);
        }

        public async Task<ServiceResponse<string>> ChangeDepositStatusAsync(DepositChangeStatusRequest request)
        {
            var deposit = await _depositRepository.GetByIdAsync(request.DepositId);
            if (deposit == null)
            {
                return ServiceResponse<string>.ErrorResponse("Deposit not found.");
            }

            if (deposit.Status == 2 || deposit.Status == 3)
            {
                return ServiceResponse<string>.ErrorResponse("Deposit already processed.");
            }

            if (request.NewStatus == 2)
            {
                var wallet = await _walletRepository.GetWithAccountByIdAsync(deposit.WalletId);
                if (wallet == null)
                {
                    return ServiceResponse<string>.ErrorResponse("Wallet not found.");
                }

                if (deposit.Total > 0)
                {
                    wallet.Remaining ??= 0;
                    wallet.Remaining += deposit.Total;
                }
                else 
                {
                    var withdrawAmount = Math.Abs(deposit.Total ?? 0);
                    if (wallet.Remaining < withdrawAmount)
                    {
                        return ServiceResponse<string>.ErrorResponse("Insufficient balance for withdrawal approval.");
                    }

                    wallet.Remaining -= withdrawAmount;
                }

                await _walletRepository.UpdateAsync(wallet);
                var transaction = new SysTransaction
                {
                    Id = Utils.Utils.GenerateIdModel("SysTransaction"),
                    TransactionType = "8",
                    DocNo = deposit.Id,
                    CreateBy = wallet.Accounts.FirstOrDefault()?.Id,
                    CreateDate = Utils.Utils.GetTimeNow()
                };
                await _sysTransactionRepository.CreateAsync(transaction);
            }

            deposit.Status = request.NewStatus;
            deposit.CancelReason = request.CancelReason;
            await _depositRepository.UpdateAsync(deposit);

            return ServiceResponse<string>.SuccessResponse("Deposit status updated successfully.");
        }
    }
}
