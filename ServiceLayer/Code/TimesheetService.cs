using ModalLayer.Modal;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ServiceLayer.Code
{
    public class TimesheetService : ITimesheetService
    {
        priv
        public DataSet GetTimesheetByUserIdService(TimesheetDetail timesheetDetail)
        {
            List<TimesheetDetail> timesheetDetails = null;
            Employee employee = null;

            if (timesheetDetail.ForMonth <= 0)
                throw new HiringBellException("Invalid month num. passed.", nameof(timesheetDetail.ForMonth), timesheetDetail.ForMonth.ToString());

            if (Convert.ToDateTime(timesheetDetail.AttendenceFromDay).Subtract(DateTime.UtcNow).TotalDays > 0)
            {
                throw new HiringBellException("Ohh!!!. Future dates are now allowed.");
            }


            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(attendenceDetail.EmployeeUid, typeof(int), "_EmployeeId"),
                new DbParam(attendenceDetail.ClientId, typeof(int), "_ClientId"),
                new DbParam(attendenceDetail.UserTypeId, typeof(int), "_UserTypeId"),
                new DbParam(attendenceDetail.ForYear, typeof(int), "_ForYear"),
                new DbParam(attendenceDetail.ForMonth, typeof(int), "_ForMonth")
            };

            var Result = _db.GetDataset("sp_attendance_get", dbParams);
            if (Result.Tables.Count == 2)
            {

                if (Result.Tables[1].Rows.Count > 0)
                {
                    employee = Converter.ToType<Employee>(Result.Tables[1]);
                    if (Convert.ToDateTime(attendenceDetail.AttendenceToDay).Subtract(employee.CreatedOn).TotalDays < 0)
                    {
                        throw new HiringBellException("Past date before DOJ not allowed.");
                    }
                }

                if (Result.Tables[0].Rows.Count > 0 && employee != null)
                {
                    var data = Result.Tables[0].Rows[0]["AttendanceDetail"].ToString();
                    var attendanceId = Convert.ToInt64(Result.Tables[0].Rows[0]["AttendanceId"]);
                    attendenceDetails = JsonConvert.DeserializeObject<List<AttendenceDetail>>(data);
                    int status = this.IsGivenDateAllowed((DateTime)attendenceDetail.AttendenceFromDay, (DateTime)attendenceDetail.AttendenceToDay, attendenceDetails);

                    attendenceDetails.ForEach(x =>
                    {
                        x.AttendanceId = attendanceId;
                        x.IsActiveDay = false;
                        x.IsOpen = status == 1 ? true : false;
                    });

                    int i = 0;
                    var generatedAttendance = this.GenerateWeekAttendaceData(attendenceDetail, status);
                    AttendenceDetail attrDetail = null;
                    foreach (var item in attendenceDetails)
                    {
                        i = 0;
                        while (i < generatedAttendance.Count)
                        {
                            attrDetail = generatedAttendance.ElementAt(i);
                            if (attrDetail.AttendanceDay.Date.Subtract(item.AttendanceDay.Date).TotalDays == 0)
                            {
                                generatedAttendance[i] = item;
                                break;
                            }
                            i++;
                        }
                    }

                    attendenceDetails = generatedAttendance;
                }
                else
                {
                    int status = this.IsGivenDateAllowed((DateTime)attendenceDetail.AttendenceFromDay, (DateTime)attendenceDetail.AttendenceToDay, null);
                    attendenceDetails = this.GenerateWeekAttendaceData(attendenceDetail, status);
                }

                if (this.IsRegisteredOnPresentWeek(employee.CreatedOn) == 1)
                {
                    attendenceDetails = attendenceDetails.Where(x => employee.CreatedOn.Date.Subtract(x.AttendanceDay.Date).TotalDays <= 0).ToList();
                }
            }

            return new AttendanceWithClientDetail { EmployeeDetail = employee, AttendacneDetails = attendenceDetails.OrderByDescending(x => x.AttendanceDay).ToList() };
        }
    }
}
