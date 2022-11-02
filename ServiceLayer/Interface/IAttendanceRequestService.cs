using ModalLayer;
using ModalLayer.Modal;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface IAttendanceRequestService
    {
        RequestModel FetchPendingRequestService(long employeeId);
        RequestModel GetManagerAndUnAssignedRequestService(long employeeId);
        Task<RequestModel> ApproveAttendanceService(AttendanceDetails attendanceDetais, int filterId = ApplicationConstants.Only);
        Task<RequestModel> RejectAttendanceService(AttendanceDetails attendanceDetail, int filterId = ApplicationConstants.Only);
        RequestModel GetRequestPageData(long employeeId, int filterId);
        List<Attendance> ReAssigneAttendanceService(AttendanceDetails attendanceDetail);
    }
}
