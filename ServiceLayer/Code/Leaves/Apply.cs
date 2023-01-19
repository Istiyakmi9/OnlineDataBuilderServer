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

            LeaveEligibilityCheck(leaveCalculationModal);

            await RequiredDocumentForExtending(leaveCalculationModal);

            await Task.CompletedTask;
        }

        // step - 1
        public void CheckForHalfDayRestriction()
        {
            if (!_leavePlanConfiguration.leaveApplyDetail.IsAllowForHalfDay)
            {
                throw new HiringBellException("Half day leave not allow under current leave type.");
            }
        }

        // step - 2
        public bool IsAllowedToSeeAndApply()
        {
            bool flag = false;
            if (_leavePlanConfiguration.leaveApplyDetail.EmployeeCanSeeAndApplyCurrentPlanLeave)
                flag = true;

            return flag;
        }

        // step - 3, 4
        private void LeaveEligibilityCheck(LeaveCalculationModal leaveCalculationModal)
        {
            // if future date then > 0 else < 0
            var presentDate = _timezoneConverter.ToUtcTime(DateTime.SpecifyKind(leaveCalculationModal.presentDate.Date, DateTimeKind.Unspecified));
            double days = leaveCalculationModal.fromDate.Date.Subtract(presentDate.Date).TotalDays;
            // step - 3
            if (days < 0) // past date
            {
                days = days * -1;
                if (_leavePlanConfiguration.leaveApplyDetail.BackDateLeaveApplyNotBeyondDays != -1 &&
                    days > _leavePlanConfiguration.leaveApplyDetail.BackDateLeaveApplyNotBeyondDays)
                    throw new HiringBellException($"Applying for this leave required minimun of " +
                        $"{_leavePlanConfiguration.leaveApplyDetail.BackDateLeaveApplyNotBeyondDays} days gap.");
            }
            else // step - 4  future date
            {
                if (_leavePlanConfiguration.leaveApplyDetail.ApplyPriorBeforeLeaveDate != -1 &&
                    days > _leavePlanConfiguration.leaveApplyDetail.ApplyPriorBeforeLeaveDate)
                    throw new HiringBellException($"Applying for this leave required minimun of " +
                        $"{_leavePlanConfiguration.leaveApplyDetail.ApplyPriorBeforeLeaveDate} days gap.");
            }
        }


        // step - 5
        public void DoesLeaveRequiredComments()
        {
            if (_leavePlanConfiguration.leaveApplyDetail.CurrentLeaveRequiredComments)
            {
                //if (string.IsNullOrEmpty(leaveCalculationModal.leaveRequestDetail.Reason))
                //    throw new HiringBellException("Comment is required for this type");
            }
        }

        // step - 6
        public async Task<bool> RequiredDocumentForExtending(LeaveCalculationModal leaveCalculationModal)
        {
            bool flag = false;
            if (_leavePlanConfiguration.leaveApplyDetail.ProofRequiredIfDaysExceeds)
            {
                var leaveDay = leaveCalculationModal.fromDate.Date.Subtract(leaveCalculationModal.toDate.Date).TotalDays;
                if (leaveDay > _leavePlanConfiguration.leaveApplyDetail.NoOfDaysExceeded)
                    flag = true;
            }

            return await Task.FromResult(flag);
        }
    }
}
