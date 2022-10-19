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
    public class LeaveRequestController : BaseController
    {
        private readonly IAttendanceRequestService _requestService;
        public LeaveRequestController(IAttendanceRequestService requestService)
        {
            _requestService = requestService;
        }

        [HttpGet("GetPendingRequests/{employeeId}/{requestTypeId}")]
        public IResponse<ApiResponse> FetchPendingRequests(int employeeId, int requestTypeId)
        {
            var result = _requestService.FetchPendingRequestService(employeeId, requestTypeId);
            return BuildResponse(result);
        }

        [HttpPut("ApprovalAction/{RequestId}")]
        public IResponse<ApiResponse> ApprovalAction([FromRoute]int RequestId, [FromBody] LeaveRequestNotification approvalRequest)
        {
            var result = _requestService.ApprovalOrRejectActionService(approvalRequest, ItemStatus.Approved, RequestId);
            return BuildResponse(result);
        }

        [HttpPut("RejectAction")]
        public IResponse<ApiResponse> RejectAction(LeaveRequestNotification approvalRequest)
        {
            var result = _requestService.ApprovalOrRejectActionService(approvalRequest, ItemStatus.Rejected, 1);
            return BuildResponse(result);
        }

        [HttpPut("ReAssigneToOtherManager")]
        public IResponse<ApiResponse> ReAssigneToOtherManager(LeaveRequestNotification approvalRequest)
        {
            var result = _requestService.ReAssigneToOtherManagerService(approvalRequest);
            return BuildResponse(result);
        }
    }
}
