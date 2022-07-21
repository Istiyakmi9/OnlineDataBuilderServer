using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal.Leaves;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;

namespace OnlineDataBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveController : BaseController
    {
        private readonly ILeaveService _leaveService;
        public LeaveController(ILeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        [HttpGet("GetAllLeavePlans")]
        public IResponse<ApiResponse> GetAllLeavePlans()
        {
            var result = _leaveService.GetLeavePlansService();
            return BuildResponse(result);
        }

        [HttpPost("AddLeavePlans")]
        public IResponse<ApiResponse> AddLeavePlans(LeavePlan leavePlan)
        {
            var result = _leaveService.AddLeavePlansService(leavePlan);
            return BuildResponse(result);
        }

        [HttpPut("UpdateLeavePlans/{leavePlanId}")]
        public IResponse<ApiResponse> UpdateLeavePlans([FromRoute]int leavePlanId, [FromBody]LeavePlan leavePlan)
        {
            var result = _leaveService.UpdateLeavePlansService(leavePlanId, leavePlan);
            return BuildResponse(result);
        }
    }
}
