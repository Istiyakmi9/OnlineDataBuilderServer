using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceLayer.Code
{
    public class LeaveCalculation : ILeaveCalculation
    {
        private readonly IDb _db;
        private Employee _employee;
        private Leave _employeeLeaveDetail;
        private LeavePlan _leavePlan;
        private LeavePlanConfiguration _leavePlanConfiguration;
        private readonly DateTime now = DateTime.UtcNow;

        public LeaveCalculation(IDb db)
        {
            _db = db;
        }

        private decimal CalculateLeaveIfInProbationPeriond(int leavePlanTypeId)
        {
            decimal leaveCount = 0;

            return leaveCount;
        }

        private decimal CalculateLeaveIfInNoticePeriond(int leavePlanTypeId)
        {
            decimal leaveCount = 0;

            return leaveCount;
        }

        private decimal LeaveLimitBeyondQuota() { return 0; }

        public bool CanReportingManagerAwardCasualCredits() { return false; }

        public decimal LeaveAccruedTillDate()
        {
            return 0;
        }

        public decimal EffectIfJoinInMidOfYear() { return 0; }
        public decimal EffectIfExitInMidOfYear() { return 0; }

        public decimal LeaveBasedOnProbationOrExprience() { return 0; }
        public decimal AttendanceImpactOnLeave() { return 0; }
        public decimal WeekOffAndHolidayConsideration() { return 0; }
        public decimal FutureDateLeave() { return 0; }
        public decimal ExtraLeaveOnAccrualBalance() { return 0; }
        public decimal RoundUpTheLeaveIfPresentInFraction() { return 0; }
        public decimal LeaveExpiryCalculation() { return 0; }

        private decimal CalculateFirstMonthOnDefinedLeaveRate(decimal perMonthLeaves)
        {
            decimal firstMonthProrateCount = 0;

            return firstMonthProrateCount;
        }

        private decimal CalculateFirstMonthProrateCount(decimal perMonthLeaves)
        {
            decimal firstMonthProrateCount = 0;
            if (_leavePlanConfiguration.leaveAccrual.IsLeavesProratedForJoinigMonth)
            {
                if (_employee.CreatedOn.Day >= 15)
                    // if joining date is after 15 then add half the number of days defined per month as accral
                    firstMonthProrateCount = perMonthLeaves / 2;
                else
                    // if joining date is before 15 then add full the number of days defined per month as accral
                    firstMonthProrateCount = perMonthLeaves;
            }
            else
            {
                firstMonthProrateCount = CalculateFirstMonthOnDefinedLeaveRate(perMonthLeaves);
            }

            return firstMonthProrateCount;
        }

        private decimal CalculatePresentMonthProrateCount(decimal perMonthLeaves)
        {
            decimal presentMonthDays = 0;
            if (_leavePlanConfiguration.leaveAccrual.LeaveDistributionAppliedFrom - now.Day <= 0)
                // if present month date is less then or equal to the date define for accrual
                presentMonthDays = perMonthLeaves;
            else
                // if present month date is greather than to the date define for accrual
                presentMonthDays = 0;

            return presentMonthDays;
        }

        private decimal CalculateWhenAccralMonthly(decimal alreadyAppliedLeave)
        {
            decimal availableLeaves = 0;

            decimal perMonthLeaves = _leavePlanConfiguration.leaveDetail.LeaveLimit / 12;
            decimal presentMonthLeaves = CalculatePresentMonthProrateCount(perMonthLeaves);
            int mounthCount = 0;

            if (_employee.CreatedOn.Year == now.Year)
            {
                decimal firstMonthCalculation = CalculateFirstMonthProrateCount(perMonthLeaves);
                mounthCount = now.Month - _employee.CreatedOn.Month;
                mounthCount = mounthCount >= 2 ? mounthCount - 1 : 0;
                availableLeaves = (mounthCount * perMonthLeaves) + firstMonthCalculation + presentMonthLeaves;
            }
            else
            {
                availableLeaves = (now.Month - 1) * perMonthLeaves + presentMonthLeaves;
            }

            return availableLeaves;
        }

        private decimal CalculateWhenAccralQuaterly(decimal alreadyAppliedLeave)
        {
            decimal availableLeaves = 0;

            return availableLeaves;
        }

        private decimal CalculateWhenAccralYearly(decimal alreadyAppliedLeave)
        {
            decimal availableLeaves = 0;

            return availableLeaves;
        }

        private decimal CalculateLeaveIfRegularEmployement(int leavePlanTypeId)
        {
            decimal alreadyAppliedLeave = 0;
            decimal availableLeaves = 0;
            decimal leaveLimit = 0;

            List<CompleteLeaveDetail> completeLeaveDetails = JsonConvert.DeserializeObject<List<CompleteLeaveDetail>>(_employeeLeaveDetail.LeaveDetail);
            if (completeLeaveDetails.Count > 0)
                alreadyAppliedLeave = completeLeaveDetails.FindAll(x => x.LeaveType == leavePlanTypeId && x.LeaveStatus == (int)ItemStatus.Approved).Sum(x => x.NumOfDays);

            leaveLimit = _leavePlanConfiguration.leaveDetail.LeaveLimit;
            if (_leavePlanConfiguration.leaveAccrual.CanApplyEntireLeave)
            {
                availableLeaves = leaveLimit - alreadyAppliedLeave;
            }
            else
            {
                var leaveDistributedSeq = _leavePlanConfiguration.leaveAccrual.LeaveDistributionSequence;
                switch (leaveDistributedSeq)
                {
                    case "1":
                        availableLeaves = this.CalculateWhenAccralMonthly(alreadyAppliedLeave);
                        break;
                    case "2":
                        availableLeaves = this.CalculateWhenAccralQuaterly(alreadyAppliedLeave);
                        break;
                    case "3":
                        availableLeaves = this.CalculateWhenAccralYearly(alreadyAppliedLeave);
                        break;
                }

                availableLeaves = leaveLimit - (availableLeaves - alreadyAppliedLeave);
            }

            return availableLeaves;
        }

        private LeavePlanConfiguration FetchLeavePlanConfigurationById(int leavePlanTypeId)
        {
            LeavePlanType leavePlanType = _db.Get<LeavePlanType>("sp_leave_plans_type_getbyId", new { LeavePlanTypeId = leavePlanTypeId });
            if (leavePlanType == null || string.IsNullOrEmpty(leavePlanType.PlanConfigurationDetail))
                throw new HiringBellException("Invalid plan id supplied");

            LeavePlanConfiguration leavePlanConfiguration = JsonConvert.DeserializeObject<LeavePlanConfiguration>(leavePlanType.PlanConfigurationDetail);
            return leavePlanConfiguration;
        }

        private int GetEmployeeType(long EmployeeId)
        {
            var employee = _db.Get<Employee>("SP_Employees_ById", new { EmployeeId, IsActive = 1 });
            if (employee == null)
                throw new HiringBellException("Employee does not exist. Please contact to admin.");

            int type = 0;
            if (now.Subtract(employee.CreatedOn).TotalDays <= employee.ProbationPeriodDaysLimit)
                type = 1;

            if (employee.NoticePeriodAppriedOn != null && now.Subtract((DateTime)employee.NoticePeriodAppriedOn).TotalDays <= employee.NoticePeriodDaysLimit)
                type = 2;

            _employee = employee;
            return type;
        }

        private void BuildLeaveCalculationInstances(long EmployeeId)
        {
            // List<Employee> employees = null;
            DataSet result = _db.FetchDataSet("sp_employee_leave_request_detail", new { EmployeeId = EmployeeId, Year = now.Year });

            if (result.Tables.Count != 3)
                throw new HiringBellException("Unable to get employee leave deatils");
            else
            {
                _employeeLeaveDetail = Converter.ToType<Leave>(result.Tables[0]);
                if (_employeeLeaveDetail == null)
                    throw new HiringBellException("Unable to find leave detail for the current employee. Please contact to admin.");

                _leavePlan = Converter.ToType<LeavePlan>(result.Tables[1]);
                if (_leavePlan == null)
                    throw new HiringBellException("Unable to find leave plan for the current employee. Please contact to admin.");

                // employees = Converter.ToList<Employee>(result.Tables[2]);
            }
        }

        private void ValidateAndGetLeavePlanConfiDetail(List<LeavePlanType> leavePlanTypes)
        {
            var leavePlanTypeId = leavePlanTypes.First().LeavePlanTypeId;
            if (leavePlanTypeId <= 0)
                throw new HiringBellException("Leave plan type detail is not configured properly.");

            // fetching data from database using leaveplantypeId
            _leavePlanConfiguration = FetchLeavePlanConfigurationById(leavePlanTypeId);
            if (_leavePlanConfiguration == null)
                throw new HiringBellException("Leave setup/configuration is not defined. Please complete the setup/configuration first.");
        }

        public async Task<List<LeavePlanType>> GetBalancedLeave(long EmployeeId)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            // get employee detail and store it in class level variable
            int type = this.GetEmployeeType(EmployeeId);

            // creating leave plan and detail objects for calculation
            BuildLeaveCalculationInstances(EmployeeId);

            if (_leavePlan.AssociatedPlanTypes == null)
                throw new HiringBellException("No plan is associated into your account. Please contact to admin.");

            List<LeavePlanType> leavePlanTypes = JsonConvert.DeserializeObject<List<LeavePlanType>>(_leavePlan.AssociatedPlanTypes);
            if (leavePlanTypes == null || leavePlanTypes.Count == 0)
                throw new HiringBellException("No plan is associated into your account. Please contact to admin.");

            this.ValidateAndGetLeavePlanConfiDetail(leavePlanTypes);

            return await Task.Run<List<LeavePlanType>>(() =>
            {
                LeavePlanType leavePlanType = null;
                int i = 0;
                while (i < leavePlanTypes.Count)
                {
                    leavePlanType = leavePlanTypes[i];
                    if (_leavePlanConfiguration.leaveDetail.IsLeaveDaysLimit == true)
                    {
                        var leaveLimit = _leavePlanConfiguration.leaveDetail.LeaveLimit;
                        if (leaveLimit <= 0)
                            throw new HiringBellException("leave limit is null or empty");

                        switch (type)
                        {
                            case 1:
                                leavePlanType.AvailableLeave = CalculateLeaveIfInProbationPeriond(leavePlanType.LeavePlanTypeId);
                                break;
                            case 2:
                                leavePlanType.AvailableLeave = CalculateLeaveIfInNoticePeriond(leavePlanType.LeavePlanTypeId);
                                break;
                            default:
                                leavePlanType.AvailableLeave = CalculateLeaveIfRegularEmployement(leavePlanType.LeavePlanTypeId);
                                break;
                        }
                    }
                    else
                        leavePlanType.AvailableLeave = -1;
                }

                return leavePlanTypes;
            }, cts.Token);
        }
    }
}