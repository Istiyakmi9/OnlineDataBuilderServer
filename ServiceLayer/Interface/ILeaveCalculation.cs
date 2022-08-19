using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface ILeaveCalculation
    {
        Task<List<LeavePlanType>> GetBalancedLeave(long EmployeeId);
        bool CanReportingManagerAwardCasualCredits();
        decimal LeaveAccruedTillDate();
        decimal EffectIfJoinInMidOfYear();
        decimal EffectIfExitInMidOfYear();
        decimal LeaveBasedOnProbationOrExprience();
        decimal AttendanceImpactOnLeave();
        decimal WeekOffAndHolidayConsideration();
        decimal FutureDateLeave();
        decimal ExtraLeaveOnAccrualBalance();
        decimal RoundUpTheLeaveIfPresentInFraction();
        decimal LeaveExpiryCalculation();
    }
}
