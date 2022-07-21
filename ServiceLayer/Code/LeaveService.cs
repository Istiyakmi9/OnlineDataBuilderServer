using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System.Collections.Generic;

namespace ServiceLayer.Code
{
    public class LeaveService : ILeaveService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;

        public LeaveService(IDb db, CurrentSession currentSession)
        {
            _db = db;
            _currentSession = currentSession;
        }

        public string AddLeavePlansService(LeavePlan leavePlan)
        {
            ValidateLeavePlanToInsert(leavePlan);
            string result = _db.Execute<LeavePlan>("sp_leave_plans_insupd", new
            {
                leavePlan.IsPaidLeave,
                leavePlan.MaxLeaveLimit,
                leavePlan.IsSickLeave,
                leavePlan.IsStatutoryLeave,
                leavePlan.LeavePlanId,
                leavePlan.ShowDescription,
                leavePlan.LeavePlanCode,
                leavePlan.PlanName,
                leavePlan.PlanDescription,
                leavePlan.LeaveGroupId,
                leavePlan.IsMale,
                leavePlan.IsMarried,
                leavePlan.IsRestrictOnGender,
                leavePlan.IsRestrictOnMaritalStatus,
                Reasons = leavePlan.Reasons,
                AdminId = _currentSession.CurrentUserDetail.UserId
            }, true);

            return result;
        }

        private void ValidateLeavePlanToInsert(LeavePlan leavePlan)
        {
            if (leavePlan == null)
                throw new HiringBellException("Empty Leave plan submitted.");

            int multiPlanFlag = 3;
            if (leavePlan.IsPaidLeave)
                multiPlanFlag--;

            if (leavePlan.IsSickLeave)
                multiPlanFlag--;

            if (leavePlan.IsStatutoryLeave)
                multiPlanFlag--;

            if (multiPlanFlag != 2)
            {
                if (leavePlan.IsSickLeave)
                    throw new HiringBellException("Multiple leave type selected. (i.e. Select only one from Sick or Paid or Statutory)");

                if (leavePlan.IsStatutoryLeave)
                    throw new HiringBellException("Multiple leave type selected. (i.e. Select only one from Sick or Paid or Statutory)");
            }
        }

        public List<LeavePlan> GetLeavePlansService()
        {
            List<LeavePlan> LeavePlans = _db.GetList<LeavePlan>("sp_leave_plans_get");
            return LeavePlans;
        }

        public string UpdateLeavePlansService(int leavePlanId, LeavePlan leavePlan)
        {
            ValidateLeavePlanToInsert(leavePlan);
            LeavePlan record = _db.Get<LeavePlan>("sp_leave_plans_getbyId", new { LeavePlanId = leavePlanId });

            if (record == null || record.LeavePlanId != leavePlanId)
                throw new HiringBellException("Trying to udpate invalid leave plan");

            leavePlan.LeaveGroupId = record.LeaveGroupId;

            return this.AddLeavePlansService(leavePlan);
        }
    }
}
