using ModalLayer;
using ModalLayer.Modal;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface IAttendanceRequestService
    {
        RequestModel FetchPendingRequestService(long employeeId);
        RequestModel GetManagerAndUnAssignedRequestService(long employeeId);
        RequestModel ApproveAttendanceService(AttendanceDetails attendanceDetais, int filterId = ApplicationConstants.Only);
        RequestModel RejectAttendanceService(AttendanceDetails attendanceDetail, int filterId = ApplicationConstants.Only);
        RequestModel GetRequestPageData(long employeeId, int filterId);
        List<Attendance> ReAssigneAttendanceService(AttendanceDetails attendanceDetail);
        EmailSenderModal PrepareSendEmailNotification(EmployeeNotificationModel notification, EmailTemplate template);
    }
}
