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
        private readonly IEmployeeService _employeeService;

        public LeaveService(IDb db, CurrentSession currentSession, IEmployeeService employeeService)
        {
            _db = db;
            _currentSession = currentSession;
            _employeeService = employeeService;
        }

        public List<LeavePlan> AddLeavePlansService(LeavePlan leavePlan)
        {
            List<LeavePlan> leavePlans = null;
            if (leavePlan.LeavePlanId > 0)
            {
                leavePlans = _db.GetList<LeavePlan>("sp_leave_plans_get");
                if (leavePlans.Count <= 0)
                    throw new HiringBellException("Invalid leave plan.");

                var result = leavePlans.Find(x => x.LeavePlanId == leavePlan.LeavePlanId);
                if (result == null)
                    throw new HiringBellException("Invalid leave plan.");

                result.PlanName = leavePlan.PlanName;
                result.PlanDescription = leavePlan.PlanDescription;
                result.IsShowLeavePolicy = leavePlan.IsShowLeavePolicy;
                result.IsUploadedCustomLeavePolicy = leavePlan.IsUploadedCustomLeavePolicy;
                result.PlanStartCalendarDate = leavePlan.PlanStartCalendarDate;
                leavePlan = result;
            } else
            {
                leavePlan.AssociatedPlanTypes = "[]";
            }
            var value = _db.Execute<LeavePlan>("sp_leave_plan_insupd", leavePlan, true);
            if (string.IsNullOrEmpty(value))
                throw new HiringBellException("Unable to add or update leave plan");

            leavePlans = _db.GetList<LeavePlan>("sp_leave_plans_get");
            return leavePlans;
        }

        public List<LeavePlanType> AddLeavePlanTypeService(LeavePlanType leavePlanType)
        {
            List<LeavePlanType> leavePlanTypes = default(List<LeavePlanType>);
            ValidateLeavePlanToInsert(leavePlanType);

            leavePlanType.PlanConfigurationDetail = "{}";
            string result = _db.Execute<LeavePlanType>("sp_leave_plans_type_insupd", new
            {
                leavePlanType.IsPaidLeave,
                leavePlanType.MaxLeaveLimit,
                leavePlanType.IsSickLeave,
                leavePlanType.IsStatutoryLeave,
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

            if (ApplicationConstants.IsExecuted(result))
            {
                leavePlanTypes = _db.GetList<LeavePlanType>("sp_leave_plans_type_get");
            }

            return leavePlanTypes;
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

        public dynamic GetLeavePlansService(FilterModel filterModel)
        {
            List<LeavePlan> leavePlans = _db.GetList<LeavePlan>("sp_leave_plans_get");
            List<Employee> employees = _employeeService.GetEmployees(filterModel);
            return new { LeavePlan = leavePlans, Employees = employees };
        }

        public List<LeavePlanType> UpdateLeavePlanTypeService(int leavePlanTypeId, LeavePlanType leavePlanType)
        {
            if (leavePlanType.LeavePlanTypeId <= 0)
                throw new HiringBellException("Leave plan type id not found. Please add one plan first.");

            ValidateLeavePlanToInsert(leavePlanType);
            LeavePlanType record = _db.Get<LeavePlanType>("sp_leave_plans_type_getbyId", new { LeavePlanTypeId = leavePlanTypeId });

            if (record == null || record.LeavePlanTypeId != leavePlanTypeId)
                throw new HiringBellException("Trying to udpate invalid leave plan type");

            return this.AddLeavePlanTypeService(leavePlanType);
        }

        public string AddUpdateLeaveQuotaService(LeaveDetail leaveDetail)
        {
            string result = _db.Execute<LeaveDetail>("sp_leave_detail_InsUpdate", leaveDetail, true);
            return result;
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

        public List<LeavePlanType> GetLeaveTypeFilterService()
        {
            List<LeavePlanType> leavePlanTypes = _db.GetList<LeavePlanType>("sp_leave_plans_type_get");
            return leavePlanTypes;
        }

        public LeavePlan LeavePlanUpdateTypes(int leavePlanId, List<LeavePlanType> leavePlanTypes)
        {
            if (leavePlanId <= 0)
                throw new HiringBellException("Invalid leave plan id.");

            LeavePlan leavePlan = _db.Get<LeavePlan>("sp_leave_plans_getbyId", new { LeavePlanId = leavePlanId });
            if (leavePlan == null)
                throw new HiringBellException("Invalid leave plan selected.");

            leavePlanTypes.ForEach(item => item.PlanConfigurationDetail = "");
            leavePlan.AssociatedPlanTypes = JsonConvert.SerializeObject(leavePlanTypes);

            var result = _db.Execute<LeavePlan>("sp_leave_plan_insupd", leavePlan, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Unable to add leave type.");
            return leavePlan;
        }

        public List<LeavePlan> SetDefaultPlanService(int LeavePlanId, LeavePlan leavePlan)
        {
            List<LeavePlan> leavePlans = null;
            if (leavePlan.LeavePlanId <= 0)
                throw new HiringBellException("Invalid leave plan selected.");
                
            var value = _db.Execute<LeavePlan>("sp_leave_plan_set_default", new
            {
                leavePlan.LeavePlanId,
                leavePlan.IsDefaultPlan
            }, true);
            if (string.IsNullOrEmpty(value))
                throw new HiringBellException("Unable to add or update leave plan");

            leavePlans = _db.GetList<LeavePlan>("sp_leave_plans_get");
            return leavePlans;
        }
    }
}
