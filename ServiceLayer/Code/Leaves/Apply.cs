using BottomhalfCore.Services.Interface;
using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using System;
using System.Threading.Tasks;

namespace ServiceLayer.Code.Leaves
{
    public class Apply
    {
        private LeavePlanConfiguration _leavePlanConfiguration;
        private LeavePlanType _leavePlanType;

        private readonly ITimezoneConverter _timezoneConverter;

        public Apply(ITimezoneConverter timezoneConverter)
        {
            _timezoneConverter = timezoneConverter;
        }

        public async Task CheckLeaveApplyRules(LeaveCalculationModal leaveCalculationModal, LeavePlanType leavePlanType)
        {
            _leavePlanType = leavePlanType;
            _leavePlanConfiguration = leaveCalculationModal.leavePlanConfiguration;

            CheckForHalfDayRestriction();

            AllowToSeeAndApply();

            LeaveEligibilityCheck(leaveCalculationModal);

            DoesLeaveRequiredComments();

            RequiredDocumentForExtending(leaveCalculationModal);

            await Task.CompletedTask;
        }

        // step - 1
        private void CheckForHalfDayRestriction()
        {
            if (!_leavePlanConfiguration.leaveApplyDetail.IsAllowForHalfDay)
            {
                //if (leaveCalculationModal.leaveRequestDetail.Session.Contains("halfday"))
                //    throw new HiringBellException("Apply halfday is not allowed for this type");
            }
        }

        // step - 2
        public void AllowToSeeAndApply()
        {
            if (!_leavePlanConfiguration.leaveApplyDetail.EmployeeCanSeeAndApplyCurrentPlanLeave)
                throw new HiringBellException("You don't have enough permission to apply this leave.");
        }

        // step - 3, 4
        private void LeaveEligibilityCheck(LeaveCalculationModal leaveCalculationModal)
        {
            // if future date then > 0 else < 0
            var presentDate = _timezoneConverter.ToUtcTime(DateTime.SpecifyKind(leaveCalculationModal.presentDate.Date, DateTimeKind.Unspecified));
            double days = leaveCalculationModal.fromDate.Subtract(presentDate).TotalDays;
            if (days < 0) // past date
            {
                days = days * -1;
                if (_leavePlanConfiguration.leaveApplyDetail.BackDateLeaveApplyNotBeyondDays != -1 &&
                    days > _leavePlanConfiguration.leaveApplyDetail.BackDateLeaveApplyNotBeyondDays)
                    throw new HiringBellException($"Back dated leave more than {_leavePlanConfiguration.leaveApplyDetail.BackDateLeaveApplyNotBeyondDays} days can't be allowed.");
            }
            else // future date
            {
                if (_leavePlanConfiguration.leaveApplyDetail.ApplyPriorBeforeLeaveDate != -1 &&
                    days > _leavePlanConfiguration.leaveApplyDetail.ApplyPriorBeforeLeaveDate)
                    throw new HiringBellException($"Apply this leave before {_leavePlanConfiguration.leaveApplyDetail.ApplyPriorBeforeLeaveDate} days.");
            }
        }

        private bool CheckProbationLeaveRestriction(LeavePlanType leavePlanType, LeaveCalculationModal leaveCalculationModal)
        {
            bool flag = true;
            // if employee completed probation period
            if (_leavePlanConfiguration.leavePlanRestriction.CanApplyAfterProbation && leaveCalculationModal.employeeType == 0)
            {
                var totalDays = Convert.ToDouble(_leavePlanConfiguration.leavePlanRestriction.DaysAfterProbation) +
                                leaveCalculationModal.employee.ProbationPeriodDaysLimit;

                if (leaveCalculationModal.fromDate.Subtract(leaveCalculationModal.employee.CreatedOn.AddDays(totalDays)).TotalDays < 0)
                {
                    flag = false;
                    leavePlanType.IsApplicable = false;
                    leavePlanType.Reason = $"This leave can only be utilized {_leavePlanConfiguration.leaveApplyDetail.ApplyPriorBeforeLeaveDate} days before leave start date.";
                }
            }
            else if (leaveCalculationModal.employeeType == 1) // if employee currently serving probation period
            {
                DateTime afterJoiningDate = leaveCalculationModal.employee.CreatedOn.AddDays(Convert.ToDouble(_leavePlanConfiguration.leavePlanRestriction.DaysAfterJoining));
                if (afterJoiningDate.Subtract(leaveCalculationModal.employee.CreatedOn.AddDays(leaveCalculationModal.employee.ProbationPeriodDaysLimit)).TotalDays <= 0)
                {
                    var totalNumOfDays = leaveCalculationModal.toDate.Subtract(leaveCalculationModal.fromDate).TotalDays;
                    if (_leavePlanConfiguration.leavePlanRestriction.IsAvailRestrictedLeavesInProbation)
                    {
                        if (Convert.ToDouble(_leavePlanConfiguration.leavePlanRestriction.LeaveLimitInProbation) - totalNumOfDays > 0)
                        {
                            flag = false;
                            leavePlanType.IsApplicable = false;
                            leavePlanType.Reason = $"This leave can only be utilized {_leavePlanConfiguration.leaveApplyDetail.ApplyPriorBeforeLeaveDate} days before leave start date.";
                        }
                    }
                }
            }

            return flag;
        }

        // step - 5
        private void DoesLeaveRequiredComments()
        {
            if (_leavePlanConfiguration.leaveApplyDetail.CurrentLeaveRequiredComments)
            {
                //if (string.IsNullOrEmpty(leaveCalculationModal.leaveRequestDetail.Reason))
                //    throw new HiringBellException("Comment is required for this type");
            }

        }

        // step - 6
        private void RequiredDocumentForExtending(LeaveCalculationModal leaveCalculationModal)
        {
            if (_leavePlanConfiguration.leaveApplyDetail.ProofRequiredIfDaysExceeds)
            {
                var leaveDay = leaveCalculationModal.toDate.Date.Subtract(leaveCalculationModal.fromDate.Date).TotalDays;
                if (leaveDay > _leavePlanConfiguration.leaveApplyDetail.NoOfDaysExceeded)
                    throw new HiringBellException("Document proof is required");
            }
        }
    }
}
