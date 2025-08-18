using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.Mappers;
using PPC.Service.ModelRequest.MemberShipRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.MemberShipResponse;
using PPC.DAO.Models;

namespace PPC.Service.Services
{
    public class MemberShipService : IMemberShipService
    {
        private readonly IMemberShipRepository _memberShipRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ISysTransactionRepository _sysTransactionRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IMemberMemberShipRepository _memberMemberShipRepository;

        public MemberShipService(IMemberShipRepository memberShipRepository, IMemberRepository memberRepository, IWalletRepository walletRepository, ISysTransactionRepository sysTransactionRepository, IAccountRepository accountRepository, IMemberMemberShipRepository memberMemberShipRepository)
        {
            _memberShipRepository = memberShipRepository;
            _memberRepository = memberRepository;
            _walletRepository = walletRepository;
            _sysTransactionRepository = sysTransactionRepository;
            _accountRepository = accountRepository;
            _memberMemberShipRepository = memberMemberShipRepository;
        }

        public async Task<ServiceResponse<string>> CreateMemberShipAsync(MemberShipCreateRequest request)
        {
            if (await _memberShipRepository.IsNameDuplicatedAsync(request.MemberShipName))
            {
                return ServiceResponse<string>.ErrorResponse("Tên gói thành viên đã tồn tại");
            }

            var memberShip = request.ToCreateMemberShip();
            await _memberShipRepository.CreateAsync(memberShip);

            return ServiceResponse<string>.SuccessResponse("Gói thành viên đã được tạo thành công");
        }

        public async Task<ServiceResponse<List<MemberShipDto>>> GetAllMemberShipsAsync()
        {
            var memberShips = await _memberShipRepository.GetAllActiveAsync();
            var result = memberShips.ToDtoList();
            return ServiceResponse<List<MemberShipDto>>.SuccessResponse(result);
        }

        public async Task<ServiceResponse<string>> UpdateMemberShipAsync(MemberShipUpdateRequest request)
        {
            var memberShip = await _memberShipRepository.GetByIdAsync(request.Id);
            if (memberShip == null || memberShip.Status == 0)
            {
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy gói thành viên");
            }

            if (!string.Equals(memberShip.MemberShipName, request.MemberShipName, StringComparison.OrdinalIgnoreCase))
            {
                if (await _memberShipRepository.IsNameDuplicatedAsync(request.MemberShipName))
                {
                    return ServiceResponse<string>.ErrorResponse("Tên gói thành viên đã tồn tại");
                }
            }

            request.MapToEntity(memberShip);
            await _memberShipRepository.UpdateAsync(memberShip);

            return ServiceResponse<string>.SuccessResponse("");
        }

        public async Task<ServiceResponse<string>> DeleteMemberShipAsync(string id)
        {
            var memberShip = await _memberShipRepository.GetByIdAsync(id);
            if (memberShip == null || memberShip.Status == 0)
            {
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy gói thành viên");
            }

            memberShip.Status = 0;
            await _memberShipRepository.UpdateAsync(memberShip);

            return ServiceResponse<string>.SuccessResponse("Đã xóa gói thành viên");
        }

        public async Task<ServiceResponse<MemberBuyMemberShipResponse>> BuyMemberShipAsync(string memberId, string accountId, MemberBuyMemberShipRequest request)
        {
            if (string.IsNullOrEmpty(accountId))
                return ServiceResponse<MemberBuyMemberShipResponse>.ErrorResponse("Không được phép truy cập");
            if (string.IsNullOrEmpty(memberId))
                return ServiceResponse<MemberBuyMemberShipResponse>.ErrorResponse("Không được phép truy cập");

            var member = await _memberRepository.GetByIdAsync(memberId);
            if (member == null)
                return ServiceResponse<MemberBuyMemberShipResponse>.ErrorResponse("Không tìm thấy người dùng");

            var membership = await _memberShipRepository.GetByIdAsync(request.MemberShipId);
            if (membership == null || membership.Status != 1)
                return ServiceResponse<MemberBuyMemberShipResponse>.ErrorResponse("Không tìm thấy gói thành viên hoặc gói đang không hoạt động");

            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null || string.IsNullOrEmpty(account.WalletId))
                return ServiceResponse<MemberBuyMemberShipResponse>.ErrorResponse("Không tìm thấy ví");

            var wallet = await _walletRepository.GetByIdAsync(account.WalletId);
            if (wallet == null || wallet.Status != 1)
                return ServiceResponse<MemberBuyMemberShipResponse>.ErrorResponse("Không tìm thấy ví");

