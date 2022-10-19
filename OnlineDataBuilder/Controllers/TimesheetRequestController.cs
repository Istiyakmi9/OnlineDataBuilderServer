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
    public class TimesheetRequestController : BaseController
    {
        private readonly ITimesheetRequestService _requestService;
        public TimesheetRequestController(ITimesheetRequestService requestService)
        {
            _requestService = requestService;
        }

        [HttpPut("ApproveTimesheet")]
        public IResponse<ApiResponse> ApproveTimesheet([FromBody] TimesheetDetail approvalRequest)
        {
            var result = _requestService.ApprovalOrRejectActionService(approvalRequest);
            return BuildResponse(result);
        }

        [HttpPut("RejectAction")]
        public IResponse<ApiResponse> RejectAction(ApprovalRequest approvalRequest)
        {
            var result = _requestService.ApprovalOrRejectActionService(approvalRequest, ItemStatus.Rejected, 1);
            return BuildResponse(result);
        }

        [HttpPut("ReAssigneToOtherManager")]
        public IResponse<ApiResponse> ReAssigneToOtherManager(ApprovalRequest approvalRequest)
        {
            var result = _requestService.ReAssigneToOtherManagerService(approvalRequest);
            return BuildResponse(result);
        }
    }
}
