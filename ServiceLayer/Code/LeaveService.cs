using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceLayer.Code
{
    public class LeaveService : ILeaveService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;
        private readonly IEmployeeService _employeeService;
        private readonly ICommonService _commonService;

        public LeaveService(IDb db, CurrentSession currentSession, IEmployeeService employeeService, ICommonService commonService)
        {
            _db = db;
            _currentSession = currentSession;
            _employeeService = employeeService;
            _commonService = commonService;
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
            }
            else
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
            BuildConfigurationDetailObject(leavePlanType);

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

        private void BuildConfigurationDetailObject(LeavePlanType leavePlanType)
        {
            LeavePlanConfiguration leavePlanConfiguration = new LeavePlanConfiguration();
            if (!_commonService.IsEmptyJson(leavePlanType.PlanConfigurationDetail))
                leavePlanConfiguration = JsonConvert.DeserializeObject<LeavePlanConfiguration>(leavePlanType.PlanConfigurationDetail);

            if (leavePlanConfiguration.leaveDetail == null)
                leavePlanConfiguration.leaveDetail = new LeaveDetail();

            if (leavePlanConfiguration.leaveAccrual == null)
                leavePlanConfiguration.leaveAccrual = new LeaveAccrual();

            if (leavePlanConfiguration.leaveApplyDetail == null)
                leavePlanConfiguration.leaveApplyDetail = new LeaveApplyDetail();

            if (leavePlanConfiguration.leaveEndYearProcessing == null)
                leavePlanConfiguration.leaveEndYearProcessing = new LeaveEndYearProcessing();

            if (leavePlanConfiguration.leaveHolidaysAndWeekoff == null)
                leavePlanConfiguration.leaveHolidaysAndWeekoff = new LeaveHolidaysAndWeekoff();

            if (leavePlanConfiguration.leavePlanRestriction == null)
                leavePlanConfiguration.leavePlanRestriction = new LeavePlanRestriction();

            if (leavePlanConfiguration.leaveApproval == null)
                leavePlanConfiguration.leaveApproval = new LeaveApproval();

            leavePlanType.PlanConfigurationDetail = JsonConvert.SerializeObject(leavePlanConfiguration);
        }

        public List<LeavePlanType> UpdateLeavePlanTypeService(int leavePlanTypeId, LeavePlanType leavePlanType)
        {
            if (leavePlanType.LeavePlanTypeId <= 0)
                throw new HiringBellException("Leave plan type id not found. Please add one plan first.");

            ValidateLeavePlanToInsert(leavePlanType);

            LeavePlanType record = _db.Get<LeavePlanType>("sp_leave_plans_type_getbyId", new { LeavePlanTypeId = leavePlanTypeId });

            if (record == null || record.LeavePlanTypeId != leavePlanTypeId)
                throw new HiringBellException("Trying to udpate invalid leave plan type");

            record.IsPaidLeave = leavePlanType.IsPaidLeave;
            record.AvailableLeave = leavePlanType.AvailableLeave;
            record.IsSickLeave = leavePlanType.IsSickLeave;
            record.MaxLeaveLimit = leavePlanType.MaxLeaveLimit;
            record.LeavePlanCode = leavePlanType.LeavePlanCode;
            record.AdminId = _currentSession.CurrentUserDetail.UserId;
            record.IsMale = leavePlanType.IsMale;
            record.IsMarried = leavePlanType.IsMarried;
            record.IsRestrictOnGender = leavePlanType.IsRestrictOnGender;
            record.IsRestrictOnMaritalStatus = leavePlanType.IsRestrictOnMaritalStatus;
            record.IsStatutoryLeave = leavePlanType.IsStatutoryLeave;
            record.PlanDescription = leavePlanType.PlanDescription;

            return this.AddLeavePlanTypeService(record);
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

        public string ApprovalOrRejectActionService(ApprovalRequest approvalRequest, ItemStatus status)
        {
            string message = string.Empty;
            DbParam[] param = new DbParam[]
            {
                new DbParam(approvalRequest.ApprovalRequestId, typeof(long), "_ApprovalRequestId"),
                new DbParam(approvalRequest.LeaveRequestId, typeof(long), "_LeaveRequestId"),
                new DbParam(1, typeof(int), "_RequestType")
            };

            var result = _db.GetDataset("sp_approval_request_GetById", param);
            if (result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
            {
                var leaveDetailString = result.Tables[0].Rows[0]["LeaveDetail"].ToString();
                List<CompleteLeaveDetail> completeLeaveDetail = JsonConvert.DeserializeObject<List<CompleteLeaveDetail>>(leaveDetailString);
                ApprovalRequest existingRecord = Converter.ToType<ApprovalRequest>(result.Tables[0]);

                if (completeLeaveDetail != null)
                {
                    var singleLeaveDetail = completeLeaveDetail.Find(x =>
                        approvalRequest.FromDate.Subtract(x.LeaveFromDay).TotalDays == 0 &&
                        approvalRequest.ToDate.Subtract(x.LeaveToDay).TotalDays == 0
                    );

                    if (singleLeaveDetail != null)
                    {
                        singleLeaveDetail.LeaveStatus = (int)status;
                        singleLeaveDetail.RespondedBy = _currentSession.CurrentUserDetail.UserId;
                        leaveDetailString = JsonConvert.SerializeObject((from n in completeLeaveDetail
                                                                         select new
                                                                         {
                                                                             Reason = n.Reason,
                                                                             Session = n.Session,
                                                                             AssignTo = n.AssignTo,
                                                                             LeaveType = n.LeaveType,
                                                                             NumOfDays = n.NumOfDays,
                                                                             ProjectId = n.ProjectId,
                                                                             UpdatedOn = n.UpdatedOn,
                                                                             EmployeeId = n.EmployeeId,
                                                                             LeaveToDay = n.LeaveToDay,
                                                                             LeaveStatus = n.LeaveStatus,
                                                                             RequestedOn = n.RequestedOn,
                                                                             RespondedBy = n.RespondedBy,
                                                                             EmployeeName = n.EmployeeName,
                                                                             LeaveFromDay = n.LeaveFromDay
                                                                         }));
                    }
                    else
                    {
                        throw new HiringBellException("Error");
                    }
                }
                else
                {
                    throw new HiringBellException("Error");
                }

                if (existingRecord != null)
                {
                    existingRecord.RequestStatusId = approvalRequest.RequestStatusId;

                    param = new DbParam[]
                    {
                        new DbParam(existingRecord.ApprovalRequestId, typeof(long), "_ApprovalRequestId"),
                        new DbParam(existingRecord.Message, typeof(string), "_Message"),
                        new DbParam(existingRecord.UserName, typeof(string), "_UserName"),
                        new DbParam(existingRecord.UserId, typeof(long), "_UserId"),
                        new DbParam(existingRecord.UserTypeId, typeof(int), "_UserTypeId"),
                        new DbParam(DateTime.Now, typeof(DateTime), "_RequestedOn"),
                        new DbParam(existingRecord.Email, typeof(string), "_Email"),
                        new DbParam(existingRecord.Mobile, typeof(string), "_Mobile"),
                        new DbParam(existingRecord.FromDate, typeof(DateTime), "_FromDate"),
                        new DbParam(existingRecord.ToDate, typeof(DateTime), "_ToDate"),
                        new DbParam(existingRecord.AssigneeId, typeof(long), "_AssigneeId"),
                        new DbParam(existingRecord.ProjectId, typeof(long), "_ProjectId"),
                        new DbParam(existingRecord.ProjectName, typeof(string), "_ProjectName"),
                        new DbParam(existingRecord.RequestStatusId, typeof(int), "_RequestStatusId"),
                        new DbParam(existingRecord.LeaveRequestId, typeof(long), "_LeaveRequestId"),
                        new DbParam(leaveDetailString, typeof(string), "_LeaveDetail"),
                        new DbParam(existingRecord.LeaveType, typeof(int), "_LeaveType"),
                        new DbParam(existingRecord.AttendanceId, typeof(long), "_AttendanceId"),
                        new DbParam(existingRecord.RequestType, typeof(int), "_RequestType")
                    };

                    message = _db.ExecuteNonQuery("sp_approval_request_leave_InsUpdate", param, true);
                }
            }
            return message;
        }
    }
}
