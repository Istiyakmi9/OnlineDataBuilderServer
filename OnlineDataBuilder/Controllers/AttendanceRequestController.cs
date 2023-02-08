using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
using System.Threading.Tasks;

namespace OnlineDataBuilder.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceRequestController : BaseController
    {
        private readonly IAttendanceRequestService _requestService;
        public AttendanceRequestController(IAttendanceRequestService requestService)
        {
            _requestService = requestService;
        }

        [HttpGet("GetManagerRequestedData/{employeeId}")]
        public IResponse<ApiResponse> FetchPendingRequests(int employeeId)
        {
            var result = _requestService.FetchPendingRequestService(employeeId);
            return BuildResponse(result);
        }

        [HttpGet("GetAllRequestedData/{employeeId}")]
        [Authorize(Roles = Role.Admin)]
        public IResponse<ApiResponse> GetAllRequestedData(int employeeId)
        {
            var result = _requestService.GetManagerAndUnAssignedRequestService(employeeId);
            return BuildResponse(result);
        }

        [HttpPut("ApprovalAction")]
        public async Task<ApiResponse> ApprovalAction(AttendenceDetail attendanceDetail)
        {
            var result = await _requestService.ApproveAttendanceService(attendanceDetail);
            return BuildResponse(result);
        }

        [HttpPut("RejectAction")]
        public async Task<ApiResponse> RejectAction([FromBody] AttendenceDetail attendanceDetail)
        {
            var result = await _requestService.RejectAttendanceService(attendanceDetail);
            return BuildResponse(result);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPut("ApproveAttendanceRequest/{filterId}")]
        public async Task<ApiResponse> ApproveAttendanceRequest([FromRoute] int filterId, [FromBody] AttendenceDetail attendanceDetail)
        {
            var result = await _requestService.ApproveAttendanceService(attendanceDetail, filterId);
            return BuildResponse(result);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPut("RejectAttendanceRequest/{filterId}")]
        public async Task<ApiResponse> RejectAttendanceRequest([FromRoute] int filterId, [FromBody] AttendenceDetail attendanceDetail)
        {
            var result = await _requestService.RejectAttendanceService(attendanceDetail, filterId);
            return BuildResponse(result);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPut("ReAssigneAttendanceRequest/{filterId}")]
        public IResponse<ApiResponse> ReAssigneToOtherManager(AttendenceDetail attendanceDetail)
        {
            var result = _requestService.ReAssigneAttendanceService(attendanceDetail);
            return BuildResponse(result);
        }
    }
}