            var price = membership.Price ?? 0;
            if ((wallet.Remaining ?? 0) < price)
                return ServiceResponse<MemberBuyMemberShipResponse>.ErrorResponse("Số dư không đủ");

            if (await _memberMemberShipRepository.MemberHasActiveMemberShipAsync(memberId, membership.Id))
            {
                return ServiceResponse<MemberBuyMemberShipResponse>.ErrorResponse("Thành viên đã sở hữu gói thành viên này và gói vẫn chưa hết hạn");
            }

            wallet.Remaining -= price;
            await _walletRepository.UpdateAsync(wallet);

            var now = Utils.Utils.GetTimeNow();
            var expiry = now.AddDays(membership.ExpiryDate ?? 30);
            var memberMemberShip = new MemberMemberShip
            {
                Id = Utils.Utils.GenerateIdModel("MemberMemberShip"),
                MemberId = member.Id,
                MemberShipId = membership.Id,
                Price = price,
                CreateDate = now,
                ExpiryDate = expiry,
                Status = 1
            };
            await _memberMemberShipRepository.CreateAsync(memberMemberShip);

            var sysTransaction = new SysTransaction
            {
                Id = Utils.Utils.GenerateIdModel("SysTransaction"),
                TransactionType = "5",
                DocNo = memberMemberShip.Id,
                CreateBy = accountId,
                CreateDate = now
            };
            await _sysTransactionRepository.CreateAsync(sysTransaction);

            var response = new MemberBuyMemberShipResponse
            {
                MemberShipId = membership.Id,
                MemberId = member.Id,
                Price = price,
                Remaining = wallet.Remaining,
                TransactionId = sysTransaction.Id,
                Message = "Buy membership successfully"
            };

            return ServiceResponse<MemberBuyMemberShipResponse>.SuccessResponse(response);
        }
        public async Task<ServiceResponse<List<MyMemberShipStatusResponse>>> GetMemberShipStatusAsync(string memberId)
        {
            var now = Utils.Utils.GetTimeNow();

            // Lấy tất cả membership hệ thống (để hiển thị đầy đủ)
            var memberships = await _memberShipRepository.GetAllActiveAsync();

            // Lấy danh sách MemberMemberShip đang hoạt động
            var activeMemberships = await _memberShipRepository.GetActiveMemberShipsByMemberIdAsync(memberId);

            // Map theo MemberShipId để tra nhanh
            var activeMembershipDict = activeMemberships
                .GroupBy(mms => mms.MemberShipId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.ExpiryDate).FirstOrDefault());

            var result = new List<MyMemberShipStatusResponse>();

            foreach (var memberShip in memberships)
            {
                activeMembershipDict.TryGetValue(memberShip.Id, out var activeMemberShip);

                var isActive = activeMemberShip != null && activeMemberShip.ExpiryDate > now;
                var expiredDate = activeMemberShip?.ExpiryDate;

                var response = new MyMemberShipStatusResponse
                {
                    MemberShip = new MemberShipDto
                    {
                        Id = memberShip.Id,
                        MemberShipName = memberShip.MemberShipName,
                        Rank = memberShip.Rank,
                        DiscountBooking = memberShip.DiscountBooking,
                        DiscountCourse = memberShip.DiscountCourse,
                        Price = memberShip.Price,
                        ExpiryDate = memberShip.ExpiryDate,
                        Status = memberShip.Status
                    },
                    ExpiredDate = expiredDate,
                    IsActive = isActive
                };

                result.Add(response);
            }

            return ServiceResponse<List<MyMemberShipStatusResponse>>.SuccessResponse(result);
        }

        public async Task<int> GetMaxBookingDiscountByMemberAsync(string memberId)
        {
            var now = Utils.Utils.GetTimeNow();

            var activeMemberships = await _memberMemberShipRepository
                .GetActiveMemberShipsByMemberIdAsync(memberId); 

            var validDiscounts = activeMemberships
                .Where(mms => mms.MemberShip != null && mms.MemberShip.Status == 1)
                .Select(mms => mms.MemberShip.DiscountBooking ?? 0)
                .ToList();

            return validDiscounts.Any() ? validDiscounts.Max() : 0;
        }

        public async Task<int> GetMaxCourseDiscountByMemberAsync(string memberId)
        {
            var activeMemberships = await _memberMemberShipRepository
                .GetActiveMemberShipsByMemberIdAsync(memberId);

            return activeMemberships
                .Where(mms => mms.MemberShip != null && mms.MemberShip.Status == 1)
                .Select(mms => mms.MemberShip.DiscountCourse ?? 0)
                .DefaultIfEmpty(0)
                .Max();
        }

    }
}
