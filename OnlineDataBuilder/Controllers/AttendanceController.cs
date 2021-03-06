using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
using System.Collections.Generic;
using System.Net;

namespace OnlineDataBuilder.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : BaseController
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [HttpPost("InsertUpdateTimesheet")]
        public IResponse<ApiResponse> InsertUpdateTimesheet(List<AttendenceDetail> attendenceDetail)
        {
            var result = _attendanceService.InsertUpdateTimesheet(attendenceDetail);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        [HttpPost("GetAttendanceByUserId")]
        public IResponse<ApiResponse> GetAttendanceByUserId(AttendenceDetail attendenceDetail)
        {
            var result = _attendanceService.GetAttendanceByUserId(attendenceDetail);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        [HttpGet("BuildMonthBlankAttadanceData")]
        public IResponse<ApiResponse> BuildMonthBankAttadanceData()
        {
            return BuildResponse(null, HttpStatusCode.OK);
        }

        [HttpGet("GetPendingAttendanceById/{EmployeeId}/{UserTypeId}/{clientId}")]
        public IResponse<ApiResponse> GetPendingAttendanceById(long employeeId, int UserTypeId, long clientId)
        {
            var result = _attendanceService.GetAllPendingAttendanceByUserIdService(employeeId, UserTypeId, clientId);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        [HttpPost("GetAttendamceById")]
        public IResponse<ApiResponse> GetAttendamceById(AttendenceDetail attendenceDetail)
        {
            var result = _attendanceService.GetAttendamceById(attendenceDetail);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        [HttpPost("SubmitAttendance")]
        public IResponse<ApiResponse> SubmitAttendance(AttendenceDetail commentDetails)
        {
            var result = _attendanceService.SubmitAttendanceService(commentDetails);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        [HttpPost("EnablePermission")]
        public IResponse<ApiResponse> EnablePermission(AttendenceDetail attendenceDetail)
        {
            var result = _attendanceService.EnablePermission(attendenceDetail);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        [HttpPost("ApplyLeave")]
        public IResponse<ApiResponse> ApplyLeave(LeaveDetails leaveDetail)
        {
            var result = _attendanceService.ApplyLeaveService(leaveDetail);
            return BuildResponse(result);
        }

        [HttpPost("GetAllLeavesByEmpId/{EmployeeId}")]
        public IResponse<ApiResponse> GetAllLeavesByEmpId(long EmployeeId, FilterModel filterModel)
        {
            var result = _attendanceService.GetAllLeavesByEmpIdService(EmployeeId, filterModel);
            return BuildResponse(result);
        }
    }
}
