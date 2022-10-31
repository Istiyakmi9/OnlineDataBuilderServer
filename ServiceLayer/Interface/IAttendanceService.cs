using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface IAttendanceService
    {
        List<AttendenceDetail> InsertUpdateTimesheet(List<AttendenceDetail> attendenceDetail);
        AttendanceWithClientDetail GetAttendanceByUserId(AttendenceDetail attendenceDetail);
        AttendanceWithClientDetail EnablePermission(AttendenceDetail attendenceDetail);
        string SubmitAttendanceService(AttendenceDetail commentDetails);
        List<AttendenceDetail> GetAllPendingAttendanceByUserIdService(long employeeId, int UserTypeId, long clientId);
    }
}
