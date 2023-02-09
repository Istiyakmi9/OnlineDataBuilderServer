using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Code;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

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

        [HttpPost("SubmitAttendance")]
        public async Task<ApiResponse> SubmitAttendance(AttendenceDetail commentDetails)
        {
            var result = await _attendanceService.SubmitAttendanceService(commentDetails);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        [HttpPost("GetMissingAttendanceRequest")]
        public async Task<ApiResponse> GetMissingAttendanceRequest(FilterModel filter)
        {
            var result = await _attendanceService.GetMissingAttendanceRequestService(filter);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        [HttpPost("RaiseMissingAttendanceRequest")]
        public async Task<ApiResponse> RaiseMissingAttendanceRequest(CompalintOrRequest compalintOrRequest)
        {
            var result = await _attendanceService.RaiseMissingAttendanceRequestService(compalintOrRequest);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        [HttpPost("EnablePermission")]
        public IResponse<ApiResponse> EnablePermission(AttendenceDetail attendenceDetail)
        {
            var result = _attendanceService.EnablePermission(attendenceDetail);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        [HttpPost("GetEmployeePerformance")]
        public IResponse<ApiResponse> GetEmployeePerformance(AttendenceDetail attendenceDetail)
        {
            var result = _attendanceService.GetEmployeePerformanceService(attendenceDetail);
            return BuildResponse(result, HttpStatusCode.OK);
        }

    }
}
