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

        public List<LeavePlan> AddLeavePlansService(LeavePlan leavePlan)
        {
            string result = _db.Execute<LeavePlan>("sp_leave_plan_insupd", leavePlan, true);
            List<LeavePlan> leavePlans = _db.GetList<LeavePlan>("sp_leave_plans_get");
            return leavePlans;
        }

        public string AddLeavePlanTypeService(LeavePlanType leavePlanType)
        {
            if (leavePlanType.LeavePlanId <= 0)
                throw new HiringBellException("Leave plan id not found. Please add one plan first.");

            ValidateLeavePlanToInsert(leavePlanType);

            leavePlanType.PlanConfigurationDetail = "{}";
            string result = _db.Execute<LeavePlanType>("sp_leave_plans_type_insupd", new
            {
                leavePlanType.IsPaidLeave,
                leavePlanType.MaxLeaveLimit,
                leavePlanType.IsSickLeave,
                leavePlanType.IsStatutoryLeave,
                leavePlanType.LeavePlanId,
                leavePlanType.LeavePlanTypeId,
                leavePlanType.ShowDescription,
                leavePlanType.LeavePlanCode,
                leavePlanType.PlanName,
                leavePlanType.PlanDescription,
                leavePlanType.IsMale,
                leavePlanType.IsMarried,
                leavePlanType.IsRestrictOnGender,
                leavePlanType.IsRestrictOnMaritalStatus,
                Reasons = leavePlanType.Reasons,
                PlanConfigurationDetail = leavePlanType.PlanConfigurationDetail,
                AdminId = _currentSession.CurrentUserDetail.UserId
            }, true);

            return result;
        }

        private void ValidateLeavePlanToInsert(LeavePlanType leavePlanType)
        {
            if (leavePlanType == null)
                throw new HiringBellException("Empty Leave plan submitted.");

            int multiPlanFlag = 3;
            if (leavePlanType.IsPaidLeave)
                multiPlanFlag--;

            if (leavePlanType.IsSickLeave)
                multiPlanFlag--;

            if (leavePlanType.IsStatutoryLeave)
                multiPlanFlag--;

            if (multiPlanFlag != 2)
            {
                if (leavePlanType.IsSickLeave)
                    throw new HiringBellException("Multiple leave type selected. (i.e. Select only one from Sick or Paid or Statutory)");

                if (leavePlanType.IsStatutoryLeave)
                    throw new HiringBellException("Multiple leave type selected. (i.e. Select only one from Sick or Paid or Statutory)");
            }
        }

        public List<LeavePlanType> GetLeavePlansService()
        {
            List<LeavePlanType> leavePlanType = _db.GetList<LeavePlanType>("sp_leave_plans_get");
            return leavePlanType;
        }

        public string UpdateLeavePlanTypeService(int leavePlanTypeId, LeavePlanType leavePlanType)
        {
            if (leavePlanType.LeavePlanId <= 0)
                throw new HiringBellException("Leave plan id not found. Please add one plan first.");

            ValidateLeavePlanToInsert(leavePlanType);
            LeavePlanType record = _db.Get<LeavePlanType>("sp_leave_plans_type_getbyId", new { LeavePlanTypeId = leavePlanTypeId });

            if (record == null || record.LeavePlanId != leavePlanTypeId)
                throw new HiringBellException("Trying to udpate invalid leave plan");

            leavePlanType.LeavePlanId = record.LeavePlanId;

            return this.AddLeavePlanTypeService(leavePlanType);
        }

        public string AddUpdateLeaveQuotaService(LeaveDetail leaveDetail)
        {
            string result = _db.Execute<LeaveDetail>("sp_leave_detail_InsUpdate", leaveDetail, true);
            return result;
        }

        public List<LeavePlanType> GetLeaveTypeDetailByPlan(int leavePlanId)
        {
            List<LeavePlanType> leavePlanTypes = _db.GetList<LeavePlanType>("sp_leave_plans_type_get_planid", new { LeavePlanId = leavePlanId });
            if (leavePlanTypes == null)
                throw new HiringBellException("Invalid plan id supplied");

            return leavePlanTypes;
        }

        public LeavePlanConfiguration GetLeaveTypeDetailByIdService(int leavePlanTypeId)
        {
            LeavePlanType leavePlanType = _db.Get<LeavePlanType>("sp_leave_plans_type_getbyId", new { LeavePlanTypeId = leavePlanTypeId });
            if (leavePlanType == null)
                throw new HiringBellException("Invalid plan id supplied");

            LeavePlanConfiguration leavePlanConfiguration = JsonConvert.DeserializeObject<LeavePlanConfiguration>(leavePlanType.PlanConfigurationDetail);
            if (leavePlanConfiguration == null)
                leavePlanConfiguration = new LeavePlanConfiguration();

            return leavePlanConfiguration;
        }
    }
}
