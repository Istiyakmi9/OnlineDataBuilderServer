using ModalLayer.Modal;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface IAttendanceRequestService
    {
        RequestModel FetchPendingRequestService(long employeeId);
        RequestModel GetManagerAndUnAssignedRequestService(long employeeId);
        RequestModel ApprovalAttendanceService(AttendanceDetails attendanceDetais);
        RequestModel ApproveAttendanceService(int filterId, AttendanceDetails attendanceDetais);
        RequestModel RejectAttendanceService(AttendanceDetails attendanceDetail);
        List<Attendance> ReAssigneAttendanceService(AttendanceDetails attendanceDetail);
    }
}
