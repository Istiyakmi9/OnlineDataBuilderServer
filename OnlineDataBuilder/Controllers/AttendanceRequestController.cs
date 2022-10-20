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

        [HttpGet("GetPendingRequests/{employeeId}/{requestTypeId}")]
        public IResponse<ApiResponse> FetchPendingRequests(int employeeId, int requestTypeId)
        {
            var result = _requestService.FetchPendingRequestService(employeeId, requestTypeId);
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
