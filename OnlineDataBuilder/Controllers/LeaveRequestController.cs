using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;

namespace OnlineDataBuilder.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveRequestController : BaseController
    {
        private readonly ILeaveRequestService _requestService;
        public LeaveRequestController(ILeaveRequestService requestService)
        {
            _requestService = requestService;
        }

        [HttpPut("ApprovalAction")]
        public IResponse<ApiResponse> ApprovalAction(LeaveRequestDetail leaveRequestDetail)
        {
            var result = _requestService.ApprovalLeaveService(leaveRequestDetail);
            return BuildResponse(result);
        }

        [HttpPut("RejectAction")]
        public IResponse<ApiResponse> RejectAction(LeaveRequestDetail leaveRequestDetail)
        {
            var result = _requestService.RejectLeaveService(leaveRequestDetail);
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
