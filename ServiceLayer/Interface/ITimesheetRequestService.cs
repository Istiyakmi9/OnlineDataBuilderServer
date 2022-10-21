using ModalLayer.Modal;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface ITimesheetRequestService
    {
        dynamic ApprovalTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails);
        dynamic RejectTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails);
        List<DailyTimesheetDetail> ReAssigneTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails);
    }
}
