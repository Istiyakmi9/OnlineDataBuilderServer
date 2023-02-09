using ModalLayer;
using ModalLayer.Modal;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface IAttendanceRequestService
    {
        RequestModel FetchPendingRequestService(long employeeId, ItemStatus itemStatus = ItemStatus.Pending);
        RequestModel GetManagerAndUnAssignedRequestService(long employeeId);
        Task<RequestModel> ApproveAttendanceService(AttendenceDetail attendanceDetais, int filterId = ApplicationConstants.Only);
        Task<RequestModel> RejectAttendanceService(AttendenceDetail attendanceDetail, int filterId = ApplicationConstants.Only);
        RequestModel GetRequestPageData(long employeeId, int filterId);
        List<Attendance> ReAssigneAttendanceService(AttendenceDetail attendanceDetail);
    }
}
