using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using BottomhalfCore.Services.Interface;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using ModalLayer.Modal.Leaves;
using Newtonsoft.Json;
using ServiceLayer.Code.Leaves;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceLayer.Code
{
    public class LeaveCalculation : ILeaveCalculation
    {
        private readonly IDb _db;
        private LeavePlanConfiguration _leavePlanConfiguration;
        private readonly DateTime now = DateTime.UtcNow;

        private readonly ITimezoneConverter _timezoneConverter;
        private readonly CurrentSession _currentSession;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        private readonly Quota _quota;
        private readonly LeaveFromManagement _leaveFromManagement;
        private readonly Accrual _accrual;
        private readonly Apply _apply;
        private readonly Restriction _restriction;
        private readonly HolidaysAndWeekoffs _holidaysAndWeekoffs;
        private readonly Approval _approval;
        private readonly YearEndCalculation _yearEndCalculation;

        public LeaveCalculation(IDb db,
            ITimezoneConverter timezoneConverter,
            CurrentSession currentSession,
            Quota quota,
            LeaveFromManagement leaveFromManagement,
            Accrual accrual,
            Apply apply,
            Restriction restriction)
        {
            _db = db;
            _timezoneConverter = timezoneConverter;
            _currentSession = currentSession;
            _quota = quota;
            _leaveFromManagement = leaveFromManagement;
            _accrual = accrual;
            _apply = apply;
            _restriction = restriction;
        }

        public async Task<LeaveCalculationModal> GetBalancedLeave(long EmployeeId, DateTime FromDate, DateTime ToDate)
        {
            var leaveCalculationModal = await GetCalculationModal(EmployeeId, FromDate, ToDate);

            int i = 0;
            while (i < leaveCalculationModal.leavePlanTypes.Count)
            {
                await ProcessLeaveSections(leaveCalculationModal, leaveCalculationModal.leavePlanTypes[i]);
                i++;
            }

            return leaveCalculationModal;
        }

        public async Task<LeaveCalculationModal> GetRequestTypeBalancedLeave(LeaveRequestModal leaveRequestModal)
        {
            var leaveCalculationModal = await GetCalculationModal(
                leaveRequestModal.EmployeeId,
                leaveRequestModal.LeaveFromDay,
                leaveRequestModal.LeaveToDay);
            
            LeavePlanType leavePlanType = leavePlanType =
                leaveCalculationModal.leavePlanTypes.Find(x => x.LeavePlanTypeId == leaveRequestModal.LeaveTypeId);

            if (leavePlanType == null)
                throw new HiringBellException("Request leave type not found. Please contact to admin.");

            await ProcessLeaveSections(leaveCalculationModal, leavePlanType);

            SameDayRequestValidationCheck(leaveCalculationModal);

            return leaveCalculationModal;
        }

        private async Task ProcessLeaveSections(LeaveCalculationModal leaveCalculationModal, LeavePlanType leavePlanType)
        {
            // get current leave plan configuration and check if its valid one.
            ValidateAndGetLeavePlanConfiguration(leavePlanType);
            leaveCalculationModal.leavePlanConfiguration = _leavePlanConfiguration;

            // call leave quote
            await _quota.CalculateFinalLeaveQuota(leaveCalculationModal, leavePlanType);

            // call leave by management

            // call leave accrual
            await _accrual.CalculateLeaveAccrual(leaveCalculationModal, leavePlanType);

            // call apply leave
            await _apply.CheckLeaveApplyRules(leaveCalculationModal, leavePlanType);

            // call leave restriction
            await _restriction.CheckRestrictionForLeave(leaveCalculationModal, leavePlanType);

            // call holiday and weekoff
            // call leave approval
            // call year end processing


            // await RunEmployeeLeaveAccrualCycle(leaveCalculationModal, leavePlanType);

            await Task.CompletedTask;
        }

        private async Task ComputeApplyingLeaveDays(LeaveCalculationModal leaveCalculationModal)
        {
            leaveCalculationModal.numberOfLeaveApplyring = 0;
            if (_leavePlanConfiguration.leaveHolidaysAndWeekoff.AdJoiningHolidayIsConsiderAsLeave)
            {

            }

            if (!_leavePlanConfiguration.leaveHolidaysAndWeekoff.AdjoiningWeekOffIsConsiderAsLeave)
            {
                var fromDate = _timezoneConverter.ToTimeZoneDateTime(
                    leaveCalculationModal.fromDate,
                    _currentSession.TimeZone
                    );

                var toDate = _timezoneConverter.ToTimeZoneDateTime(
                    leaveCalculationModal.toDate,
                    _currentSession.TimeZone
                    );

                while (toDate.Subtract(fromDate).TotalDays >= 0)
                {
                    if (fromDate.DayOfWeek != DayOfWeek.Saturday && fromDate.DayOfWeek != DayOfWeek.Sunday)
                        leaveCalculationModal.numberOfLeaveApplyring++;

                    fromDate = fromDate.AddDays(1);
                }
            }

            await Task.CompletedTask;
        }


        private void CheckSameDateAlreadyApplied(List<CompleteLeaveDetail> completeLeaveDetails, LeaveCalculationModal leaveCalculationModal)
        {
            try
            {
                if (completeLeaveDetails.Count > 0)
                {
                    decimal backDayLimit = _leavePlanConfiguration.leaveApplyDetail.BackDateLeaveApplyNotBeyondDays;
                    DateTime initFilterDate = now.AddDays(Convert.ToDouble(-backDayLimit));

                    var empLeave = completeLeaveDetails
                                    .Where(x => x.LeaveFromDay.Subtract(initFilterDate).TotalDays >= 0);
                    if (empLeave.Any())
                    {
                        var startDate = leaveCalculationModal.fromDate;
                        var endDate = leaveCalculationModal.toDate;
                        Parallel.ForEach(empLeave, i =>
                        {
                            if (i.LeaveFromDay.Month == startDate.Month)
                            {
                                if (startDate.Date.Subtract(i.LeaveFromDay.Date).TotalDays >= 0 &&
                                    startDate.Date.Subtract(i.LeaveToDay.Date).TotalDays <= 0)
                                    throw new HiringBellException($"From date: " +
                                        $"{_timezoneConverter.ToTimeZoneDateTime(startDate, _currentSession.TimeZone)} " +
                                        $"already exist in another leave request");
                            }

                            if (i.LeaveToDay.Month == endDate.Month)
                            {
                                if (endDate.Date.Subtract(i.LeaveFromDay.Date).TotalDays >= 0 &&
                                    endDate.Date.Subtract(i.LeaveToDay.Date).TotalDays <= 0)
                                    throw new HiringBellException($"To date: " +
                                        $"{_timezoneConverter.ToTimeZoneDateTime(endDate, _currentSession.TimeZone)} " +
                                        $"already exist in another leave request");
                            }
                        });
                    }
                }
            }
            catch (AggregateException ax)
            {
                if (ax.Flatten().InnerExceptions.Count > 0)
                {
                    var hex = ax.Flatten().InnerExceptions.ElementAt(0) as HiringBellException;
                    throw hex;
                }

                throw;
            }
        }

        private void SameDayRequestValidationCheck(LeaveCalculationModal leaveCalculationModal)
        {
            if (!string.IsNullOrEmpty(leaveCalculationModal.leaveRequestDetail.LeaveDetail))
            {
                List<CompleteLeaveDetail> completeLeaveDetails = JsonConvert.DeserializeObject<List<CompleteLeaveDetail>>(leaveCalculationModal.leaveRequestDetail.LeaveDetail);
                if (completeLeaveDetails.Count > 0)
                {
                    CheckSameDateAlreadyApplied(completeLeaveDetails, leaveCalculationModal);
                }
            }
        }

        private void ValidateAndGetLeavePlanConfiguration(LeavePlanType leavePlanType)
        {
            // fetching data from database using leaveplantypeId
            _leavePlanConfiguration = JsonConvert.DeserializeObject<LeavePlanConfiguration>(leavePlanType.PlanConfigurationDetail);
            if (_leavePlanConfiguration == null)
                throw new HiringBellException("Leave setup/configuration is not defined. Please complete the setup/configuration first.");
        }

        private void LoadCalculationData(long EmployeeId, LeaveCalculationModal leaveCalculationModal)
        {
            var ds = _db.FetchDataSet("sp_leave_plan_calculation_get", new
            {
                EmployeeId,
                _currentSession.CurrentUserDetail.ReportingManagerId,
                IsActive = 1,
                Year = now.Year
            }, false);

            if (ds != null && ds.Tables.Count == 5)
            {
                //if (ds.Tables[0].Rows.Count == 0 || ds.Tables[1].Rows.Count == 0 || ds.Tables[3].Rows.Count == 0)
                if (ds.Tables[0].Rows.Count == 0 || ds.Tables[1].Rows.Count == 0)
                    throw new HiringBellException("Fail to get employee related details. Please contact to admin.");

                leaveCalculationModal.employee = Converter.ToType<Employee>(ds.Tables[0]);
                leaveCalculationModal.leavePlanTypes = Converter.ToList<LeavePlanType>(ds.Tables[1]);
                leaveCalculationModal.leaveRequestDetail = Converter.ToType<LeaveRequestDetail>(ds.Tables[2]);

                if (!string.IsNullOrEmpty(leaveCalculationModal.leaveRequestDetail.LeaveQuotaDetail))
                    leaveCalculationModal.leaveRequestDetail.EmployeeLeaveQuotaDetail = JsonConvert
                        .DeserializeObject<List<EmployeeLeaveQuota>>(leaveCalculationModal.leaveRequestDetail.LeaveQuotaDetail);
                else
                    leaveCalculationModal.leaveRequestDetail.EmployeeLeaveQuotaDetail = new List<EmployeeLeaveQuota>();

                leaveCalculationModal.companySetting = Converter.ToType<CompanySetting>(ds.Tables[3]);
                leaveCalculationModal.leavePlan = Converter.ToType<LeavePlan>(ds.Tables[4]);
            }
            else
                throw new HiringBellException("Employee does not exist. Please contact to admin.");
        }

        private void CheckForProbationPeriod(LeaveCalculationModal leaveCalculationModal)
        {
            leaveCalculationModal.employeeType = ApplicationConstants.Regular;
            if ((leaveCalculationModal.employee.CreatedOn.AddDays(leaveCalculationModal.companySetting.ProbationPeriodInDays))
                .Subtract(now).TotalDays > 0)
            {
                leaveCalculationModal.employeeType = ApplicationConstants.InProbationPeriod;
                leaveCalculationModal.probationEndDate = leaveCalculationModal.employee
                    .CreatedOn.AddDays(leaveCalculationModal.companySetting.ProbationPeriodInDays);
            }
        }

        private void CheckForNoticePeriod(LeaveCalculationModal leaveCalculationModal)
        {
            if (leaveCalculationModal.employee.NoticePeriodId != 0 && leaveCalculationModal.employee.NoticePeriodAppliedOn != null)
                leaveCalculationModal.employeeType = ApplicationConstants.InNoticePeriod;
        }

        private async Task<LeaveCalculationModal> GetCalculationModal(long EmployeeId, DateTime FromDate, DateTime ToDate)
        {
            var leaveCalculationModal = new LeaveCalculationModal();
            leaveCalculationModal.fromDate = FromDate;
            leaveCalculationModal.toDate = ToDate;
            leaveCalculationModal.presentDate = DateTime.Now;

            // get employee detail and store it in class level variable
            LoadCalculationData(EmployeeId, leaveCalculationModal);

            // Check employee is in probation period
            CheckForProbationPeriod(leaveCalculationModal);

            // Check employee is in notice period
            CheckForNoticePeriod(leaveCalculationModal);


            await ComputeApplyingLeaveDays(leaveCalculationModal);
            return leaveCalculationModal;
        }

        private decimal LeaveLimitForCurrentType(int leavePlanTypeId, decimal availableLeaves, LeaveCalculationModal leaveCalculationModal)
        {
            decimal alreadyAppliedLeave = 0;

            if (!string.IsNullOrEmpty(leaveCalculationModal.leaveRequestDetail.LeaveDetail))
            {
                List<CompleteLeaveDetail> completeLeaveDetails = JsonConvert.DeserializeObject<List<CompleteLeaveDetail>>(leaveCalculationModal.leaveRequestDetail.LeaveDetail);
                if (completeLeaveDetails.Count > 0)
                {
                    alreadyAppliedLeave = completeLeaveDetails
                        .FindAll(x => x.LeaveTypeId == leavePlanTypeId && x.LeaveStatus != (int)ItemStatus.Rejected)
                        .Sum(x => x.NumOfDays);

                    leaveCalculationModal.lastApprovedLeaveDetail = completeLeaveDetails
                        .Where(x => x.LeaveStatus != (int)ItemStatus.Rejected)
                        .OrderByDescending(x => x.LeaveToDay).FirstOrDefault();
                }

            }

            alreadyAppliedLeave = availableLeaves - alreadyAppliedLeave;
            return alreadyAppliedLeave;
        }

        #region APPLY FOR LEAVE

        public async Task<LeaveCalculationModal> CheckAndApplyForLeave(LeaveRequestModal leaveRequestModal)
        {
            try
            {
                var leaveCalculationModal = await GetRequestTypeBalancedLeave(leaveRequestModal);

                LeavePlanType leavePlanType =
                    leaveCalculationModal.leavePlanTypes.Find(x => x.LeavePlanTypeId == leaveRequestModal.LeaveTypeId);


                var appliedDetail = await ApplyAndSaveChanges(leaveCalculationModal, leaveRequestModal);
                leavePlanType.AvailableLeave -= leaveCalculationModal.numberOfLeaveApplyring;
                return leaveCalculationModal;
            }
            catch
            {
                throw;
            }
        }

        private async Task<string> ApplyAndSaveChanges(LeaveCalculationModal leaveCalculationModal, LeaveRequestModal leaveRequestModal)
        {
            decimal totalAllocatedLeave = leaveCalculationModal.leavePlanTypes.Sum(x => x.MaxLeaveLimit);

            string result = string.Empty;
            List<CompleteLeaveDetail> leaveDetails = new List<CompleteLeaveDetail>();

            leaveCalculationModal.leaveRequestDetail.EmployeeId = leaveRequestModal.EmployeeId;
            leaveCalculationModal.leaveRequestDetail.Reason = leaveRequestModal.Reason;
            leaveCalculationModal.leaveRequestDetail.LeaveFromDay = leaveRequestModal.LeaveFromDay;
            leaveCalculationModal.leaveRequestDetail.LeaveToDay = leaveRequestModal.LeaveToDay;
            leaveCalculationModal.leaveRequestDetail.LeaveTypeId = leaveRequestModal.LeaveTypeId;

            if (leaveCalculationModal.leaveRequestDetail.LeaveDetail != null)
                leaveDetails = JsonConvert.DeserializeObject<List<CompleteLeaveDetail>>(leaveCalculationModal.leaveRequestDetail.LeaveDetail);

            CompleteLeaveDetail newLeaveDeatil = new CompleteLeaveDetail()
            {
                EmployeeId = leaveCalculationModal.leaveRequestDetail.EmployeeId,
                EmployeeName = leaveCalculationModal.employee.FirstName + " " + leaveCalculationModal.employee.LastName,
                AssignTo = leaveRequestModal.AssignTo,
                Session = leaveRequestModal.Session,
                LeaveTypeName = leaveRequestModal.LeavePlanName,
                LeaveTypeId = leaveRequestModal.LeaveTypeId,
                LeaveFromDay = leaveRequestModal.LeaveFromDay,
                LeaveToDay = leaveRequestModal.LeaveToDay,
                NumOfDays = Convert.ToDecimal(leaveCalculationModal.numberOfLeaveApplyring),
                LeaveStatus = (int)ItemStatus.Pending,
                Reason = leaveRequestModal.Reason,
                RequestedOn = DateTime.UtcNow
            };

            leaveDetails.Add(newLeaveDeatil);

            leaveCalculationModal.leaveRequestDetail.LeaveQuotaDetail = JsonConvert.SerializeObject(
                leaveCalculationModal.leavePlanTypes.Select(x => new EmployeeLeaveQuota
                {
                    LeavePlanTypeId = x.LeavePlanTypeId,
                    AvailableLeave = x.AvailableLeave
                }));

            leaveCalculationModal.leaveRequestDetail.LeaveDetail = JsonConvert.SerializeObject(leaveDetails);
            result = _db.Execute<LeaveRequestDetail>("sp_leave_notification_and_request_InsUpdate", new
            {
                leaveCalculationModal.leaveRequestDetail.LeaveRequestId,
                leaveCalculationModal.leaveRequestDetail.EmployeeId,
                leaveCalculationModal.leaveRequestDetail.LeaveDetail,
                leaveCalculationModal.leaveRequestDetail.Reason,
                AssignTo = _currentSession.CurrentUserDetail.ReportingManagerId,
                Year = leaveRequestModal.LeaveToDay.Year,
                leaveCalculationModal.leaveRequestDetail.LeaveFromDay,
                leaveCalculationModal.leaveRequestDetail.LeaveToDay,
                leaveCalculationModal.leaveRequestDetail.LeaveTypeId,
                RequestStatusId = (int)ItemStatus.Pending,
                AvailableLeaves = 0,
                TotalLeaveApplied = 0,
                TotalApprovedLeave = 0,
                TotalLeaveQuota = totalAllocatedLeave,
                leaveCalculationModal.leaveRequestDetail.LeaveQuotaDetail,
                NumOfDays = leaveCalculationModal.numberOfLeaveApplyring,
                LeaveRequestNotificationId = 0
            }, true);

            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("fail to insert or update");

            return await Task.FromResult(result);
        }

        #endregion
    }
}