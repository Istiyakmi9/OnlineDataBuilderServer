using ModalLayer.Modal;
using System;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface IAttendanceService
    {
        List<AttendenceDetail> InsertUpdateAttendance(List<AttendenceDetail> attendenceDetail);
        AttendanceWithClientDetail GetAttendanceByUserId(AttendenceDetail attendenceDetail);
        List<DateTime> GetAllPendingAttendanceByUserIdService(long employeeId, int UserTypeId);
    }
}
