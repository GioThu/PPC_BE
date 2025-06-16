using PPC.DAO.Models;
using PPC.Service.ModelRequest.MemberShipRequest;
using PPC.Service.ModelResponse.MemberShipResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Mappers
{
    public static class MemberShipMappers
    {
        public static MemberShip ToCreateMemberShip(this MemberShipCreateRequest request)
        {
            return new MemberShip
            {
                Id = Utils.Utils.GenerateIdModel("MemberShip"),
                MemberShipName = request.MemberShipName,
                Rank = request.Rank,
                DiscountCourse = request.DiscountCourse,
                DiscountBooking = request.DiscountBooking,
                Price = request.Price,
                ExpiryDate = request.ExpiryDate,
                Status = 1
            };
        }

        public static MemberShipDto ToDto(this MemberShip entity)
        {
            return new MemberShipDto
            {
                Id = entity.Id,
                MemberShipName = entity.MemberShipName,
                Rank = entity.Rank,
                DiscountCourse = entity.DiscountCourse,
                DiscountBooking = entity.DiscountBooking,
                Price = entity.Price,
                ExpiryDate = entity.ExpiryDate,
                Status = entity.Status
            };
        }

        public static List<MemberShipDto> ToDtoList(this List<MemberShip> entities)
        {
            return entities.Select(e => e.ToDto()).ToList();
        }

        public static void MapToEntity(this MemberShipUpdateRequest request, MemberShip entity)
        {
            entity.MemberShipName = request.MemberShipName;
            entity.Rank = request.Rank;
            entity.DiscountCourse = request.DiscountCourse;
            entity.DiscountBooking = request.DiscountBooking;
            entity.Price = request.Price;
            entity.ExpiryDate = request.ExpiryDate;
        }
    }
}
