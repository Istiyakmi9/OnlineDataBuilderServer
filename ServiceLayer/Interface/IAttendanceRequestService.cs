using ModalLayer.Modal;
using System.Collections.Generic;
using System.Data;

namespace ServiceLayer.Interface
{
    public interface IAttendanceRequestService
    {
        dynamic FetchPendingRequestService(long employeeId, int requestTypeId);
        dynamic ApprovalAttendanceService(AttendanceDetails attendanceDetais);
        dynamic RejectAttendanceService(AttendanceDetails attendanceDetail);
        List<Attendance> ReAssigneAttendanceService(AttendanceDetails attendanceDetail);
    }
}
