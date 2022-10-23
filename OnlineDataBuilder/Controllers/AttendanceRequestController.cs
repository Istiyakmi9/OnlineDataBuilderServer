using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;

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

        [Authorize(Roles = Role.Admin)]
        [HttpPut("ApproveAttendance/{filterId}")]
        public IResponse<ApiResponse> ApproveAttendance([FromRoute]int filterId, [FromBody]AttendanceDetails attendanceDetail)
        {
            var result = _requestService.ApproveAttendanceService(filterId, attendanceDetail);
            return BuildResponse(result);
        }

        [HttpPut("ApprovalAction")]
        public IResponse<ApiResponse> ApprovalAction(AttendanceDetails attendanceDetail)
        {
            var result = _requestService.ApprovalAttendanceService(attendanceDetail);
            return BuildResponse(result);
        }

        [HttpPut("RejectAction")]
        public IResponse<ApiResponse> RejectAction(AttendanceDetails attendanceDetail)
        {
            var result = _requestService.RejectAttendanceService(attendanceDetail);
            return BuildResponse(result);
        }

        [HttpPut("ReAssigneToOtherManager")]
        public IResponse<ApiResponse> ReAssigneToOtherManager(AttendanceDetails attendanceDetail)
        {
            var result = _requestService.ReAssigneAttendanceService(attendanceDetail);
            return BuildResponse(result);
        }
    }
}
