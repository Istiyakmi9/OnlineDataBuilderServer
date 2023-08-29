using Confluent.Kafka;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Kafka;
using ModalLayer.Modal;
using Newtonsoft.Json;
using OnlineDataBuilder.ContextHandler;
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
        private readonly ProducerConfig _producerConfig;

        public AttendanceController(IAttendanceService attendanceService,
            ProducerConfig producerConfig)
        {
            _attendanceService = attendanceService;
            _producerConfig = producerConfig;
        }

        [HttpPost("GetAttendanceByUserId")]
        public async Task<ApiResponse> GetAttendanceByUserId(Attendance attendance)
        {
            var result = await _attendanceService.GetAttendanceByUserId(attendance);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        [HttpGet("SendEmailNotification")]
        [AllowAnonymous]
        public async Task<ApiResponse> SendEmailNotification()
        {
            KafkaEmailDetail kafkaEmailDetail = new KafkaEmailDetail
            {
                Body = "Email html tempalate",
                Subject = "Testing"
            };

            var result = JsonConvert.SerializeObject(kafkaEmailDetail);
            using(var producer = new ProducerBuilder<Null, string>(_producerConfig).Build())
            {
                await producer.ProduceAsync("test", new Message<Null, string>
                {
                    Value = result
                });

                producer.Flush(TimeSpan.FromSeconds(10));
            }
            
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
        public async Task<ApiResponse> SubmitAttendance(Attendance attendance)
        {
            var result = await _attendanceService.SubmitAttendanceService(attendance);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        [HttpPost("GetMissingAttendanceRequest")]
        public async Task<ApiResponse> GetMissingAttendanceRequest(FilterModel filter)
        {
            var result = await _attendanceService.GetMissingAttendanceRequestService(filter);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        [HttpPost("GetMissingAttendanceApprovalRequest")]
        public async Task<ApiResponse> GetMissingAttendanceApprovalRequest(FilterModel filter)
        {
            var result = await _attendanceService.GetMissingAttendanceApprovalRequestService(filter);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        [HttpPost("RaiseMissingAttendanceRequest")]
        public async Task<ApiResponse> RaiseMissingAttendanceRequest(ComplaintOrRequestWithEmail compalintOrRequest)
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

        [HttpPut("ApproveRaisedAttendanceRequest")]
        public async Task<ApiResponse> ApproveRaisedAttendanceRequest(List<ComplaintOrRequest> complaintOrRequests)
        {
            var result = await _attendanceService.ApproveRaisedAttendanceRequestService(complaintOrRequests);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        [HttpPut("RejectRaisedAttendanceRequest")]
        public async Task<ApiResponse> RejectRaisedAttendanceRequestService(List<ComplaintOrRequest> complaintOrRequests)
        {
            var result = await _attendanceService.RejectRaisedAttendanceRequestService(complaintOrRequests);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        [HttpGet]
        public async Task<ApiResponse> GenerateAttendance()
        {
            await _attendanceService.GenerateAttendanceService();
            return BuildResponse(ApplicationConstants.Successfull, HttpStatusCode.OK);
        }
    }
}
