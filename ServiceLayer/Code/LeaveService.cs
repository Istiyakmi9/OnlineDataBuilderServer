using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using Newtonsoft.Json;
using ServiceLayer.Caching;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLayer.Code
{
    public class LeaveService : ILeaveService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;
        private readonly ICommonService _commonService;
        private readonly ICacheManager _cacheManager;
        private readonly ILeaveCalculation _leaveCalculation;

        public LeaveService(IDb db,
            CurrentSession currentSession,
            ICommonService commonService,
            ICacheManager cacheManager,
            ILeaveCalculation leaveCalculation)
        {
            _db = db;
            _currentSession = currentSession;
            _commonService = commonService;
            _cacheManager = cacheManager;
            _leaveCalculation = leaveCalculation;
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
                result.IsDefaultPlan = leavePlan.IsDefaultPlan;
                leavePlan = result;
            }
            else
            {
                leavePlan.CompanyId = _currentSession.CurrentUserDetail.CompanyId;
                leavePlan.AssociatedPlanTypes = "[]";
            }
            var value = _db.Execute<LeavePlan>("sp_leave_plan_insupd", leavePlan, true);
            if (string.IsNullOrEmpty(value))
                throw new HiringBellException("Unable to add or update leave plan");

            leavePlans = _db.GetList<LeavePlan>("sp_leave_plans_get");
            _cacheManager.ReLoad(CacheTable.Company, Converter.ToDataTable<LeavePlan>(leavePlans));
            return leavePlans;
        }

        public List<LeavePlanType> AddLeavePlanTypeService(LeavePlanType leavePlanType)
        {
            List<LeavePlanType> leavePlanTypes = default(List<LeavePlanType>);
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

        public List<LeavePlan> GetLeavePlansService(FilterModel filterModel)
        {
            List<LeavePlan> leavePlans = _db.GetList<LeavePlan>("sp_leave_plans_get");
            if (leavePlans == null)
                throw new HiringBellException("Leave plans not found.");

            leavePlans.ForEach(item =>
            {
                if (!string.IsNullOrEmpty(item.AssociatedPlanTypes))
                {
                    var planTypes = JsonConvert.DeserializeObject<List<LeavePlanType>>(item.AssociatedPlanTypes);
                    if (planTypes != null)
                    {
                        Parallel.ForEach(planTypes, type =>
                        {
                            type.LeavePlanId = item.LeavePlanId;
                        });
                    }

                    item.AssociatedPlanTypes = JsonConvert.SerializeObject(planTypes);
                }
            });

            return leavePlans;
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

        public async Task<LeavePlan> LeavePlanUpdateTypes(int leavePlanId, List<LeavePlanType> leavePlanTypes)
        {
            if (leavePlanId <= 0)
                throw new HiringBellException("Invalid leave plan id.");

            LeavePlan leavePlan = _db.Get<LeavePlan>("sp_leave_plans_getbyId", new { LeavePlanId = leavePlanId });
            if (leavePlan == null)
                throw new HiringBellException("Invalid leave plan selected.");

            leavePlanTypes.ForEach(item => item.PlanConfigurationDetail = "");
            leavePlan.AssociatedPlanTypes = JsonConvert.SerializeObject(leavePlanTypes);

            var result = await _db.ExecuteAsync("sp_leave_plan_insupd", leavePlan, true);
            if (result.rowsEffected != 1 || string.IsNullOrEmpty(result.statusMessage))
                throw new HiringBellException("Unable to add leave type. Please contact to admin.");
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

        public string LeaveRquestManagerActionService(LeaveRequestNotification notification, ItemStatus status)
        {
            string message = string.Empty;
            var requestNotification = _db.Get<LeaveRequestNotification>("sp_leave_request_notification_get_byId", new
            {
                notification.LeaveRequestNotificationId
            });

            if (requestNotification != null)
            {
                List<CompleteLeaveDetail> completeLeaveDetail = JsonConvert
                  .DeserializeObject<List<CompleteLeaveDetail>>(requestNotification.LeaveDetail);

                if (completeLeaveDetail != null)
                {
                    var singleLeaveDetail = completeLeaveDetail.Find(x =>
                        requestNotification.FromDate.Subtract(x.LeaveFromDay).TotalDays == 0 &&
                        requestNotification.ToDate.Subtract(x.LeaveToDay).TotalDays == 0
                    );

                    if (singleLeaveDetail != null)
                    {
                        singleLeaveDetail.LeaveStatus = (int)status;
                        singleLeaveDetail.RespondedBy = _currentSession.CurrentUserDetail.UserId;
                        requestNotification.LeaveDetail = JsonConvert.SerializeObject(
                            (from n in completeLeaveDetail
                             select new
                             {
                                 Reason = n.Reason,
                                 Session = n.Session,
                                 AssignTo = n.AssignTo,
                                 LeaveType = n.LeaveTypeId,
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
                             })
                            );
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

                if (requestNotification != null)
                {
                    requestNotification.LastReactedOn = DateTime.UtcNow;
                    requestNotification.RequestStatusId = notification.RequestStatusId;
                    message = _db.Execute<LeaveRequestNotification>("sp_leave_request_notification_InsUpdate", new
                    {
                        requestNotification.LeaveRequestNotificationId,
                        requestNotification.LeaveRequestId,
                        requestNotification.UserMessage,
                        requestNotification.EmployeeId,
                        requestNotification.AssigneeId,
                        requestNotification.ProjectId,
                        requestNotification.ProjectName,
                        requestNotification.FromDate,
                        requestNotification.ToDate,
                        requestNotification.NumOfDays,
                        requestNotification.RequestStatusId,
                        requestNotification.LeaveTypeId,
                        requestNotification.FeedBackMessage,
                        requestNotification.LastReactedOn,
                        requestNotification.LeaveDetail
                    }, true);
                }
            }
            return message;
        }

        private void UpdateLeavePlanDetail(LeaveCalculationModal leaveCalculationModal)
        {
            var leaves = JsonConvert.DeserializeObject<List<CompleteLeaveDetail>>(
                leaveCalculationModal.leaveRequestDetail.LeaveDetail);
            if (leaves != null)
            {
                Parallel.ForEach(leaveCalculationModal.leavePlanTypes, i =>
                {
                    var consumed = leaves
                    .Where(x => x.LeaveTypeId == i.LeavePlanTypeId && x.LeaveStatus != (int)ItemStatus.Rejected)
                    .Sum(x => x.NumOfDays);

                    i.ConsumedLeave = consumed;
                });
            }
        }

        private void ValidateRequestModal(LeaveRequestModal leaveRequestModal)
        {
            if (leaveRequestModal == null)
                throw new HiringBellException("Invalid request detail sumitted.");

            if (leaveRequestModal.EmployeeId <= 0)
                throw new HiringBellException("Invalid Employee Id submitted.");

            if (leaveRequestModal.LeaveFromDay == null || leaveRequestModal.LeaveToDay == null)
                throw new HiringBellException("Invalid From and To date passed.");

        }

        public async Task<dynamic> ApplyLeaveService(LeaveRequestModal leaveRequestModal)
        {
            this.ValidateRequestModal(leaveRequestModal);
            var leaveCalculationModal = await _leaveCalculation.CheckAndApplyForLeave(leaveRequestModal);

            if (!string.IsNullOrEmpty(leaveCalculationModal.leaveRequestDetail.LeaveDetail))
                this.UpdateLeavePlanDetail(leaveCalculationModal);
            return new
            {
                LeavePlanTypes = leaveCalculationModal.leavePlanTypes,
                EmployeeLeaveDetail = leaveCalculationModal.leaveRequestDetail,
                Employee = leaveCalculationModal.employee
            };
        }

        private async Task<LeaveCalculationModal> GetCalculatedLeaveDetail(LeaveCalculationModal leaveCalculationModal)
        {
            var requestDetail = _db.Get<LeaveRequestDetail>("sp_employee_leave_request_filter", new
            {
                EmployeeId = leaveCalculationModal.employee.EmployeeId,
                SearchString = " 1=1 ",
                SortBy = string.Empty,
                PageIndex = 1,
                PageSize = 1
            });

            decimal totalLeaveLimit = 0;
            decimal totalAvailableLeave = 0;
            leaveCalculationModal.leavePlanTypes.ForEach(x =>
            {
                totalAvailableLeave += x.AvailableLeave;
                totalLeaveLimit += x.MaxLeaveLimit;
            });

            var leaveDetails = JsonConvert.DeserializeObject<List<CompleteLeaveDetail>>(leaveCalculationModal.leaveRequestDetail.LeaveDetail);
            if (leaveDetails == null)
                throw new HiringBellException("Unable to get leave detail. Please contact to admin.");

            //leaveDetails.Where(x => x.LeaveTypeId == )

            //var status = _db.Execute<string>("sp_employee_leave_request_InsUpdate", new
            //{
            //    LeaveRequestId = 0,
            //    EmployeeId = leaveCalculationModal.employee.EmployeeId,
            //    LeaveDetail = JsonConvert.SerializeObject(leaveCalculationModal.leaveRequestDetail.LeaveDetail),
            //    Year = DateTime.UtcNow.Year,
            //    AvailableLeaves = totalAvailableLeave,
            //    TotalLeaveApplied = 0,
            //    TotalApprovedLeave = 0,
            //    TotalLeaveQuota = totalLeaveLimit,
            //    LeaveQuotaDetail = JsonConvert.SerializeObject(leaveCalculationModal),
            //}, true);

            //if (ApplicationConstants.IsExecuted(status))
            //    throw new HiringBellException("Unable to update leave detail.");

            return leaveCalculationModal;
        }

        private async Task<LeaveCalculationModal> GetLatestLeaveDetail(long employeeId)
        {
            if (employeeId < 0)
                throw new HiringBellException("Invalid employee id.");

            LeaveCalculationModal leaveCalculationModal = default(LeaveCalculationModal);
            leaveCalculationModal = await _leaveCalculation.GetBalancedLeave(employeeId, DateTime.Now, DateTime.Now);
            if (leaveCalculationModal == null)
                throw new HiringBellException("Unable to calculate leave balance detail. Please contact to admin.");

            return leaveCalculationModal;
        }

        public async Task<dynamic> GetEmployeeLeaveDetail(LeaveRequestModal leaveRequestModal)
        {
            this.ValidateRequestModal(leaveRequestModal);
            var leaveCalculationModal = await GetLatestLeaveDetail(leaveRequestModal.EmployeeId);

            if (!string.IsNullOrEmpty(leaveCalculationModal.leaveRequestDetail.LeaveDetail))
                this.UpdateLeavePlanDetail(leaveCalculationModal);
            return new
            {
                LeavePlanTypes = leaveCalculationModal.leavePlanTypes,
                EmployeeLeaveDetail = leaveCalculationModal.leaveRequestDetail,
                Employee = leaveCalculationModal.employee
            };
        }

        public async Task<List<LeavePlanType>> ApplyLeaveService_Testing(ApplyLeave applyLeave)
        {
            if (applyLeave.EmployeeId < 0)
                throw new HiringBellException("Invalid employee id.");

            List<LeavePlanType> leavePlanTypes = null;
            //leavePlanTypes = await _leaveCalculation.GetBalancedLeave(applyLeave.EmployeeId, Convert.ToDateTime("2022-10-10"), Convert.ToDateTime("2022-10-15"));
            LeaveRequestModal leaveDetail = new LeaveRequestModal
            {
                EmployeeId = 4,
                LeaveTypeId = 1,
                LeaveFromDay = Convert.ToDateTime("2022-10-22"),
                LeaveToDay = Convert.ToDateTime("2022-10-22")
            };

            await _leaveCalculation.CheckAndApplyForLeave(leaveDetail);

            return leavePlanTypes;
        }
    }
}
