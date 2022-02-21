using ModalLayer.Modal;
using System.Collections.Generic;
using System.Data;

namespace ServiceLayer.Interface
{
    public interface IAttendanceService
    {
        string InsertUpdateAttendance(List<AttendenceDetail> attendenceDetail);
        DataSet GetAttendanceByUserId(AttendenceDetail attendenceDetail);
    }
}
