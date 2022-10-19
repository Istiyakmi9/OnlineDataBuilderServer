using ModalLayer.Modal;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface ITimesheetRequestService
    {
        dynamic FetchPendingRequestService(long employeeId, int requestTypeId);
        List<DailyTimesheetDetail> ApprovalTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails);
        List<DailyTimesheetDetail> RejectTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails);
        List<DailyTimesheetDetail> ReAssigneTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails);
    }
}
