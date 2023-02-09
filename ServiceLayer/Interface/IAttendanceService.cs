using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface IAttendanceService
    {
        AttendanceWithClientDetail GetAttendanceByUserId(AttendenceDetail attendenceDetail);
        AttendanceWithClientDetail EnablePermission(AttendenceDetail attendenceDetail);
        Task<string> SubmitAttendanceService(AttendenceDetail commentDetails);
        Task<string> RaiseMissingAttendanceRequestService(CompalintOrRequestWithEmail compalintOrRequest);
        Task<List<CompalintOrRequest>> GetMissingAttendanceRequestService(FilterModel filter);
        List<AttendenceDetail> GetAllPendingAttendanceByUserIdService(long employeeId, int UserTypeId, long clientId);
        dynamic GetEmployeePerformanceService(AttendenceDetail attendanceDetails);
    }
}
