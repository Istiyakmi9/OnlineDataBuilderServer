using ModalLayer.Modal;
using System;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface IAttendanceService
    {
        List<AttendenceDetail> InsertUpdateAttendance(List<AttendenceDetail> attendenceDetail);
        AttendanceWithClientDetail GetAttendanceByUserId(AttendenceDetail attendenceDetail);
        string AddComment(AttendenceDetail commentDetails);
        List<AttendenceDetail> GetAllPendingAttendanceByUserIdService(long employeeId, int UserTypeId, long clientId);
        dynamic GetAttendamceById(AttendenceDetail attendenceDetail);
    }
}
