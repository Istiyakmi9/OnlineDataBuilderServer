using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using BottomhalfCore.Services.Interface;
using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLayer.Code
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;
        private readonly ITimezoneConverter _timezoneConverter;
        private readonly ILeaveCalculation _leaveCalculation;

        public AttendanceService(IDb db, ILeaveCalculation leaveCalculation, ITimezoneConverter timezoneConverter, CurrentSession currentSession)
        {
            _db = db;
            _currentSession = currentSession;
            _timezoneConverter = timezoneConverter;
            _leaveCalculation = leaveCalculation;
        }

        public AttendanceWithClientDetail GetAttendanceByUserId(AttendenceDetail attendenceDetail)
        {
            List<AttendenceDetail> attendenceDetails = null;
            Employee employee = null;

            if (attendenceDetail.ForMonth <= 0)
                throw new HiringBellException("Invalid month num. passed.", nameof(attendenceDetail.ForMonth), attendenceDetail.ForMonth.ToString());

            if (Convert.ToDateTime(attendenceDetail.AttendenceFromDay).Subtract(DateTime.UtcNow).TotalDays > 0)
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
                    if (Convert.ToDateTime(attendenceDetail.AttendenceToDay?.Date).Subtract(employee.CreatedOn.Date).TotalDays < 0)
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

                if (this.IsRegisteredOnPresentMonth(employee.CreatedOn) == 1)
                {
                    attendenceDetails = attendenceDetails.Where(x => employee.CreatedOn.Date.Subtract(x.AttendanceDay.Date).TotalDays <= 0).ToList();
                }
            }

            return new AttendanceWithClientDetail { EmployeeDetail = employee, AttendacneDetails = attendenceDetails.OrderByDescending(x => x.AttendanceDay).ToList() };
        }

        public List<AttendenceDetail> GetAllPendingAttendanceByUserIdService(long employeeId, int UserTypeId, long clientId)
        {
            List<AttendenceDetail> attendanceSet = new List<AttendenceDetail>();
            DateTime current = DateTime.UtcNow;

            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(employeeId, typeof(long), "_EmployeeId"),
                new DbParam(UserTypeId == 0 ? _currentSession.CurrentUserDetail.UserTypeId : UserTypeId, typeof(int), "_UserTypeId"),
                new DbParam(current.Year, typeof(int), "_ForYear"),
                new DbParam(current.Month, typeof(int), "_ForMonth")
            };

            var Result = _db.GetDataset("sp_attendance_detall_pending", dbParams);
            if (Result.Tables.Count == 1 && Result.Tables[0].Rows.Count > 0)
            {
                var currentAttendance = Converter.ToType<Attendance>(Result.Tables[0]);
                attendanceSet = JsonConvert.DeserializeObject<List<AttendenceDetail>>(currentAttendance.AttendanceDetail);
            }

            return attendanceSet;
        }

        public List<AttendenceDetail> InsertUpdateTimesheet(List<AttendenceDetail> attendenceDetail)
        {
            string result = string.Empty;
            var firstItem = attendenceDetail.FirstOrDefault();
            if (firstItem == null)
            {
                throw new HiringBellException("Invalid AttendanceDetail submitted.");
            }

            var invalidAttendaceData = attendenceDetail.FindAll(x => x.EmployeeUid <= 0 || x.ClientId <= 0);
            if (invalidAttendaceData.Count > 0)
                throw new HiringBellException("Invalid Employee/Client Id passed.");

            DateTime firstDate = firstItem.AttendanceDay.AddYears(2);
            DateTime lastDate = firstItem.AttendanceDay.AddYears(-2);
            int j = 0;
            while (j < attendenceDetail.Count)
            {
                var x = attendenceDetail.ElementAt(j);
                if ((x.AttendanceDay - firstDate).TotalDays < 0)
                    firstDate = _timezoneConverter.ToIstTime(x.AttendanceDay);

                if ((x.AttendanceDay - lastDate).TotalDays > 0)
                    lastDate = _timezoneConverter.ToIstTime(x.AttendanceDay);

                j++;
            }

            ValidateDateOfAttendanceSubmission(firstDate, lastDate);

            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(firstItem.EmployeeUid, typeof(int), "_EmployeeId"),
                new DbParam(firstItem.ClientId, typeof(int), "_ClientId"),
                new DbParam(firstItem.UserTypeId, typeof(int), "_UserTypeId"),
                new DbParam(firstItem.ForYear, typeof(int), "_ForYear"),
                new DbParam(firstItem.ForMonth, typeof(int), "_ForMonth")
            };

            var Result = _db.GetDataset("sp_employee_timesheet_get", dbParams);
            if (Result.Tables.Count != 2 && Result.Tables[0].Rows.Count == 0)
            {
                throw new HiringBellException("Attendance detail is invalid.");
            }

            List<AttendenceDetail> otherMonthAttendanceDetail = new List<AttendenceDetail>();
            List<AttendenceDetail> finalAttendanceSet = new List<AttendenceDetail>();

            int status = 1;
            Attendance currentAttendance = null;
            if (Result.Tables[0].Rows.Count > 0)
            {
                currentAttendance = new Attendance();
                currentAttendance = Converter.ToType<Attendance>(Result.Tables[0]);
                finalAttendanceSet = JsonConvert.DeserializeObject<List<AttendenceDetail>>(currentAttendance.AttendanceDetail);
                status = this.IsGivenDateAllowed(firstDate, lastDate, finalAttendanceSet);
            }
            else
            {
                status = 0;
                var currentMonthDateTime = _timezoneConverter.ToIstTime(DateTime.UtcNow);
                int totalDays = DateTime.DaysInMonth(currentMonthDateTime.Year, currentMonthDateTime.Month);
                currentAttendance = new Attendance
                {
                    EmployeeId = firstItem.EmployeeUid,
                    AttendanceId = 0,
                    ForYear = currentMonthDateTime.Year,
                    AttendanceDetail = "[]",
                    UserTypeId = (int)UserType.Employee,
                    TotalDays = totalDays,
                    TotalWeekDays = (int)_timezoneConverter.GetBusinessDays(
                                        _timezoneConverter.GetUtcFirstDay(currentMonthDateTime.Year, currentMonthDateTime.Month),
                                        _timezoneConverter.GetUtcDateTime(currentMonthDateTime.Year, currentMonthDateTime.Month, totalDays)),
                    DaysPending = totalDays,
                    ForMonth = currentMonthDateTime.Month
                };

                firstItem.AttendenceFromDay = firstDate;
                firstItem.AttendenceToDay = lastDate;
                finalAttendanceSet = this.GenerateWeekAttendaceData(firstItem, status);
            }

            Employee employee = new Employee();
            if (Result.Tables[1].Rows.Count > 0)
            {
                employee = Converter.ToType<Employee>(Result.Tables[1]);
            }
            else
            {
                throw new HiringBellException("User detail not found.");
            }

            if (finalAttendanceSet == null)
            {
                throw new HiringBellException("Unable to get record. Please contact to admin.");
            }

            int i = 0;
            int dayStatus = (int)DayStatus.Empty;
            DateTime clientKindDateTime = DateTime.Now;
            while (i < attendenceDetail.Count)
            {
                var x = attendenceDetail.ElementAt(i);
                x.AttendanceDay = _timezoneConverter.ToIstTime(x.AttendanceDay);
                var item = finalAttendanceSet.Find(i => i.AttendanceDay.Subtract(x.AttendanceDay).TotalDays == 0);
                if (item != null)
                {
                    clientKindDateTime = _timezoneConverter.ToIstTime(x.AttendanceDay);
                    switch (clientKindDateTime.DayOfWeek)
                    {
                        case DayOfWeek.Sunday:
                        case DayOfWeek.Saturday:
                            dayStatus = (int)DayStatus.Weekend;
                            break;
                        default:
                            dayStatus = item.PresentDayStatus;
                            break;
                    }
                    item.TotalMinutes = x.TotalMinutes;
                    item.UserTypeId = x.UserTypeId;
                    item.EmployeeUid = x.EmployeeUid;
                    item.PresentDayStatus = dayStatus;
                    item.IsOnLeave = false;
                    item.AttendanceId = firstItem.AttendanceId;
                    item.UserComments = x.UserComments;
                    item.AttendanceDay = x.AttendanceDay;
                    item.AttendenceStatus = x.AttendenceStatus;
                    item.ClientId = x.ClientId;
                }
                else
                {
                    if (x.AttendanceDay.Month != currentAttendance.ForMonth)
                    {
                        x.AttendanceId = -1;
                        x.ForMonth = x.AttendanceDay.Month;
                        x.ForYear = x.AttendanceDay.Year;
                        x.PresentDayStatus = dayStatus;
                        x.IsOnLeave = false;
                        x.PresentDayStatus = 1;
                        otherMonthAttendanceDetail.Add(x);
                    }
                    else
                    {
                        finalAttendanceSet.Add(new AttendenceDetail
                        {
                            TotalMinutes = x.TotalMinutes,
                            UserTypeId = x.UserTypeId,
                            EmployeeUid = x.EmployeeUid,
                            PresentDayStatus = dayStatus,
                            IsOnLeave = false,
                            AttendanceId = firstItem.AttendanceId,
                            UserComments = x.UserComments,
                            AttendanceDay = x.AttendanceDay,
                            AttendenceStatus = x.AttendenceStatus,
                            ClientId = x.ClientId
                        });
                    }
                }

                i++;
            }

            if (this.IsRegisteredOnPresentWeek(employee.CreatedOn) == 1)
            {
                finalAttendanceSet = finalAttendanceSet.Where(x => employee.CreatedOn.Date.Subtract(x.AttendanceDay.Date).TotalDays <= 0).ToList();
            }

            result = this.UpdateOrInsertAttendanceDetail(finalAttendanceSet, currentAttendance, ApplicationConstants.InserUpdateAttendance);
            if (string.IsNullOrEmpty(result))
            {
                throw new HiringBellException("Unable to insert/update record. Please contact to admin.");
            }

            List<AttendenceDetail> attendenceDetails = new List<AttendenceDetail>();
            finalAttendanceSet.ForEach(x =>
            {
                if (x.AttendanceDay >= TimeZoneInfo.ConvertTimeToUtc(firstDate) && x.AttendanceDay <= TimeZoneInfo.ConvertTimeToUtc(lastDate))
                {
                    attendenceDetails.Add(x);
                }
            });

            if (otherMonthAttendanceDetail.Count > 0)
            {
                currentAttendance.ForMonth = otherMonthAttendanceDetail.First().ForMonth;
                currentAttendance.ForYear = otherMonthAttendanceDetail.First().ForYear;

                if (this.IsRegisteredOnPresentWeek(employee.CreatedOn) == 1)
                {
                    otherMonthAttendanceDetail = otherMonthAttendanceDetail.Where(x => employee.CreatedOn.Date.Subtract(x.AttendanceDay.Date).TotalDays <= 0).ToList();
                }

                result = this.UpdateOrInsertAttendanceDetail(otherMonthAttendanceDetail, currentAttendance, "sp_attendance_insupd_by_monthandyear");
                if (string.IsNullOrEmpty(result))
                {
                    throw new HiringBellException("Unable to insert/update record. Please contact to admin.");
                }

                otherMonthAttendanceDetail.ForEach(x =>
                {
                    if (x.AttendanceDay >= TimeZoneInfo.ConvertTimeToUtc(firstDate) && x.AttendanceDay <= TimeZoneInfo.ConvertTimeToUtc(lastDate))
                    {
                        attendenceDetails.Add(x);
                    }
                });
            }

            return attendenceDetails;
        }

        private DateTime GetPreviousThreeWorkingDaysBackDate()
        {
            int i = 4;
            DateTime todayDate = DateTime.UtcNow.Date;
            while (true)
            {
                todayDate = todayDate.AddDays(-1);
                switch (todayDate.DayOfWeek)
                {
                    case DayOfWeek.Saturday:
                    case DayOfWeek.Sunday:
                        break;
                    default:
                        i--;
                        break;
                }

                if (i == 0)
                    break;
            }

            if (i > 0)
                todayDate = todayDate.AddDays(-1 * i);

            return todayDate;
        }

        public string SubmitAttendanceService(AttendenceDetail commentDetails)
        {
            string Result = string.Empty;
            bool flag = false;
            Employee employee = _db.Get<Employee>("SP_Employees_ById", new
            {
                EmployeeId = _currentSession.CurrentUserDetail.UserId,
                IsActive = 1
            });

            if (employee == null)
            {
                throw new HiringBellException("No employee found. Please login again.");
            }

            DateTime barrierDate = this.GetPreviousThreeWorkingDaysBackDate();
            if (commentDetails.AttendanceDay.Subtract(barrierDate).TotalDays >= 0)
            {
                int totalDays = DateTime.DaysInMonth(commentDetails.AttendanceDay.Year, commentDetails.AttendanceDay.Month);
                var currentAttendence = new Attendance
                {
                    EmployeeId = commentDetails.EmployeeUid,
                    AttendanceId = 0,
                    ForYear = commentDetails.AttendanceDay.Year,
                    AttendanceDetail = "[]",
                    UserTypeId = (int)UserType.Employee,
                    TotalDays = totalDays,
                    TotalWeekDays = (int)_timezoneConverter.GetBusinessDays(
                                        _timezoneConverter.GetUtcFirstDay(commentDetails.AttendanceDay.Year, commentDetails.AttendanceDay.Month),
                                        _timezoneConverter.GetUtcDateTime(commentDetails.AttendanceDay.Year, commentDetails.AttendanceDay.Month, totalDays)),
                    DaysPending = totalDays,
                    ForMonth = commentDetails.AttendanceDay.Month
                };

                DateTime requestedDate = (DateTime)commentDetails.AttendenceFromDay;
                var attendanceList = new List<AttendenceDetail>();
                DbParam[] dbParams = new DbParam[]
                {
                    new DbParam(commentDetails.EmployeeUid, typeof(long), "_EmployeeId"),
                    new DbParam(commentDetails.UserTypeId, typeof(int), "_UserTypeId"),
                    new DbParam(commentDetails.AttendanceDay.Month, typeof(int), "_ForMonth"),
                    new DbParam(commentDetails.AttendanceDay.Year, typeof(int), "_ForYear")
                };

                var result = _db.GetDataset("Sp_Attendance_GetById", dbParams);
                if (result.Tables.Count == 1 && result.Tables[0].Rows.Count > 0)
                {
                    currentAttendence = Converter.ToType<Attendance>(result.Tables[0]);
                    attendanceList = JsonConvert.DeserializeObject<List<AttendenceDetail>>(currentAttendence.AttendanceDetail);

                    if (attendanceList.Count == 0)
                    {
                        flag = true;
                        int status = this.IsGivenDateAllowed((DateTime)commentDetails.AttendenceFromDay, (DateTime)commentDetails.AttendenceToDay, attendanceList);
                        DateTime now = (DateTime)commentDetails.AttendenceToDay;
                        var monthFirstDate = _timezoneConverter.ToUtcTime(new DateTime(now.Year, now.Month, 1));
                        attendanceList = this.GenerateWeekAttendaceData(commentDetails, status, monthFirstDate);

                        if (this.IsRegisteredOnPresentWeek(employee.CreatedOn) == 1)
                        {
                            attendanceList = attendanceList.Where(x => employee.CreatedOn.Date.Subtract(x.AttendanceDay.Date).TotalDays <= 0).ToList();
                        }
                    }
                }
                else
                {
                    int status = this.IsGivenDateAllowed((DateTime)commentDetails.AttendenceFromDay, (DateTime)commentDetails.AttendenceToDay, attendanceList);
                    DateTime now = (DateTime)commentDetails.AttendenceToDay;
                    var monthFirstDate = _timezoneConverter.ToUtcTime(new DateTime(now.Year, now.Month, 1));
                    attendanceList = this.GenerateWeekAttendaceData(commentDetails, status, monthFirstDate);
                }

                AttendenceDetail attendanceOn = null;
                if (employee.CreatedOn.Year == commentDetails.AttendanceDay.Year && employee.CreatedOn.Month == commentDetails.AttendanceDay.Month)
                {
                    attendanceList = attendanceList
                        .Where(x => x.AttendanceDay.Subtract(employee.CreatedOn).TotalDays >= 0)
                        .ToList();
                }

                while (requestedDate.Date.Subtract(((DateTime)commentDetails.AttendenceToDay).Date).TotalDays <= 0)
                {
                    attendanceOn = attendanceList.Find(x => x.AttendanceDay.Date.Subtract(requestedDate.Date).TotalDays == 0);

                    if (attendanceOn == null)
                    {
                        attendanceOn = new AttendenceDetail
                        {
                            AttendanceDay = commentDetails.AttendanceDay,
                            PresentDayStatus = (int)ItemStatus.Pending,
                            AttendanceId = 0,
                            AttendenceFromDay = commentDetails.AttendenceToDay,
                            AttendenceStatus = (int)ItemStatus.Pending,
                            AttendenceToDay = commentDetails.AttendenceToDay,
                            BillingHours = 9 * 5,
                            EmployeeUid = commentDetails.EmployeeUid,
                            ForMonth = commentDetails.ForMonth,
                            ForYear = commentDetails.ForYear,
                            IsActiveDay = true,
                            IsHoliday = false,
                            IsOnLeave = false,
                            IsOpen = false,
                            SubmittedBy = _currentSession.CurrentUserDetail.UserId,
                            SubmittedOn = DateTime.Now,
                            UserComments = commentDetails.UserComments,
                            UserId = commentDetails.EmployeeUid,
                            UserTypeId = (int)UserType.Employee
                        };

                        attendanceList.Add(attendanceOn);
                    }
                    else
                    {
                        attendanceOn.PresentDayStatus = (int)DayStatus.WorkFromHome;
                        attendanceOn.UserComments = commentDetails.UserComments;
                    }

                    if (flag) break;
                    requestedDate = requestedDate.AddDays(1);
                }

                var AttendaceDetail = JsonConvert
                                        .SerializeObject((
                                            from n in attendanceList
                                            select new AttendanceDetail
                                            {
                                                TotalMinutes = n.TotalMinutes,
                                                UserTypeId = n.UserTypeId,
                                                PresentDayStatus = n.PresentDayStatus,
                                                EmployeeUid = n.EmployeeUid,
                                                AttendanceId = n.AttendanceId,
                                                UserComments = n.UserComments,
                                                AttendanceDay = n.AttendanceDay,
                                                AttendenceStatus = n.AttendenceStatus,
                                                Email = employee.Email,
                                                EmployeeName = string.Concat(employee.FirstName, 
                                                                " ",
                                                                employee.LastName).Trim(),
                                                Mobile = employee.Mobile,
                                                ReportingManagerId = employee.ReportingManagerId,
                                                ManagerName = "NA"
                                            }));

                double MonthsMinutes = 0;
                currentAttendence.DaysPending = 0;
                attendanceList.ForEach(x =>
                {
                    MonthsMinutes += x.TotalMinutes;
                    if (x.AttendenceStatus == 8)
                        currentAttendence.DaysPending++;
                });


                Result = _db.Execute<Attendance>("sp_attendance_update_request", new
                {
                    AttendanceId = currentAttendence.AttendanceId,
                    AttendanceDetail = AttendaceDetail,
                    UserTypeId = UserType.Employee,
                    EmployeeId = commentDetails.EmployeeUid,
                    TotalDays = currentAttendence.TotalDays,
                    TotalWeekDays = currentAttendence.TotalWeekDays,
                    DaysPending = currentAttendence.DaysPending,
                    TotalBurnedMinutes = MonthsMinutes,
                    ForYear = currentAttendence.ForYear,
                    ForMonth = currentAttendence.ForMonth,
                    UserI = _currentSession.CurrentUserDetail.UserId
                }, true);
            }
            else
            {
                Result = "Ops!!! You are not allow to submit this date attendace. Please raise a request to your direct manager.";
            }

            return Result;
        }

        public AttendanceWithClientDetail EnablePermission(AttendenceDetail attendenceDetail)
        {
            if (attendenceDetail.ForMonth <= 0)
                throw new HiringBellException("Invalid month num. passed.", nameof(attendenceDetail.ForMonth), attendenceDetail.ForMonth.ToString());

            if (Convert.ToDateTime(attendenceDetail.AttendenceFromDay).Subtract(DateTime.UtcNow).TotalDays > 0)
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
            return null;
        }

        public async Task<dynamic> ApplyLeaveService(LeaveRequestDetail leaveDetail)
        {
            if (leaveDetail.LeaveFromDay == null || leaveDetail.LeaveToDay == null)
                throw new HiringBellException("Invalid From and To date passed.");

            await _leaveCalculation.CheckAndApplyForLeave(leaveDetail);

            return this.GetEmployeeLeaveDetail(new ApplyLeave
            {
                EmployeeId = _currentSession.CurrentUserDetail.UserId,
                FromDate = leaveDetail.LeaveFromDay,
                ToDate = leaveDetail.LeaveToDay
            });
        }

        #region LeaveCalculation And Assignment

        public async Task<dynamic> GetEmployeeLeaveDetail(ApplyLeave applyLeave)
        {
            if (applyLeave.EmployeeId < 0)
                throw new HiringBellException("Invalid employee id.");

            var leaveCalculationModal = await _leaveCalculation.GetBalancedLeave(applyLeave.EmployeeId, DateTime.Now, DateTime.Now);

            return new
            {
                LeavePlanTypes = leaveCalculationModal.leavePlanTypes,
                EmployeeLeaveDetail = leaveCalculationModal.leaveRequestDetail,
                Employee = leaveCalculationModal.employee
            };
        }

        public async Task<List<LeavePlanType>> ApplyLeaveService_Testing(ApplyLeave applyLeave)
        {
            if (applyLeave.EmployeeId < 0)
                throw new HiringBellException("Invalid employee id.");

            List<LeavePlanType> leavePlanTypes = null;
            //leavePlanTypes = await _leaveCalculation.GetBalancedLeave(applyLeave.EmployeeId, Convert.ToDateTime("2022-10-10"), Convert.ToDateTime("2022-10-15"));
            LeaveRequestDetail leaveDetail = new LeaveRequestDetail
            {
                EmployeeId = 4,
                LeaveType = 1,
                LeaveFromDay = Convert.ToDateTime("2022-10-22"),
                LeaveToDay = Convert.ToDateTime("2022-10-22")
            };

            await _leaveCalculation.CheckAndApplyForLeave(leaveDetail);

            return leavePlanTypes;
        }

        #endregion

        private List<AttendenceDetail> GenerateWeekAttendaceData(AttendenceDetail attendenceDetail, int isOpen, DateTime? monthFirstDate = null)
        {
            List<AttendenceDetail> attendenceDetails = new List<AttendenceDetail>();
            DateTime startDate = (DateTime)attendenceDetail.AttendenceFromDay;
            if (monthFirstDate != null)
                startDate = (DateTime)monthFirstDate;

            var endDate = (DateTime)attendenceDetail.AttendenceToDay;

            while (startDate.Subtract(endDate).TotalDays <= 0)
            {
                attendenceDetails.Add(new AttendenceDetail
                {
                    IsActiveDay = false,
                    TotalDays = DateTime.DaysInMonth(startDate.Year, startDate.Month),
                    AttendanceDay = startDate,
                    AttendanceId = 0,
                    AttendenceStatus = (int)ItemStatus.Pending,
                    BillingHours = 480,
                    ClientId = attendenceDetail.ClientId,
                    DaysPending = DateTime.DaysInMonth(startDate.Year, startDate.Month),
                    EmployeeUid = attendenceDetail.EmployeeUid,
                    ForMonth = startDate.Month,
                    ForYear = startDate.Year,
                    TotalMinutes = 480,
                    IsHoliday = (startDate.DayOfWeek == DayOfWeek.Saturday
                                    ||
                                startDate.DayOfWeek == DayOfWeek.Sunday) ? true : false,
                    IsOnLeave = false,
                    LeaveId = 0,
                    UserComments = string.Empty,
                    UserTypeId = (int)UserType.Employee,
                    IsOpen = isOpen == 1 ? true : false
                });

                startDate = startDate.AddDays(1);
            }

            return attendenceDetails;
        }

        private int IsRegisteredOnPresentWeek(DateTime RegistratedOn)
        {
            var weekFirstDate = _timezoneConverter.FirstDayOfWeekIST();
            var weekLastDate = _timezoneConverter.LastDayOfWeekIST();

            if (RegistratedOn.Date.Subtract(weekFirstDate.Date).TotalDays >= 0 && RegistratedOn.Date.Subtract(weekLastDate.Date).TotalDays <= 0)
            {
                return 1;
            }

            return 0;
        }

        private int IsRegisteredOnPresentMonth(DateTime RegistratedOn)
        {
            var monthFirstDay = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            if (RegistratedOn.Date.Subtract(monthFirstDay.Date).TotalDays <= 0)
            {
                return 0;
            }

            return 1;
        }

        private int IsGivenDateAllowed(DateTime From, DateTime To, List<AttendenceDetail> attendanceData)
        {
            var startDate = _timezoneConverter.ToIstTime(From);
            var endDate = _timezoneConverter.ToIstTime(To);
            var weekFirstDate = _timezoneConverter.FirstDayOfWeekIST();
            var weekLastDate = _timezoneConverter.LastDayOfWeekIST();

            if (startDate.Date.Subtract(weekFirstDate.Date).TotalDays == 0 && endDate.Date.Subtract(weekLastDate.Date).TotalDays == 0)
            {
                return 1;
            }
            else
            {
                if (attendanceData == null)
                    return 0;

                var workingWeek = attendanceData.Where(x => x.AttendanceDay.Date.Subtract(From.Date).TotalDays == 0).FirstOrDefault();
                if (workingWeek == null || !workingWeek.IsOpen)
                    return 0;

                // if (!workingWeek.IsOpen)
                // {
                //     throw new HiringBellException("Only present week attendance is allowed. For previous week please raise a permission to your manager or HR.");
                // }
            }

            return 0;
        }

        //public void GenerateCurrentMonthAttendaceSheet()
        //{
        //    DbParam[] dbParams = new DbParam[]
        //    {
        //        new DbParam("1=1", typeof(int), "_searchString"),
        //        new DbParam(attendenceDetail.EmployeeUid, typeof(int), "_sortBy"),
        //        new DbParam(attendenceDetail.EmployeeUid, typeof(int), "_pageIndex"),
        //        new DbParam(attendenceDetail.EmployeeUid, typeof(int), "_pageSize")
        //    };

        //    var Result = _db.GetDataset("SP_Employees_Get", dbParams);

        //    List<AttendenceDetail> finalAttendanceSet = new List<AttendenceDetail>();
        //    DateTime firstDay = _timezoneConverter.GetUtcFirstDay(DateTime.Now.Year, DateTime.Now.Month);
        //    int days = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
        //    int i = 0;
        //    while (i < days)
        //    {
        //        finalAttendanceSet.Add(new AttendenceDetail
        //        {
        //            TotalMinutes = 0,
        //            UserTypeId = 0,
        //            EmployeeUid = 0,
        //            IsHoliday = false,
        //            IsOnLeave = false,
        //            AttendanceId = 0,
        //            UserComments = "NA",
        //            AttendanceDay = DateTime.Now.AddDays(i + 1),
        //            AttendenceStatus = 1,
        //            ClientId = 0
        //        });

        //        i++;
        //    }

        //    Attendance attendance = new Attendance
        //    {
        //        AttendanceDetail = null,
        //        AttendanceId = 0,
        //        EmployeeId = 0,
        //        UserTypeId = (int)UserType.Employee,
        //        TotalDays = 0,
        //        TotalWeekDays = 0,
        //        DaysPending = 0,
        //        ForYear = 0,
        //        ForMonth = 0
        //    };

        //    this.UpdateOrInsertAttendanceDetail(finalAttendanceSet, attendance, ApplicationConstants.InserUpdateAttendance);
        //}

        private string UpdateOrInsertAttendanceDetail(List<AttendenceDetail> finalAttendanceSet, Attendance currentAttendance, string procedure)
        {
            var firstAttn = finalAttendanceSet.FirstOrDefault();

            var AttendaceDetail = JsonConvert.SerializeObject((from n in finalAttendanceSet
                                                               select new
                                                               {
                                                                   TotalMinutes = n.TotalMinutes,
                                                                   UserTypeId = n.UserTypeId,
                                                                   PresentDayStatus = n.PresentDayStatus,
                                                                   EmployeeUid = n.EmployeeUid,
                                                                   AttendanceId = n.AttendanceId,
                                                                   UserComments = n.UserComments,
                                                                   AttendanceDay = n.AttendanceDay,
                                                                   AttendenceStatus = n.AttendenceStatus
                                                               }));

            double MonthsMinutes = 0;
            currentAttendance.DaysPending = 0;
            finalAttendanceSet.ForEach(x =>
            {
                MonthsMinutes += x.TotalMinutes;
                if (x.AttendenceStatus == 8)
                    currentAttendance.DaysPending++;
            });

            var dbParams = new DbParam[]
            {
                new DbParam(currentAttendance.AttendanceId, typeof(long), "_AttendanceId"),
                new DbParam(currentAttendance.EmployeeId, typeof(long), "_EmployeeId"),
                new DbParam(currentAttendance.UserTypeId, typeof(int), "_UserTypeId"),
                new DbParam(AttendaceDetail, typeof(string), "_AttendanceDetail"),
                new DbParam(currentAttendance.TotalDays, typeof(int), "_TotalDays"),
                new DbParam(currentAttendance.TotalWeekDays, typeof(int), "_TotalWeekDays"),
                new DbParam(currentAttendance.DaysPending, typeof(int), "_DaysPending"),
                new DbParam(MonthsMinutes, typeof(double), "_TotalBurnedMinutes"),
                new DbParam(currentAttendance.ForYear, typeof(int), "_ForYear"),
                new DbParam(currentAttendance.ForMonth, typeof(int), "_ForMonth"),
                new DbParam(_currentSession.CurrentUserDetail.UserId, typeof(long), "_UserId")
            };

            var result = _db.ExecuteNonQuery(procedure, dbParams, true);
            if (string.IsNullOrEmpty(result))
                return null;
            return result;
        }

        private void ValidateDateOfAttendanceSubmission(DateTime firstDate, DateTime lastDate)
        {
            DateTime now = DateTime.Now;
            DateTime presentDate = _timezoneConverter.GetUtcDateTime(now.Year, now.Month, now.Day);

            // handling future date
            if (presentDate.Subtract(lastDate).TotalDays > 0)
            {
                throw new HiringBellException("Future date's are not allowed.");
            }
            // handling past date
            else if (presentDate.Subtract(firstDate).TotalDays < 0)
            {
                if (_currentSession.CurrentUserDetail.RoleId != (int)UserType.Admin)
                {
                    throw new HiringBellException("Past week's are not allowed.");
                }
            }
        }
    }
}
