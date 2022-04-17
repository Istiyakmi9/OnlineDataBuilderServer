﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
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

        [HttpPost("InsertUpdateAttendance")]
        public IResponse<ApiResponse> InsertUpdateAttendance(List<AttendenceDetail> attendenceDetail)
        {
            var result = _attendanceService.InsertUpdateAttendance(attendenceDetail);
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
    }
}
