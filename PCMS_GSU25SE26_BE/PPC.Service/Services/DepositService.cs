using AutoMapper;
using Microsoft.AspNetCore.Http;
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
using PPC.Service.Utils;
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
                return ServiceResponse<string>.ErrorResponse("Tài khoản không có ví");
            }

            var deposit = request.ToCreateDeposit(walletId);
            await _depositRepository.CreateAsync(deposit);

            var wallet = await _walletRepository.GetByIdAsync(walletId);
            if (wallet == null)
            {
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy ví");
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

            return ServiceResponse<string>.SuccessResponse("Giao dịch nạp tiền thành công");
        }

        public async Task<ServiceResponse<string>> CreateWithdrawAsync(string accountId, WithdrawCreateRequest request)
        {
            var (walletId, remaining) = await _accountRepository.GetWalletInfoByAccountIdAsync(accountId);
            if (walletId == null)
            {
                return ServiceResponse<string>.ErrorResponse("Tài khoản không có ví");
            }

            if (remaining == null || remaining < request.Total)
            {
                return ServiceResponse<string>.ErrorResponse("Bạn không đủ số dư");
            }

            var withdraw = request.ToCreateWithdraw(walletId);
            await _depositRepository.CreateAsync(withdraw);

            return ServiceResponse<string>.SuccessResponse("Yêu cầu rút tiền đã được tạo thành công");
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
                return ServiceResponse<List<DepositDto>>.ErrorResponse("Không tìm thấy tài khoản");
            }

            if (string.IsNullOrEmpty(account.WalletId))
            {
                return ServiceResponse<List<DepositDto>>.ErrorResponse("Tài khoản không có ví");
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
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy giao dịch");
            }

            if (deposit.Status == 2 || deposit.Status == 3)
            {
                return ServiceResponse<string>.ErrorResponse("Giao dịch đã được xử lý");
            }

            if (request.NewStatus == 2)
            {
                var wallet = await _walletRepository.GetWithAccountByIdAsync(deposit.WalletId);
                if (wallet == null)
                {
                    return ServiceResponse<string>.ErrorResponse("Không tìm thấy ví");
                }

                var withdrawAmount = deposit.Total ?? 0;

                if (withdrawAmount <= 0)
                {
                    return ServiceResponse<string>.ErrorResponse("Số tiền rút không hợp lệ");
                }

                wallet.Remaining ??= 0;

                if (wallet.Remaining < withdrawAmount)
                {
                    return ServiceResponse<string>.ErrorResponse("Số dư không đủ để phê duyệt yêu cầu rút tiền");
                }

                wallet.Remaining -= withdrawAmount;

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

            return ServiceResponse<string>.SuccessResponse("Trạng thái giao dịch nạp tiền đã được cập nhật thành công");
        }

        public async Task<ServiceResponse<string>> CreateVNPayDepositAsync(HttpContext context, string accountId, VnPayRequest request)
        {
            var (walletId, _) = await _accountRepository.GetWalletInfoByAccountIdAsync(accountId);
            if (string.IsNullOrEmpty(walletId))
            {
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy ví");
            }

            // 👇 Lưu accountId và amount vào return URL
            var returnUrl = $"https://ppcbackend.azurewebsites.net/api/Deposit/vnpay-return?accountId={accountId}&amount={request.Amount}";

            string vnp_Url = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            string vnp_TmnCode = "ULVE3NUK";
            string vnp_HashSecret = "REFWL616A23MJOFK118BV7GA6FBS0609";

            TimeZoneInfo vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var orderId = DateTime.Now.Ticks.ToString();
            var createdDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);

            VnPayLib vnpay = new VnPayLib();
            vnpay.AddRequestData("vnp_Version", VnPayLib.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", ((int)(request.Amount * 100)).ToString());
            vnpay.AddRequestData("vnp_BankCode", "VNBANK");
            vnpay.AddRequestData("vnp_CreateDate", createdDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utilss.GetIpAddress(context));
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", $"Nạp tiền {request.Amount} VND - Order {orderId}");
            vnpay.AddRequestData("vnp_OrderType", "other");

            // 👇 Set return URL chứa accountId và amount
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", orderId);

            var paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return ServiceResponse<string>.SuccessResponse(paymentUrl);
        }
    }
}
