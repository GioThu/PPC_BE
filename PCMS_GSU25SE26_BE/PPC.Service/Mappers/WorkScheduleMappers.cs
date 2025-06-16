using PPC.DAO.Models;
using PPC.Service.ModelRequest.WorkScheduleRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Mappers
{
    public static class WorkScheduleMappers
    {
        public static WorkSchedule ToCreateWorkSchedule(this WorkScheduleCreateRequest workScheduleCreateRequest)
        {
            return new WorkSchedule
            {
                Id = Utils.Utils.GenerateIdModel("WorkSchedule"),
                WorkDate = workScheduleCreateRequest.WorkDate,
                StartTime = workScheduleCreateRequest.StartTime,
                EndTime = workScheduleCreateRequest.EndTime,
                Description = workScheduleCreateRequest.Description,
                CreateAt = Utils.Utils.GetTimeNow(),
                Status = 1,
            };
        }
    }
}
