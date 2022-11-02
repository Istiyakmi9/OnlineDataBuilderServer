using ModalLayer.Modal;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface ITimesheetRequestService
    {
        Task<RequestModel> ApprovalTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails, int filterId = 1);
        Task<RequestModel> RejectTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails, int filterId = 1);
        List<DailyTimesheetDetail> ReAssigneTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails, int filterId = 1);
    }
}
