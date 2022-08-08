using ModalLayer.Modal;
using System.Data;

namespace ServiceLayer.Interface
{
    public interface ITimesheetService
    {
        dynamic GetTimesheetByUserIdService(TimesheetDetail timesheetDetail);
    }
}
