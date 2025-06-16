using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.Mappers;
using PPC.Service.ModelRequest.MemberShipRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.MemberShipResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class MemberShipService : IMemberShipService
    {
        private readonly IMemberShipRepository _memberShipRepository;

        public MemberShipService(IMemberShipRepository memberShipRepository)
        {
            _memberShipRepository = memberShipRepository;
        }

        public async Task<ServiceResponse<string>> CreateMemberShipAsync(MemberShipCreateRequest request)
        {
            if (await _memberShipRepository.IsNameDuplicatedAsync(request.MemberShipName))
            {
                return ServiceResponse<string>.ErrorResponse("Membership name already exists.");
            }

            var memberShip = request.ToCreateMemberShip();
            await _memberShipRepository.CreateAsync(memberShip);

            return ServiceResponse<string>.SuccessResponse("Membership created successfully.");
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
                return ServiceResponse<string>.ErrorResponse("Membership not found.");
            }

            if (!string.Equals(memberShip.MemberShipName, request.MemberShipName, StringComparison.OrdinalIgnoreCase))
            {
                if (await _memberShipRepository.IsNameDuplicatedAsync(request.MemberShipName))
                {
                    return ServiceResponse<string>.ErrorResponse("Membership name already exists.");
                }
            }

            request.MapToEntity(memberShip);
            await _memberShipRepository.UpdateAsync(memberShip);

            return ServiceResponse<string>.SuccessResponse("Membership updated successfully.");
        }

        public async Task<ServiceResponse<string>> DeleteMemberShipAsync(string id)
        {
            var memberShip = await _memberShipRepository.GetByIdAsync(id);
            if (memberShip == null || memberShip.Status == 0)
            {
                return ServiceResponse<string>.ErrorResponse("Membership not found.");
            }

            memberShip.Status = 0;
            await _memberShipRepository.UpdateAsync(memberShip);

            return ServiceResponse<string>.SuccessResponse("Membership deleted (status set to 0).");
        }
    }
}
