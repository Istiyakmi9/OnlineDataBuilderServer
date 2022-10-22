using ModalLayer.Modal;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface ITimesheetRequestService
    {
        RequestModel ApprovalTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails);
        RequestModel RejectTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails);
        List<DailyTimesheetDetail> ReAssigneTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails);
    }
}
