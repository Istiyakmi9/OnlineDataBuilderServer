using ModalLayer.Modal;
using System.Collections.Generic;
using System.Data;

namespace ServiceLayer.Interface
{
    public interface IAttendanceRequestService
    {
        dynamic FetchPendingRequestService(long employeeId, int requestTypeId);
        List<TimesheetDetail> ApprovalTimesheetService(TimesheetDetail timesheetDetail);
        List<TimesheetDetail> RejectTimesheetService(TimesheetDetail timesheetDetail);
        List<TimesheetDetail> ReAssigneTimesheetService(TimesheetDetail timesheetDetail);
    }
}
