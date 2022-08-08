using ModalLayer.Modal;
using System.Data;

namespace ServiceLayer.Interface
{
    public interface ITimesheetService
    {
        DataSet GetTimesheetByUserIdService(TimesheetDetail timesheetDetail);
    }
}
