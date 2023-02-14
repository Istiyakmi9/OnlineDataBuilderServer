﻿using ModalLayer.Modal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface ITimesheetService
    {
        List<TimesheetDetail> GetTimesheetByUserIdService(TimesheetDetail timesheetDetail);
        Task<TimesheetDetail> GetWeekTimesheetDataService(TimesheetDetail timesheetDetail);
        Task<TimesheetDetail> SubmitTimesheetService(TimesheetDetail timesheetDetail);
        Task<string> ExecuteActionOnTimesheetService(TimesheetDetail timesheetDetail);
        List<TimesheetDetail> GetPendingTimesheetByIdService(long employeeId, long clientId);
        dynamic GetEmployeeTimeSheetService(TimesheetDetail timesheetDetail);
        (List<DailyTimesheetDetail>, List<DateTime>) BuildFinalTimesheet(TimesheetDetail currentTimesheetDetail);
        BillingDetail EditEmployeeBillDetailService(GenerateBillFileDetail fileDetail);
        Task RunWeeklyTimesheetCreation(DateTime TimesheetStartDate);
        List<TimesheetDetail> GetTimesheetByFilterService(FilterModel filterModel);
    }
}
