using ModalLayer.Modal;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface ITimesheetService
    {
        dynamic GetTimesheetByUserIdService(TimesheetDetail timesheetDetail);
        List<DailyTimesheetDetail> InsertUpdateTimesheet(List<DailyTimesheetDetail> dailyTimesheetDetail);
        List<TimesheetDetail> GetPendingTimesheetByIdService(long employeeId, long clientId);
        dynamic GetEmployeeTimeSheetService(TimesheetDetail timesheetDetail);
        List<DailyTimesheetDetail> UpdateTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails, string comment);
    }
}
