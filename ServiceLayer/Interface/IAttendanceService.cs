using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using System;
using System.Collections.Generic;
using System.Data;

namespace ServiceLayer.Interface
{
    public interface IAttendanceService
    {
        List<AttendenceDetail> InsertUpdateTimesheet(List<AttendenceDetail> attendenceDetail);
        AttendanceWithClientDetail GetAttendanceByUserId(AttendenceDetail attendenceDetail);
        AttendanceWithClientDetail EnablePermission(AttendenceDetail attendenceDetail);
        string SubmitAttendanceService(AttendenceDetail commentDetails);
        dynamic ApplyLeaveService(LeaveDetails leaveDetail);
        dynamic GetAllLeavesByEmpIdService(long EmployeeId, int Year);
        List<AttendenceDetail> GetAllPendingAttendanceByUserIdService(long employeeId, int UserTypeId, long clientId);
        dynamic GetAttendamceById(AttendenceDetail attendenceDetail);
    }
}
