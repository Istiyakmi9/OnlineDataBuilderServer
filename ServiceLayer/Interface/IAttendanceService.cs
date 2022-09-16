using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface IAttendanceService
    {
        List<AttendenceDetail> InsertUpdateTimesheet(List<AttendenceDetail> attendenceDetail);
        AttendanceWithClientDetail GetAttendanceByUserId(AttendenceDetail attendenceDetail);
        AttendanceWithClientDetail EnablePermission(AttendenceDetail attendenceDetail);
        string SubmitAttendanceService(AttendenceDetail commentDetails);
        dynamic ApplyLeaveService(LeaveRequestDetail leaveDetail);
        Task<List<LeavePlanType>> ApplyLeaveService_Testing(ApplyLeave applyLeave);
        Task<dynamic> GetEmployeeLeaveDetail(ApplyLeave applyLeave);
        List<AttendenceDetail> GetAllPendingAttendanceByUserIdService(long employeeId, int UserTypeId, long clientId);
        dynamic GetAttendamceById(AttendenceDetail attendenceDetail);
    }
}
