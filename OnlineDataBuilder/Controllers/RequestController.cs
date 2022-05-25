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
    public class RequestController : BaseController
    {
        private readonly IRequestService _requestService;
        public RequestController(IRequestService requestService)
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
        public IResponse<ApiResponse> ApprovalAction(ApprovalRequest approvalRequest)
        {
            var result = _requestService.ApprovalOrRejectActionService(approvalRequest, ItemStatus.Approved);
            return BuildResponse(result);
        }

        [HttpPut("RejectAction")]
        public IResponse<ApiResponse> RejectAction(ApprovalRequest approvalRequest)
        {
            var result = _requestService.ApprovalOrRejectActionService(approvalRequest, ItemStatus.Rejected);
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
