using ModalLayer.Modal;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface ITimesheetRequestService
    {
        RequestModel ApprovalTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails, int filterId = 1);
        RequestModel RejectTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails, int filterId = 1);
        List<DailyTimesheetDetail> ReAssigneTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails, int filterId = 1);
    }
}
