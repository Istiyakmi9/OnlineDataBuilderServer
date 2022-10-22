using ModalLayer.Modal;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface IAttendanceRequestService
    {
        RequestModel FetchPendingRequestService(long employeeId, int requestTypeId);
        RequestModel GetManagerAndUnAssignedRequestService(long employeeId, int requestTypeId);
        RequestModel ApprovalAttendanceService(AttendanceDetails attendanceDetais);
        RequestModel RejectAttendanceService(AttendanceDetails attendanceDetail);
        List<Attendance> ReAssigneAttendanceService(AttendanceDetails attendanceDetail);
    }
}
